# Design: Unity MCP Server Optimization

## Context

The Unity MCP Server (`com.ivanmurzak.unity.mcp`) provides the bridge between AI assistants (Claude Code, Cursor, etc.) and the Unity Editor. The current implementation is functional but lacks visibility into connection state and request handling, making debugging and monitoring difficult.

**Current Architecture:**
```
AI Client (Claude Code)
    │
    ▼ stdio transport
Unity MCP Server (.exe)
    │
    ▼ SignalR Hub
Unity Plugin (Editor)
    │
    ├── ConnectionManager (manages hub connection)
    ├── MainWindowEditor (UI)
    └── Tool handlers (Assets, Scene, GameObject, etc.)
```

## Goals / Non-Goals

### Goals
- Provide real-time visibility into client connections
- Display request/response history for debugging
- Show health metrics (request rate, latency, errors)
- Validate configuration consistency across files
- Improve connection reliability and error handling

### Non-Goals
- Changing the MCP protocol
- Modifying the external server executable
- Adding new tool capabilities
- Breaking existing client integrations

## Decisions

### Decision 1: In-Editor Metrics Collection

**What**: Collect and display metrics within the Unity Editor, not in the external server.

**Why**: 
- The plugin has full access to all request/response data
- No need to modify the external server executable
- Metrics are available even if server restarts
- Aligns with Unity's editor extension patterns

**Implementation**:
```csharp
public class ConnectionMetrics
{
    public int TotalRequests { get; private set; }
    public int SuccessfulRequests { get; private set; }
    public int FailedRequests { get; private set; }
    public double AverageResponseTimeMs { get; private set; }
    public DateTime LastActivityTime { get; private set; }
    
    private readonly RollingAverage _responseTimeAverage = new(windowSize: 100);
    private readonly RollingCounter _requestsPerMinute = new(TimeSpan.FromMinutes(1));
}
```

### Decision 2: Request History Buffer

**What**: Use a circular buffer to store the last N requests for display.

**Why**:
- Bounded memory usage
- Fast insertion O(1)
- Configurable size (default 100, max 1000)
- Sufficient for debugging recent issues

**Implementation**:
```csharp
public class RequestHistory
{
    private readonly CircularBuffer<RequestLogEntry> _buffer;
    
    public RequestHistory(int capacity = 100)
    {
        _buffer = new CircularBuffer<RequestLogEntry>(capacity);
    }
    
    public void Add(RequestLogEntry entry) => _buffer.Add(entry);
    public IEnumerable<RequestLogEntry> GetAll() => _buffer.AsEnumerable();
}

public record RequestLogEntry(
    string ToolName,
    DateTime Timestamp,
    TimeSpan Duration,
    bool Success,
    string? ErrorMessage,
    int RequestSizeBytes,
    int ResponseSizeBytes
);
```

### Decision 3: UI Structure

**What**: Add collapsible panels to the existing editor window.

**Why**:
- Non-intrusive - users can collapse sections they don't need
- Consistent with Unity's editor UI patterns
- Progressive disclosure of information

**Layout**:
```
┌─────────────────────────────────────┐
│ AI Game Developer                   │
├─────────────────────────────────────┤
│ ▶ Connection Status                 │
│   [●] Connected to localhost:8081   │
│   [Connect] [Disconnect]            │
├─────────────────────────────────────┤
│ ▼ Active Clients (1)                │
│   ┌─────────────────────────────┐   │
│   │ claude-code @ 192.168.1.1   │   │
│   │ Connected: 5m ago           │   │
│   │ Requests: 42                │   │
│   └─────────────────────────────┘   │
├─────────────────────────────────────┤
│ ▼ Health Metrics                    │
│   Requests/min: 12                  │
│   Avg response: 45ms                │
│   Error rate: 0%                    │
├─────────────────────────────────────┤
│ ▼ Request History                   │
│   [Filter: ________] [Clear]        │
│   ┌─────────────────────────────┐   │
│   │ 14:32:15 scene_get_loaded ✓ │   │
│   │ 14:32:14 gameobject_find ✓  │   │
│   │ 14:32:10 assets_refresh ✓   │   │
│   └─────────────────────────────┘   │
├─────────────────────────────────────┤
│ ▼ Configuration                     │
│   Port: 8081 ✓                      │
│   Timeout: 20000ms ✓                │
│   [Validate Paths]                  │
└─────────────────────────────────────┘
```

### Decision 4: Path Validation Approach

**What**: Create a PathValidator that checks all configuration files at startup and on-demand.

**Why**:
- Catch misconfigurations early
- Provide actionable error messages
- Support auto-fix for common issues

**Files to Validate**:
```
.mcp.json
├── mcpServers.Unity-MCP.command → must exist
└── mcpServers.Unity-MCP.args.--port → must match plugin config

.serena/config.yaml
├── mcp.servers.unity.command → must match .mcp.json
└── mcp.servers.unity.args → must match .mcp.json

Package paths
├── Editor/UI/uxml/*.uxml → must resolve
└── Editor/UI/uss/*.uss → must resolve
```

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Memory overhead from history | Bounded circular buffer with configurable size |
| Performance impact of metrics | Minimal - only increments counters on request |
| UI complexity | Collapsible sections, start collapsed by default |
| Breaking existing window | All additions are new panels, no changes to existing |

## Implementation Order

1. **Phase 1: Foundation** (Low risk)
   - Create data structures (RequestLogEntry, ConnectionMetrics)
   - Add PathValidator utility
   - Basic unit tests

2. **Phase 2: UI Panels** (Medium risk)
   - Create UXML templates for new panels
   - Integrate with MainWindowEditor
   - Add USS styling

3. **Phase 3: Integration** (Medium risk)
   - Hook metrics collection into ConnectionManager
   - Add request history tracking
   - Wire up UI to live data

4. **Phase 4: Polish** (Low risk)
   - Add filtering and search
   - Improve error messages
   - Documentation

## Open Questions

1. Should request history persist across domain reloads?
   - **Recommendation**: No - keep it simple, history is for immediate debugging

2. Should we add network traffic inspection (request/response bodies)?
   - **Recommendation**: Optional - add toggle, disabled by default (privacy/performance)

3. Should path validation run automatically on every editor focus?
   - **Recommendation**: No - only on window open and manual button click

## File Locations

| New File | Purpose |
|----------|---------|
| `Editor/Scripts/Data/RequestLogEntry.cs` | Request history data |
| `Editor/Scripts/Data/ConnectionMetrics.cs` | Metrics collection |
| `Editor/Scripts/Utils/PathValidator.cs` | Path validation |
| `Editor/Scripts/Utils/ConfigurationValidator.cs` | Config validation |
| `Editor/UI/uxml/ConnectionInfoPanel.uxml` | Client list UI |
| `Editor/UI/uxml/RequestHistoryPanel.uxml` | History list UI |
| `Editor/UI/uxml/HealthMetricsPanel.uxml` | Metrics display UI |
