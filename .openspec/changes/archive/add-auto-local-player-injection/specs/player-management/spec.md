# Player Management Spec (Delta)

## ADDED Requirements

### Requirement: Injection Registry in LocalPlayerResolver

The `LocalPlayerResolver` SHALL maintain a registry of components that have been auto-injected with local player references.

#### Scenario: Component registered after injection

- **GIVEN** a trigger component has been auto-injected with `GetLocalPlayer`
- **WHEN** the injection system completes
- **THEN** the component reference is registered in `LocalPlayerResolver`
- **AND** the registry tracks the component type and instance ID

#### Scenario: Registry query for injected components

- **GIVEN** a scene with multiple auto-injected components
- **WHEN** a developer queries the injection registry
- **THEN** the system returns all components that received auto-injection
- **AND** the query results include component types, names, and paths

### Requirement: Injection Status API

The `LocalPlayerResolver` SHALL provide an API to query injection status for individual components.

#### Scenario: Check if component was auto-injected

- **GIVEN** a trigger component in the scene
- **WHEN** code calls `LocalPlayerResolver.Instance.IsAutoInjected(component)`
- **THEN** the method returns `true` if the component was auto-injected
- **AND** returns `false` if manually configured or not injected

#### Scenario: Get injection timestamp

- **GIVEN** a component that was auto-injected
- **WHEN** code queries the injection timestamp
- **THEN** the system returns the time when injection occurred
- **AND** the timestamp is in Unity's Time.realtimeSinceStartup format

### Requirement: Manual Override Support

The system SHALL allow manual override of auto-injected references without interference.

#### Scenario: User manually changes injected reference

- **GIVEN** a trigger with auto-injected `GetLocalPlayer` reference
- **WHEN** the user manually changes the "From Character" field in the inspector
- **THEN** the manual value is preserved
- **AND** the system does not revert to auto-injection on scene reload
- **AND** the component is marked as "manually configured" in the registry
