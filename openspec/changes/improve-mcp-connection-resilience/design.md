# Design: Unity MCP Server Connection Resilience

## Context

The Unity MCP server uses SignalR HubConnection over HTTP (default: localhost:8081) with a singleton `ConnectionManager` pattern. Current implementation has multiple fragility points:

1. **Aggressive Timeouts**: 30-second connection timeout, 5-minute server timeout
2. **Fixed Retry Policy**: Always retries after 10 seconds, no backoff
3. **Race Conditions**: Volatile `connectionTask` without proper synchronization
4. **Fire-and-Forget Async**: Reconnection and disposal use unawaited async calls
5. **Single Connection Design**: Singleton pattern allows only one client

**Key Files:**
- `ConnectionManager.cs` (583 lines) - Core connection logic
- `FixedRetryPolicy.cs` (32 lines) - Simple fixed delay retry
- `HubEndpointConnectionBuilder.cs` (88 lines) - HubConnection configuration

## Goals / Non-Goals

**Goals:**
- Reduce connection drops by 90% under normal operating conditions
- Support 2-5 concurrent client connections
- Provide visibility into connection health and client status
- Maintain backward compatibility with existing tool implementations

**Non-Goals:**
- High-performance load balancing across many clients (not a server farm)
- Connection encryption/authentication (assumes trusted localhost environment)
- Horizontal scaling across multiple Unity Editor instances

## Decisions

### Decision 1: Exponential Backoff with Jitter

**What:** Replace `FixedRetryPolicy` with exponential backoff (1s, 2s, 4s, 8s... max 60s) plus random jitter (0-25%).

**Why:**
- Prevents retry storms when server is temporarily unavailable
- Reduces load during recovery
- Jitter prevents synchronized retry from multiple clients

**Implementation:**
```csharp
public class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private readonly TimeSpan _minDelay = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _maxDelay = TimeSpan.FromSeconds(60);
    private readonly double _jitterFactor = 0.25;

    public TimeSpan? NextRetryDelay(RetryContext context)
    {
        var delay = TimeSpan.FromSeconds(Math.Pow(2, context.PreviousRetryCount));
        delay = delay > _maxDelay ? _maxDelay : delay;
        var jitter = delay * (_jitterFactor * Random.Shared.NextDouble());
        return delay + jitter;
    }
}
```

**Alternatives Considered:**
- **Linear backoff**: Simpler but less effective for transient failures
- **Fibonacci backoff**: Similar to exponential but less predictable
- **No change**: Rejected due to observed frequent disconnections

### Decision 2: Circuit Breaker Pattern

**What:** Implement circuit breaker with three states: Closed (normal), Open (failing), Half-Open (testing recovery).

**Why:**
- Prevents wasted retry attempts during extended outages
- Allows system to "fail fast" instead of blocking on timeouts
- Automatic recovery testing when conditions improve

**Implementation:**
```csharp
public class ConnectionCircuitBreaker
{
    private enum State { Closed, Open, HalfOpen }

    private State _state = State.Closed;
    private int _failureCount;
    private DateTime _lastFailureTime;

    private const int FailureThreshold = 5;
    private const int OpenDurationSeconds = 30;

    public bool AllowRequest()
    {
        return _state switch
        {
            State.Closed => true,
            State.HalfOpen => true,  // Allow one test request
            State.Open => DateTime.UtcNow > _lastFailureTime.AddSeconds(OpenDurationSeconds)
                          ? TransitionToHalfOpen()
                          : false
        };
    }

    public void RecordSuccess() => _state = State.Closed;
    public void RecordFailure() { /* increment, check threshold, transition */ }
}
```

**Alternatives Considered:**
- **Polly library**: Too heavy a dependency for this use case
- **Simple retry limit**: Doesn't allow automatic recovery testing

### Decision 3: Connection Pool Architecture

**What:** Replace singleton `ConnectionManager` with `ConnectionPool` managing multiple `ConnectionInstance` objects.

**Why:**
- Enables multiple AI clients to connect simultaneously
- Provides failover capability if one connection degrades
- Better resource utilization through connection reuse

**Architecture:**
```
┌─────────────────────────────────────────────────┐
│ IConnectionPool (new interface)                 │
├─────────────────────────────────────────────────┤
│ + GetConnection(clientId): IConnectionInstance  │
│ + ReleaseConnection(clientId): void             │
│ + GetActiveClients(): IEnumerable<ClientInfo>  │
│ + GetPoolMetrics(): PoolMetrics                 │
└─────────────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────┐
│ ConnectionPool : IConnectionPool                │
├─────────────────────────────────────────────────┤
│ - _connections: ConcurrentDictionary<string,    │
│                 ConnectionInstance>             │
│ - _maxConnections: int (default: 5)            │
│ - _circuitBreaker: ConnectionCircuitBreaker     │
│ - _metrics: ConnectionMetrics                   │
└─────────────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────┐
│ ConnectionInstance                              │
├─────────────────────────────────────────────────┤
│ - _hubConnection: HubConnection                 │
│ - _clientId: string                            │
│ - _connectedAt: DateTime                       │
│ - _lastActivity: DateTime                      │
│ - _requestCount: long                          │
│ - _errorCount: long                            │
└─────────────────────────────────────────────────┘
```

**Connection Lifecycle:**
1. Client connects → `GetConnection(clientId)` called
2. Pool creates new `ConnectionInstance` if under limit
3. If at limit, returns existing connection with highest affinity or rejects
4. Each request updates `_lastActivity` timestamp
5. Idle connections (>10 min) can be cleaned up
6. Client disconnects → `ReleaseConnection(clientId)` marks for cleanup

**Alternatives Considered:**
- **Keep singleton, queue requests**: Simpler but doesn't solve multi-client
- **Multiple ConnectionManager instances**: Memory inefficient, complex DI
- **External connection broker**: Over-engineered for this use case

### Decision 4: Thread-Safe Connection Access

**What:** Replace volatile `connectionTask` with `SemaphoreSlim` for proper synchronization.

**Why:**
- Current volatile variable has race conditions
- Multiple threads can attempt simultaneous reconnection
- Lock-free async patterns require proper synchronization primitives

**Implementation:**
```csharp
private readonly SemaphoreSlim _connectionLock = new(1, 1);

public async Task<bool> ConnectAsync(CancellationToken token)
{
    await _connectionLock.WaitAsync(token);
    try
    {
        if (IsConnected) return true;
        return await InternalConnectAsync(token);
    }
    finally
    {
        _connectionLock.Release();
    }
}
```

**Alternatives Considered:**
- **ReaderWriterLockSlim**: More complex, not needed for this pattern
- **Channel<T>**: Good for request queuing but not connection management
- **Keep volatile**: Rejected due to observed race conditions

### Decision 5: Adjusted Timeout Configuration

**What:** Modify timeout values to be more resilient:

| Timeout | Current | Proposed | Rationale |
|---------|---------|----------|-----------|
| Connection attempt | 30s | 60s | Allow for slow editor startup |
| Server timeout | 5min | 10min | Accommodate long operations |
| Keep-alive | 30s | 15s | Detect failures faster |
| Request timeout | 20s | 30s | Complex tool operations |

**Why:**
- Current timeouts don't account for heavy editor operations
- Keep-alive too slow to detect network issues
- Connection attempt timeout too aggressive for cold start

**Alternatives Considered:**
- **Adaptive timeouts**: Complex, requires baseline measurement
- **User-configurable**: Adds UI complexity, most users won't tune

### Decision 6: Connection Health Monitoring

**What:** Add active health monitoring with periodic checks and metrics collection.

**Why:**
- Detect degraded connections before they fail
- Provide visibility for debugging connection issues
- Enable proactive reconnection when health drops

**Metrics Collected:**
- Connection duration
- Request count and error count
- Average latency (exponential moving average)
- Last activity timestamp
- Circuit breaker state

**Health Check:**
```csharp
private async Task HealthCheckLoop(CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), token);

        foreach (var conn in _connections.Values)
        {
            if (!await conn.PingAsync(TimeSpan.FromSeconds(5)))
            {
                _logger.LogWarning("Connection {ClientId} failed health check", conn.ClientId);
                await conn.ReconnectAsync(token);
            }
        }
    }
}
```

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Breaking change for existing integrations | Maintain `IConnectionManager` interface, add `IConnectionPool` as new preferred interface |
| Increased memory usage with multiple connections | Set max connections to 5, implement idle connection cleanup |
| Complexity increase | Comprehensive unit tests, clear separation of concerns |
| Health check overhead | Make interval configurable, disable by default for low-resource environments |

## Migration Plan

### Phase 1: Non-Breaking Improvements
1. Replace `FixedRetryPolicy` with `ExponentialBackoffRetryPolicy`
2. Add circuit breaker to existing `ConnectionManager`
3. Fix race conditions with `SemaphoreSlim`
4. Adjust timeout configurations

### Phase 2: Multi-Client Support
1. Introduce `IConnectionPool` interface
2. Implement `ConnectionPool` and `ConnectionInstance`
3. Add backward-compatible adapter for `IConnectionManager`
4. Update DI registration with factory pattern

### Phase 3: Monitoring and UI
1. Add `ConnectionMetrics` collection
2. Implement health check loop
3. Add UI panel for connection status
4. Expose events for external monitoring

### Rollback Plan
- Each phase is independently deployable
- Feature flags for new connection pool (`USE_CONNECTION_POOL=false` falls back to singleton)
- Preserve original `ConnectionManager` until Phase 3 is stable

## Open Questions

1. **Max connections**: Should 5 be the default? What happens when limit is reached - reject or queue?
2. **Client identification**: Should clients self-identify or should server assign IDs?
3. **Connection affinity**: Should repeated requests from same client prefer same connection?
4. **Metrics persistence**: Should connection metrics survive editor restart?
