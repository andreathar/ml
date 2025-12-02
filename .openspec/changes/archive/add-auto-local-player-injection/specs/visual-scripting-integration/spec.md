# Visual Scripting Integration Spec (Delta)

## ADDED Requirements

### Requirement: Automatic Local Player Injection for Triggers

The system SHALL automatically inject `GetLocalPlayer` property into GameCreator trigger components when they are created or loaded in a multiplayer context.

#### Scenario: Trigger created in multiplayer scene

- **GIVEN** a scene has `NetworkManager` or `LocalPlayerResolver` present
- **WHEN** a user creates a new GameCreator trigger component
- **AND** the trigger has a "From Character" property field
- **THEN** the system automatically sets the field to use `GetLocalPlayer` property
- **AND** the user sees "Local Player" selected in the inspector without manual intervention

#### Scenario: Existing trigger loaded in multiplayer context

- **GIVEN** a trigger component with null or unset "From Character" field
- **WHEN** the scene is loaded and multiplayer context is detected
- **THEN** the system automatically injects `GetLocalPlayer` reference
- **AND** the trigger functions correctly with the local player

### Requirement: Automatic Local Player Injection for Conditions

The system SHALL automatically inject `GetLocalPlayer` property into GameCreator condition components that reference characters.

#### Scenario: Condition with character comparison

- **GIVEN** a condition component comparing character properties
- **WHEN** the condition is created in a multiplayer scene
- **AND** one of the character fields is unset
- **THEN** the system automatically sets it to `GetLocalPlayer`
- **AND** the condition evaluates against the local player

### Requirement: Automatic Local Player Injection for Instructions

The system SHALL automatically inject `GetLocalPlayer` property into GameCreator instruction components that target characters.

#### Scenario: Instruction targeting player character

- **GIVEN** an instruction component with "Character" or "Target" field
- **WHEN** the instruction is added to a trigger or visual script
- **AND** the multiplayer context is active
- **THEN** the system sets the character field to `GetLocalPlayer`
- **AND** the instruction executes on the local player

### Requirement: Injection Rule Configuration

The system SHALL provide configurable rules for determining when to auto-inject local player references.

#### Scenario: Rule defines field patterns

- **GIVEN** an injection rule configuration
- **WHEN** the rule specifies field names matching "Character" or "From Character"
- **THEN** components with matching fields are auto-injected with `GetLocalPlayer`
- **AND** fields not matching the pattern remain unaffected

#### Scenario: Rule defines component types

- **GIVEN** an injection rule specifying component types
- **WHEN** the rule lists trigger types that should auto-inject
- **THEN** only components matching the specified types are processed
- **AND** other component types are skipped

### Requirement: Multiplayer Context Detection

The system SHALL detect multiplayer context to determine when auto-injection is appropriate.

#### Scenario: NetworkManager presence indicates multiplayer

- **GIVEN** a scene being loaded or a component being created
- **WHEN** the scene contains a `NetworkManager` singleton instance
- **THEN** the multiplayer context is considered active
- **AND** auto-injection rules are applied

#### Scenario: LocalPlayerResolver presence indicates multiplayer

- **GIVEN** a scene being loaded
- **WHEN** `LocalPlayerResolver.Instance` exists and is valid
- **THEN** the multiplayer context is considered active
- **AND** auto-injection proceeds

#### Scenario: Offline mode skips auto-injection

- **GIVEN** a scene without multiplayer components
- **WHEN** triggers and conditions are created
- **THEN** no auto-injection occurs
- **AND** components use default GameCreator behavior
