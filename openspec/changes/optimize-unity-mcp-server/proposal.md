# Change: Optimize Unity MCP Server and Enhance Editor Window

## Why

The Unity MCP Server (`com.ivanmurzak.unity.mcp`) is a critical integration point between AI assistants and the Unity Editor. The current implementation needs:

1. **Enhanced Connection Visibility**: The editor window shows basic connection status but lacks detailed information about:
   - Active client connections (who is connected)
   - Request/response history
   - Tool invocation logs
   - Connection health metrics

2. **Reference Path Consistency**: Configuration paths across `.mcp.json`, Serena config, and the package need verification to ensure:
   - Correct executable paths
   - Matching port configurations
   - Consistent timeout settings

3. **Performance Optimization**: Opportunities exist to improve:
   - Connection pooling and management
   - Request batching for high-frequency operations
   - Memory usage during reflection operations

## What Changes

### 1. Editor Window Enhancements

**Current State:**
- Basic connection status indicator (connected/disconnected/connecting)
- Host URL field
- Timeout configuration

**Proposed Additions:**
- **Active Connections Panel**: Display connected clients with:
  - Client ID/name
  - Connection timestamp
  - Last activity time
  - Requests served count
  
- **Request History Log**: Scrollable list showing:
  - Tool name invoked
  - Timestamp
  - Duration
  - Success/failure status
  - Request/response size

- **Health Metrics Dashboard**:
  - Requests per minute
  - Average response time
  - Error rate
  - Memory usage

### 2. Path Reference Verification

**Files to Synchronize:**
| File | Current Path | Required Check |
|------|--------------|----------------|
| `.mcp.json` | `Library/mcp-server/win-x64/unity-mcp-server.exe` | Verify exists |
| `.serena/config.yaml` | Unity MCP server path | Match .mcp.json |
| Package UXML paths | Multiple fallback paths | Verify resolution |

### 3. Server Optimization

**ConnectionManager Improvements:**
- Add connection pooling with configurable limits
- Implement request queuing with priority support
- Add circuit breaker pattern for failing connections

**MainThread Dispatcher:**
- Optimize batch processing of queued actions
- Add performance metrics collection

### 4. Configuration Consistency

Create unified configuration validation that ensures:
- Port numbers match across all config files
- Timeout values are consistent
- Paths are valid and accessible

## Impact

- **Affected package:** `Packages/local.com.ivanmurzak.unity.mcp/`
- **Affected files:**
  - `Editor/Scripts/UI/Window/MainWindowEditor.*.cs`
  - `Editor/UI/uxml/AiConnectorWindow.uxml`
  - `Editor/UI/uss/AiConnectorWindow.uss`
  - `Unity-MCP-Common/src/Connection/ConnectionManager.cs`
  - `Runtime/Utils/MainThread.Editor.cs`
- **External configs:** `.mcp.json`, `.serena/config.yaml`
- **Risk:** Medium - UI changes are additive, core connection logic changes need careful testing
- **BREAKING:** None - all changes are additive enhancements
