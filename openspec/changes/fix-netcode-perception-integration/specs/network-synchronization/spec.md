## ADDED Requirements

### Requirement: Network Noise Emission

The system SHALL provide network-synchronized noise emission that propagates noise events to all connected clients.

#### Scenario: Player emits noise on landing

- **WHEN** a networked player lands and triggers an OnLand event with InstructionNetworkEmitNoise
- **THEN** all NPCs within the noise radius on all clients SHALL receive the noise through their SensorHear
- **AND** the server SHALL validate the noise parameters before broadcasting

#### Scenario: Server validates noise parameters

- **WHEN** a client requests noise emission via ServerRpc
- **THEN** the server SHALL validate that radius is between 0.1 and 100
- **AND** intensity is between 0 and 10
- **AND** rate limit to maximum 10 noise emissions per second per client

#### Scenario: Noise propagates to all clients

- **WHEN** the server broadcasts a validated noise event
- **THEN** each client SHALL process the noise through their local SpatialHashPerception
- **AND** fire NetworkPerceptionEvents.EventNoiseHeard for UI subscribers

### Requirement: Network Noise Emitter Singleton

The system SHALL provide a NetworkNoiseEmitter component that manages network noise propagation.

#### Scenario: NetworkNoiseEmitter spawns with network session

- **WHEN** a network session starts
- **THEN** a single NetworkNoiseEmitter instance SHALL be spawned
- **AND** accessible via NetworkNoiseEmitter.Instance static property

#### Scenario: Emit noise from any client

- **WHEN** InstructionNetworkEmitNoise executes on any client
- **THEN** it SHALL call NetworkNoiseEmitter.Instance.EmitNoiseServerRpc()
- **AND** the noise SHALL be processed on all clients

### Requirement: Perception Debug UI

The system SHALL provide a real-time debug UI showing perception events across the network.

#### Scenario: Display noise heard events

- **WHEN** any NPC perception receives a noise event
- **THEN** the debug UI SHALL display: "[Time] NPC_Name heard Source_Name [tag] at Xm"

#### Scenario: Display awareness changes

- **WHEN** any NPC perception awareness level changes
- **THEN** the debug UI SHALL display: "[Time] NPC_Name awareness of Target: X.XX (Stage)"

#### Scenario: Display evidence noticed

- **WHEN** any NPC perception notices evidence
- **THEN** the debug UI SHALL display: "[Time] NPC_Name noticed Evidence_Name"

#### Scenario: Filter events

- **WHEN** the user configures filters on the debug UI
- **THEN** only events matching the filter criteria SHALL be displayed
- **AND** filters SHALL include: perception name, event type, max distance

#### Scenario: Toggle UI visibility

- **WHEN** the user presses the configured toggle key (default F8)
- **THEN** the debug UI visibility SHALL toggle

## MODIFIED Requirements

### Requirement: Network Perception Events

The NetworkPerceptionEvents static class SHALL provide events for network-synchronized perception state changes.

#### Scenario: Subscribe to noise heard event

- **WHEN** a subscriber registers for EventNoiseHeard
- **THEN** it SHALL receive callbacks for all noise heard events across the network
- **AND** the callback SHALL include: position, radius, tag, intensity, and source identifier

#### Scenario: Subscribe to awareness changed event

- **WHEN** a subscriber registers for EventAwarenessChanged
- **THEN** it SHALL receive callbacks when any perception's awareness of a target changes
- **AND** the callback SHALL include: perception, target, old awareness, new awareness, stage

#### Scenario: Subscribe to evidence noticed event

- **WHEN** a subscriber registers for EventEvidenceNoticed
- **THEN** it SHALL receive callbacks when any perception notices evidence
- **AND** the callback SHALL include: perception, evidence component
