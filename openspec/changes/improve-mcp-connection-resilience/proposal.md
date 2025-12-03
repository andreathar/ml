# Change: Improve Unity MCP Server Connection Resilience and Multi-Client Support

## Why

The Unity MCP server has fragile connections that disconnect frequently, causing workflow interruptions. The current architecture uses aggressive timeouts (30-second connection attempts), a fixed retry policy without exponential backoff, race conditions in concurrent access, and fire-and-forget async patterns that lead to inconsistent state. Additionally, the singleton design limits the server to a single client connection, preventing multiple AI tools from connecting simultaneously.

## What Changes

### Connection Reliability
- Replace `FixedRetryPolicy` with exponential backoff with jitter
- Implement circuit breaker pattern to prevent retry storms during extended outages
- Add proper thread-safe synchronization for concurrent connection access
- Fix fire-and-forget async patterns in reconnection and disposal logic
- Adjust timeout configurations (increase connection timeout, optimize keep-alive)
- Add connection health monitoring with configurable health check intervals

### Multi-Client Support
- **BREAKING**: Refactor `ConnectionManager` from singleton to support multiple concurrent connections
- Implement connection registry to track and manage multiple connected clients
- Add client identification and session management
- Implement request routing to distribute load across connections (when applicable)

### Monitoring and Observability
- Add connection metrics collection (latency, error rates, connection duration)
- Implement request history tracking with circular buffer
- Add UI panel for viewing connected clients and connection health
- Expose connection events for external monitoring integration

## Impact

- **Affected specs**: `mcp-connection` (new capability spec)
- **Affected code**:
  - `Packages/local.com.ivanmurzak.unity.mcp/Editor/Unity-MCP-Common/src/Connection/ConnectionManager.cs` (major refactor)
  - `Packages/local.com.ivanmurzak.unity.mcp/Editor/Unity-MCP-Common/src/Connection/FixedRetryPolicy.cs` (replace)
  - `Packages/local.com.ivanmurzak.unity.mcp/Editor/Unity-MCP-Common/src/Connection/Endpoint/HubEndpointConnectionBuilder.cs` (timeout adjustments)
  - `Packages/local.com.ivanmurzak.unity.mcp/Editor/Scripts/McpPluginBuilder.cs` (DI changes)
  - `Packages/local.com.ivanmurzak.unity.mcp/Editor/Scripts/UI/Window/MainWindowEditor.cs` (UI additions)
- **Breaking changes**: Singleton `IConnectionManager` becomes multi-connection capable, requiring consumer code to handle connection selection
