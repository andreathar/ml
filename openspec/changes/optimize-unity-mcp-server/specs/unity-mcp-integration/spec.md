# Unity MCP Integration Specification

## ADDED Requirements

### Requirement: Connection Visibility

The Unity MCP Editor Window SHALL display detailed information about active client connections including client identifier, connection timestamp, last activity time, and request count.

#### Scenario: Client connection displayed
- **WHEN** an MCP client connects to the Unity server
- **THEN** the editor window SHALL show the client in the "Active Clients" panel
- **AND** the display SHALL include connection timestamp
- **AND** the display SHALL update last activity on each request

#### Scenario: Client disconnection reflected
- **WHEN** an MCP client disconnects from the Unity server
- **THEN** the client SHALL be removed from the "Active Clients" panel within 5 seconds
- **AND** the total client count SHALL decrease

### Requirement: Request History Logging

The Unity MCP plugin SHALL maintain a circular buffer of recent requests for debugging purposes with configurable capacity.

#### Scenario: Request logged on invocation
- **WHEN** an MCP tool is invoked
- **THEN** a log entry SHALL be created with tool name, timestamp, and duration
- **AND** the entry SHALL indicate success or failure status
- **AND** the entry SHALL be visible in the Request History panel

#### Scenario: Buffer capacity management
- **GIVEN** the request history buffer is at capacity (default 100)
- **WHEN** a new request is logged
- **THEN** the oldest entry SHALL be removed
- **AND** the newest entry SHALL be added
- **AND** total entry count SHALL remain at capacity

### Requirement: Health Metrics Display

The Unity MCP plugin SHALL calculate and display health metrics including requests per minute, average response time, and error rate.

#### Scenario: Requests per minute calculation
- **WHEN** the health metrics panel is visible
- **THEN** requests per minute SHALL be calculated using a 1-minute sliding window
- **AND** the display SHALL update at least every 5 seconds

#### Scenario: Average response time display
- **WHEN** tool invocations complete
- **THEN** average response time SHALL be calculated from the last 100 requests
- **AND** the value SHALL be displayed in milliseconds

#### Scenario: Error rate display
- **WHEN** the health metrics panel is visible
- **THEN** error rate SHALL be calculated as (failed requests / total requests) * 100
- **AND** the percentage SHALL be displayed with one decimal place

### Requirement: Path Validation

The Unity MCP plugin SHALL validate that all configuration file paths are consistent and point to existing resources.

#### Scenario: Executable path validation
- **WHEN** the "Validate Paths" button is clicked
- **THEN** the system SHALL verify the Unity MCP server executable exists at the configured path
- **AND** success/failure status SHALL be displayed for each path

#### Scenario: Configuration consistency check
- **WHEN** path validation is performed
- **THEN** the port number in .mcp.json SHALL be compared to the plugin configuration
- **AND** the timeout value SHALL be compared across configurations
- **AND** mismatches SHALL be highlighted with warning indicators

#### Scenario: UXML/USS asset resolution
- **WHEN** the editor window is opened
- **THEN** all UXML and USS assets SHALL resolve from either package or local paths
- **AND** resolution failures SHALL be logged with actionable error messages

### Requirement: Configuration Synchronization

The Unity MCP plugin SHALL provide a mechanism to synchronize configuration across all related files.

#### Scenario: Port configuration sync
- **WHEN** the port is changed in the editor window
- **THEN** the user SHALL be prompted to update .mcp.json
- **AND** .serena/config.yaml SHALL be flagged for manual update

#### Scenario: Timeout configuration sync
- **WHEN** the timeout is changed in the editor window
- **THEN** the raw JSON configuration display SHALL update immediately
- **AND** the user SHALL be reminded to update MCP client configuration

## MODIFIED Requirements

### Requirement: Connection Status Display

The editor window connection status display SHALL be enhanced to show more granular state information.

#### Scenario: Detailed connection state
- **WHEN** the connection is in reconnecting state
- **THEN** the status text SHALL show "Reconnecting..." instead of just "Connecting..."
- **AND** the number of reconnection attempts SHALL be displayed

#### Scenario: Connection duration display
- **WHEN** connected to the server
- **THEN** the connection duration SHALL be displayed (e.g., "Connected for 5m 32s")
- **AND** the duration SHALL update every second while visible

### Requirement: Error Handling Display

Connection errors SHALL be displayed with actionable troubleshooting guidance.

#### Scenario: Server not found error
- **WHEN** the server executable is not found at the configured path
- **THEN** an error message SHALL be displayed with the expected path
- **AND** a suggestion to check .mcp.json configuration SHALL be shown

#### Scenario: Port conflict error
- **WHEN** the configured port is already in use
- **THEN** an error message SHALL indicate the port conflict
- **AND** a suggestion to use a different port or stop the conflicting process SHALL be shown
