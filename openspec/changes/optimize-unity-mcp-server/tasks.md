# Tasks: Optimize Unity MCP Server and Enhance Editor Window

## 1. Editor Window Connection Info Enhancements
- [ ] 1.1 Add "Active Connections" foldout section to MainWindowEditor
- [ ] 1.2 Create ConnectionInfoPanel.uxml for displaying client details
- [ ] 1.3 Implement client tracking in ConnectionManager (client ID, connect time, last activity)
- [ ] 1.4 Add real-time connection count display to status bar
- [ ] 1.5 Display client transport type (stdio/http) in connection list

## 2. Request History Log
- [ ] 2.1 Create RequestLogEntry data structure for tracking tool invocations
- [ ] 2.2 Add circular buffer for storing last N requests (configurable, default 100)
- [ ] 2.3 Create RequestHistoryPanel.uxml with scrollable list
- [ ] 2.4 Display tool name, timestamp, duration, and status for each request
- [ ] 2.5 Add copy-to-clipboard for request/response JSON
- [ ] 2.6 Implement log filtering by tool name, status, or time range

## 3. Health Metrics Dashboard
- [ ] 3.1 Create HealthMetrics data class with rolling averages
- [ ] 3.2 Add requests-per-minute counter with 1-minute window
- [ ] 3.3 Implement average response time calculation
- [ ] 3.4 Add error rate percentage display
- [ ] 3.5 Create HealthMetricsPanel.uxml with visual indicators
- [ ] 3.6 Add mini-chart for request rate over time (optional)

## 4. Path Reference Verification
- [ ] 4.1 Create PathValidator utility class
- [ ] 4.2 Verify .mcp.json Unity-MCP command path exists
- [ ] 4.3 Compare .serena/config.yaml unity server path with .mcp.json
- [ ] 4.4 Validate package UXML/USS asset paths resolve correctly
- [ ] 4.5 Add validation warning in editor window if paths mismatch
- [ ] 4.6 Create "Validate Paths" button in editor window

## 5. Configuration Consistency
- [ ] 5.1 Create ConfigurationValidator class
- [ ] 5.2 Extract port number from all config files and compare
- [ ] 5.3 Extract timeout values and verify consistency
- [ ] 5.4 Display configuration summary in editor window
- [ ] 5.5 Add "Fix Configuration" button for auto-sync (with confirmation)

## 6. ConnectionManager Optimization
- [ ] 6.1 Add connection state metrics collection
- [ ] 6.2 Implement request timing instrumentation
- [ ] 6.3 Add configurable connection timeout with exponential backoff
- [ ] 6.4 Improve error categorization (network, timeout, server error)
- [ ] 6.5 Add connection health score calculation

## 7. MainThread Dispatcher Optimization
- [ ] 7.1 Profile current MainThread.Editor.cs performance
- [ ] 7.2 Add batch size configuration for action processing
- [ ] 7.3 Implement action queue depth monitoring
- [ ] 7.4 Add timing metrics for dispatched actions

## 8. UI Styling and Polish
- [ ] 8.1 Update AiConnectorWindow.uss with new panel styles
- [ ] 8.2 Add collapsible sections for detailed info
- [ ] 8.3 Implement color-coded status indicators
- [ ] 8.4 Add tooltips for all new UI elements
- [ ] 8.5 Ensure dark/light theme compatibility

## 9. Testing
- [ ] 9.1 Add unit tests for PathValidator
- [ ] 9.2 Add unit tests for ConfigurationValidator
- [ ] 9.3 Add unit tests for HealthMetrics calculations
- [ ] 9.4 Add integration test for connection tracking
- [ ] 9.5 Manual testing of editor window with multiple clients

## 10. Documentation
- [ ] 10.1 Update README.md with new features
- [ ] 10.2 Add inline XML documentation for new public APIs
- [ ] 10.3 Update MCP server inventory in .serena/memories/
- [ ] 10.4 Add troubleshooting section for common path issues
