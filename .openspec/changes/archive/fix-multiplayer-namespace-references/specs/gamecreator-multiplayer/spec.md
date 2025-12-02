## MODIFIED Requirements

### Requirement: PowerGrid Visual Scripting Integration
The PowerGrid visual scripting components (Conditions and Instructions) SHALL properly integrate with GameCreator's visual scripting system by:
1. Inheriting from the correct base classes (`Condition` from `GameCreator.Runtime.VisualScripting` namespace)
2. Using existing GameCreator icon classes from `GameCreator.Runtime.Common` namespace
3. Following GameCreator's established patterns for visual scripting components

#### Scenario: PowerGrid Condition compiles successfully
- **WHEN** Unity compiles the project
- **THEN** all PowerGrid Condition classes compile without "type or namespace not found" errors
- **AND** the `[Image]` attributes reference existing icon classes

#### Scenario: PowerGrid Instruction compiles successfully
- **WHEN** Unity compiles the project
- **THEN** all PowerGrid Instruction classes compile without "type or namespace not found" errors
- **AND** the `[Image]` attributes reference existing icon classes

### Requirement: Network Struct Serialization Compatibility
Network structs used with `NetworkList<T>` SHALL implement `IEquatable<T>` as required by Unity Netcode for GameObjects.

#### Scenario: StatState struct is NetworkList compatible
- **WHEN** `StatState` struct is used in `NetworkList<StatState>`
- **THEN** compilation succeeds
- **AND** the struct implements `IEquatable<StatState>`

#### Scenario: StatusEffectState struct is NetworkList compatible
- **WHEN** `StatusEffectState` struct is used in `NetworkList<StatusEffectState>`
- **THEN** compilation succeeds
- **AND** the struct implements `IEquatable<StatusEffectState>`

### Requirement: RPC Instruction Inheritance
RPC Instructions that inherit from `RPCInstructionBase` SHALL follow the correct override pattern:
1. Do NOT override the sealed `Run(Args args)` method
2. Override `RunLocally(Args args)` for local execution logic
3. Use existing GameCreator icon classes for the `[Image]` attribute

#### Scenario: InstructionRPCToNearby compiles with correct inheritance
- **WHEN** Unity compiles `InstructionRPCToNearby`
- **THEN** compilation succeeds without "cannot override sealed member" errors
- **AND** the instruction properly overrides `RunLocally(Args args)` instead of `Run(Args args)`
