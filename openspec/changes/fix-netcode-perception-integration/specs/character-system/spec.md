## MODIFIED Requirements

### Requirement: Network Character Controller

The NetworkUnitDriverController SHALL move networked characters without conflicting with NetworkTransform position synchronization.

#### Scenario: Height adjustment without position conflict

- **WHEN** a network-spawned character's height changes
- **THEN** the NetworkUnitDriverController SHALL update CharacterController.height
- **AND** SHALL call ApplyMannequinPosition() for visual alignment
- **AND** SHALL NOT directly modify Transform.localPosition

#### Scenario: Radius adjustment

- **WHEN** a network-spawned character's radius changes
- **THEN** the NetworkUnitDriverController SHALL update CharacterController.radius
- **AND** the update SHALL NOT interfere with NetworkTransform

#### Scenario: Remote character animation stability

- **WHEN** viewing a network-spawned character on a remote client
- **THEN** the character's leg animations SHALL remain stable
- **AND** no jittering or erratic movement SHALL occur from physics property updates

#### Scenario: Local character physics

- **WHEN** a character is not network-spawned (single-player or owner client)
- **THEN** the base UnitDriverController behavior SHALL apply
- **AND** position adjustments SHALL be allowed for non-networked scenarios
