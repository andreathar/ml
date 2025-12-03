# Tasks: Improve MCP Connection Resilience

## 1. Phase 1 - Connection Reliability (Non-Breaking)

### 1.1 Retry Policy Improvements
- [x] 1.1.1 Create `ExponentialBackoffRetryPolicy.cs` in `Connection/` folder
- [x] 1.1.2 Implement exponential backoff calculation (1s base, 2x multiplier, 60s cap)
- [x] 1.1.3 Add random jitter (0-25%) to prevent thundering herd
- [x] 1.1.4 Update `HubEndpointConnectionBuilder.cs` to use new retry policy
- [x] 1.1.5 Deprecate `FixedRetryPolicy.cs` (keep for backward compatibility)
- [ ] 1.1.6 Write unit tests for retry delay calculations

### 1.2 Circuit Breaker Implementation
- [x] 1.2.1 Create `ConnectionCircuitBreaker.cs` with State enum (Closed, Open, HalfOpen)
- [x] 1.2.2 Implement failure threshold tracking (5 failures in 2 minutes)
- [x] 1.2.3 Implement state transitions (Closed→Open→HalfOpen→Closed)
- [x] 1.2.4 Add 30-second Open state duration before HalfOpen
- [x] 1.2.5 Integrate circuit breaker into `ConnectionManager.Connect()`
- [ ] 1.2.6 Write unit tests for all state transitions

### 1.3 Thread-Safety Fixes
- [x] 1.3.1 Replace volatile `connectionTask` with `SemaphoreSlim` in `ConnectionManager.cs`
- [x] 1.3.2 Wrap connection logic in `WaitAsync`/`Release` pattern
- [x] 1.3.3 Fix race condition in `StartConnectionLoop()` (lines 500-514)
- [x] 1.3.4 Ensure `_hubConnection` updates are atomic
- [x] 1.3.5 Fix async void patterns - await reconnection in `Closed` event handler
- [ ] 1.3.6 Write concurrency tests using parallel task execution

### 1.4 Timeout Adjustments
- [x] 1.4.1 Update `HubEndpointConnectionBuilder.cs` connection timeout: 30s → 60s
- [x] 1.4.2 Update server timeout: 5min → 10min
- [x] 1.4.3 Update keep-alive interval: 30s → 15s (faster failure detection)
- [x] 1.4.4 Create `ConnectionTimeoutConfig` class for configurable timeouts
- [x] 1.4.5 Update `ConnectionConfig.cs` to include timeout settings
- [x] 1.4.6 Add timeout validation (min/max reasonable values)

### 1.5 Disposal Improvements
- [x] 1.5.1 Fix fire-and-forget `DisposeAsync()` in `ConnectionManager.DisposeAsync()`
- [x] 1.5.2 Add proper cancellation of in-progress reconnection attempts
- [x] 1.5.3 Implement disposal state flag to reject new requests during shutdown
- [x] 1.5.4 Add timeout for disposal operations (30 seconds max)
- [x] 1.5.5 Ensure all IDisposable resources are tracked and disposed

## 2. Phase 2 - Multi-Client Support (Breaking)

### 2.1 Connection Pool Infrastructure
- [ ] 2.1.1 Create `IConnectionPool` interface with `GetConnection`, `ReleaseConnection`, `GetActiveClients`
- [ ] 2.1.2 Create `ConnectionInstance` class wrapping single HubConnection with metadata
- [ ] 2.1.3 Create `ClientInfo` record for client identification (ID, connected time, request count)
- [ ] 2.1.4 Create `ConnectionPool` implementing `IConnectionPool`
- [ ] 2.1.5 Implement `ConcurrentDictionary<string, ConnectionInstance>` for thread-safe storage
- [ ] 2.1.6 Add max connection limit configuration (default: 5)

### 2.2 Connection Lifecycle Management
- [ ] 2.2.1 Implement client registration on first request
- [ ] 2.2.2 Implement connection reuse for existing clients
- [ ] 2.2.3 Add idle connection cleanup (10-minute timeout)
- [ ] 2.2.4 Implement graceful connection handoff during reconnection
- [ ] 2.2.5 Add connection limit enforcement with clear error message
- [ ] 2.2.6 Write integration tests for multi-client scenarios

### 2.3 Backward Compatibility
- [ ] 2.3.1 Create `ConnectionManagerAdapter` implementing `IConnectionManager` using pool
- [ ] 2.3.2 Update `McpPluginBuilder.cs` to register both interfaces
- [ ] 2.3.3 Add feature flag `USE_CONNECTION_POOL` for gradual rollout
- [ ] 2.3.4 Document migration path for existing consumers
- [ ] 2.3.5 Write backward compatibility tests

## 3. Phase 3 - Monitoring and UI

### 3.1 Metrics Collection
- [ ] 3.1.1 Create `ConnectionMetrics` class (duration, requests, errors, latency)
- [ ] 3.1.2 Implement exponential moving average for latency calculation
- [ ] 3.1.3 Add error categorization (timeout, disconnect, protocol)
- [ ] 3.1.4 Create `PoolMetrics` aggregating all connection metrics
- [ ] 3.1.5 Add metrics reset capability for long-running sessions
- [ ] 3.1.6 Implement `IMetricsProvider` interface for external integration

### 3.2 Health Check System
- [ ] 3.2.1 Create `ConnectionHealthChecker` with configurable interval
- [ ] 3.2.2 Implement ping-based health check with 5-second timeout
- [ ] 3.2.3 Add automatic reconnection trigger on health check failure
- [ ] 3.2.4 Create health check event for UI notification
- [ ] 3.2.5 Add configuration to disable health checks (low-resource mode)
- [ ] 3.2.6 Write tests for health check scenarios

### 3.3 UI Enhancements
- [ ] 3.3.1 Add "Connected Clients" panel to `MainWindowEditor`
- [ ] 3.3.2 Display per-client metrics (request count, errors, latency)
- [ ] 3.3.3 Add circuit breaker state indicator
- [ ] 3.3.4 Add connection health status indicator (green/yellow/red)
- [ ] 3.3.5 Create disconnect button per client
- [ ] 3.3.6 Add request history view with circular buffer (last 100 requests)

## 4. Testing and Documentation

### 4.1 Unit Tests
- [ ] 4.1.1 Test exponential backoff delay calculations
- [ ] 4.1.2 Test circuit breaker state transitions
- [ ] 4.1.3 Test thread-safe connection access
- [ ] 4.1.4 Test connection pool lifecycle
- [ ] 4.1.5 Test metrics collection accuracy

### 4.2 Integration Tests
- [ ] 4.2.1 Test actual connection/reconnection with SignalR
- [ ] 4.2.2 Test multi-client concurrent access
- [ ] 4.2.3 Test graceful degradation under load
- [ ] 4.2.4 Test backward compatibility with existing tools

### 4.3 Documentation
- [ ] 4.3.1 Update README with new configuration options
- [ ] 4.3.2 Document multi-client setup and limitations
- [ ] 4.3.3 Add troubleshooting guide for connection issues
- [ ] 4.3.4 Document metrics and their interpretation
