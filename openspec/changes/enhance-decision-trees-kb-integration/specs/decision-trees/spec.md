# Decision Trees Specification Delta

## ADDED Requirements

### Requirement: KB Query Node Type

Decision trees SHALL support a `kb_query` node type that queries the Unity Knowledge Base before providing recommendations.

#### Scenario: KB search before pattern recommendation
- **GIVEN** a decision tree with kb_query node for "character sync"
- **WHEN** AI assistant traverses to that node
- **THEN** system SHALL query Qdrant KB at `http://localhost:6333`
- **AND** search SHALL use configured terms and filters
- **AND** results SHALL influence the next decision path
- **AND** if KB unavailable, fallback to static recommendation

#### Scenario: KB health check before query
- **GIVEN** a kb_query node is about to execute
- **WHEN** system checks KB availability
- **THEN** system SHALL verify `http://localhost:6333/healthz` returns healthy
- **AND** if unhealthy, skip KB query and use fallback path

### Requirement: Project Context Injection

Decision trees SHALL enforce project-specific constraints that prevent invalid recommendations.

#### Scenario: NetworkTransform constraint enforcement
- **GIVEN** decision tree evaluating character sync options
- **WHEN** a path would recommend NetworkTransform
- **THEN** system SHALL block that path
- **AND** redirect to NetworkCharacterAdapter alternative
- **AND** display constraint explanation to user

#### Scenario: Invasive integration pattern enforcement
- **GIVEN** decision tree for GameCreator component sync
- **WHEN** recommending ownership checks
- **THEN** system SHALL recommend `character.IsNetworkOwner` pattern
- **AND** NOT recommend generic `NetworkObject.IsOwner` without context

### Requirement: GameCreator Character Sync Tree

Decision trees SHALL include dedicated guidance for GameCreator Character synchronization.

#### Scenario: Character movement sync decision
- **GIVEN** developer needs to sync character movement
- **WHEN** traversing gc-character-sync.yaml
- **THEN** system SHALL ask about ownership model
- **AND** recommend NetworkCharacterAdapter for position sync
- **AND** reference project implementation at `NetworkCharacterAdapter.cs`

#### Scenario: Character animation sync decision
- **GIVEN** developer needs to sync character animations
- **WHEN** evaluating animation sync options
- **THEN** system SHALL recommend NetworkAnimator component
- **AND** provide GameCreator Animator integration patterns
- **AND** include animation parameter filtering guidance

#### Scenario: Character IK sync decision
- **GIVEN** developer needs to handle IK across network
- **WHEN** evaluating IK sync options
- **THEN** system SHALL recommend local-only IK
- **AND** explain that IK uses local physics queries
- **AND** NOT recommend syncing IK target positions

### Requirement: GameCreator Inventory Sync Tree

Decision trees SHALL include dedicated guidance for GameCreator Inventory synchronization.

#### Scenario: Inventory item sync decision
- **GIVEN** developer needs to sync inventory items
- **WHEN** traversing gc-inventory-sync.yaml
- **THEN** system SHALL ask about persistence requirements
- **AND** recommend appropriate sync method (NetworkVariable vs Supabase hybrid)

#### Scenario: Inventory transaction authority
- **GIVEN** developer implementing item transfers between players
- **WHEN** evaluating transaction authority
- **THEN** system SHALL recommend server-authoritative transactions
- **AND** provide ServerRpc pattern for item validation
- **AND** warn against client-trusted item operations

### Requirement: GameCreator Stats Sync Tree

Decision trees SHALL include dedicated guidance for GameCreator Stats synchronization.

#### Scenario: Stats batching decision
- **GIVEN** developer syncing multiple stats (health, mana, stamina)
- **WHEN** evaluating sync strategy
- **THEN** system SHALL recommend batched NetworkVariable pattern
- **AND** reference StatsBatch struct with INetworkSerializable
- **AND** cite 86% bandwidth reduction benefit

#### Scenario: Stats modifier sync decision
- **GIVEN** developer syncing stat modifiers (buffs, debuffs)
- **WHEN** evaluating modifier sync
- **THEN** system SHALL recommend server-calculated final values
- **AND** NOT recommend syncing individual modifier lists
- **AND** explain server authority for stat calculations

### Requirement: GameCreator Shooter Sync Tree

Decision trees SHALL include dedicated guidance for GameCreator Shooter synchronization.

#### Scenario: Projectile authority decision
- **GIVEN** developer implementing networked projectiles
- **WHEN** evaluating projectile authority
- **THEN** system SHALL ask about projectile type (hitscan vs physics)
- **AND** recommend appropriate sync pattern
- **AND** provide hit validation patterns

#### Scenario: Damage application authority
- **GIVEN** developer implementing damage system
- **WHEN** evaluating damage authority
- **THEN** system SHALL recommend server-authoritative damage
- **AND** provide damage validation ServerRpc pattern
- **AND** warn against client-trusted damage values

### Requirement: Host/Client Architecture Tree

Decision trees SHALL include guidance for Host vs Dedicated Server topology decisions.

#### Scenario: Network topology decision
- **GIVEN** developer setting up multiplayer architecture
- **WHEN** traversing host-client-architecture.yaml
- **THEN** system SHALL explain Host (listen server) vs Dedicated Server
- **AND** provide authority implications for each choice
- **AND** recommend appropriate ownership patterns

#### Scenario: Late-join synchronization
- **GIVEN** developer handling late-joining players
- **WHEN** evaluating state sync for new players
- **THEN** system SHALL recommend NetworkVariable initial sync
- **AND** provide spawn-time state restoration pattern
- **AND** reference OnNetworkSpawn initialization pattern

### Requirement: Decision Tree KB Search Integration

All decision tree outcomes SHALL reference KB-verified implementations when available.

#### Scenario: Outcome with KB reference
- **GIVEN** decision tree reaches an outcome node
- **WHEN** outcome provides code recommendation
- **THEN** system SHALL include KB search terms for verification
- **AND** provide file paths to actual project implementations
- **AND** include assembly name for code location

#### Scenario: KB-powered code examples
- **GIVEN** decision outcome includes code example
- **WHEN** example references project patterns
- **THEN** code SHALL match actual project implementation
- **AND** include file path and line numbers where applicable
- **AND** use project-specific naming conventions (m_ prefix, Rpc suffixes)

## MODIFIED Requirements

### Requirement: Network Sync Method Tree Enhancement

The existing network-sync-method.yaml SHALL be enhanced with GameCreator awareness.

#### Scenario: GameCreator-aware sync decision
- **GIVEN** developer using network-sync-method.yaml
- **WHEN** selecting sync method for GameCreator data
- **THEN** system SHALL recognize GameCreator data types
- **AND** provide GC-specific recommendations
- **AND** reference invasive integration patterns

### Requirement: State Management Tree Enhancement

The existing state-management.yaml SHALL include project constraints.

#### Scenario: Project constraint injection
- **GIVEN** developer traversing state-management.yaml
- **WHEN** evaluating authority patterns
- **THEN** system SHALL inject NetworkTransform constraint
- **AND** recommend NetworkCharacterAdapter for character state
- **AND** enforce IsNetworkOwner pattern for ownership checks
