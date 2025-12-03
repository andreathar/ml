# Netcode for GameObjects Integration Specification

**Capability:** netcode-gamecreator-integration
**Status:** Proposed
**Last Updated:** 2025-12-03

## Overview

The Netcode for GameObjects Integration package provides editable source files and components to integrate GameCreator 2 with Unity Netcode for GameObjects (NGO). This enables multiplayer character synchronization while preserving all GameCreator features.

**Location:** `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/`

---

## ADDED Requirements

### Requirement: Editable Package Structure

The integration SHALL provide an editable package structure at `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/` containing Runtime, Editor, and Prefabs folders.

#### Scenario: Package folder creation
- **GIVEN** a Unity project with GameCreator installed
- **WHEN** the Netcode integration package is added
- **THEN** the following structure SHALL exist:
  - `Netcode_for_GameObjects_Integration/Runtime/`
  - `Netcode_for_GameObjects_Integration/Editor/`
  - `Netcode_for_GameObjects_Integration/Prefabs/`
  - `Netcode_for_GameObjects_Integration/Resources/`

### Requirement: Runtime Assembly Definition

The package SHALL provide `GameCreator.Netcode.Runtime.asmdef` with proper references to Unity Netcode and GameCreator assemblies.

#### Scenario: Assembly compilation
- **GIVEN** the Netcode integration package is installed
- **WHEN** Unity compiles the project
- **THEN** the GameCreator.Netcode.Runtime assembly SHALL compile without errors
- **AND** it SHALL reference Unity.Netcode.Runtime
- **AND** it SHALL reference GameCreator.Runtime.Characters

### Requirement: NetworkCharacter Component

The package SHALL provide a `NetworkCharacter` class extending `Character` with network-aware properties.

#### Scenario: IsNetworkSpawned property access
- **GIVEN** a GameObject with NetworkCharacter component
- **WHEN** code accesses `networkCharacter.IsNetworkSpawned`
- **THEN** it SHALL return a boolean indicating network spawn state
- **AND** the property SHALL be settable

#### Scenario: Backward compatibility
- **GIVEN** a project using standard Character components
- **WHEN** NetworkCharacter is not used
- **THEN** all existing Character functionality SHALL work unchanged

### Requirement: NetworkUnitDriverController Component

The package SHALL provide a `NetworkUnitDriverController` class that skips CharacterController.center modification for networked characters.

#### Scenario: Networked character driver behavior
- **GIVEN** a NetworkCharacter with `IsNetworkSpawned = true`
- **WHEN** NetworkUnitDriverController.UpdatePhysicProperties() executes
- **THEN** CharacterController.center SHALL NOT be reset to Vector3.zero
- **AND** height and radius updates SHALL still occur

#### Scenario: Non-networked character driver behavior
- **GIVEN** a NetworkCharacter with `IsNetworkSpawned = false`
- **WHEN** NetworkUnitDriverController.UpdatePhysicProperties() executes
- **THEN** standard UnitDriverController behavior SHALL apply
- **AND** CharacterController.center SHALL be managed normally

### Requirement: NetworkCharacterAdapter Component

The package SHALL provide a `NetworkCharacterAdapter` NetworkBehaviour that bridges Character and network systems.

#### Scenario: Network spawn lifecycle
- **GIVEN** a prefab with NetworkObject and NetworkCharacterAdapter
- **WHEN** NetworkObject.Spawn() is called
- **THEN** NetworkCharacterAdapter.OnNetworkSpawn() SHALL execute
- **AND** the associated NetworkCharacter.IsNetworkSpawned SHALL be set to true

#### Scenario: Network despawn lifecycle
- **GIVEN** a spawned networked character
- **WHEN** the NetworkObject is despawned
- **THEN** NetworkCharacterAdapter.OnNetworkDespawn() SHALL execute
- **AND** the associated NetworkCharacter.IsNetworkSpawned SHALL be set to false

### Requirement: Motion State Synchronization

The package SHALL synchronize character motion state (direction, speed, grounded) via NetworkVariables.

#### Scenario: Movement replication
- **GIVEN** a networked character owned by Client A
- **WHEN** Client A moves the character
- **THEN** MoveDirection NetworkVariable SHALL update
- **AND** remote clients SHALL receive the movement state
- **AND** the character SHALL appear to move on remote clients

#### Scenario: Grounded state sync
- **GIVEN** a networked character
- **WHEN** the owner character lands on ground
- **THEN** IsGrounded NetworkVariable SHALL update to true
- **AND** remote clients SHALL receive the grounded state

### Requirement: Network Visual Scripting Instructions

The package SHALL provide visual scripting Instructions for common network operations.

#### Scenario: Spawn player instruction
- **GIVEN** a visual script with "Network: Spawn Player" instruction
- **WHEN** the instruction executes on the server
- **THEN** a player prefab SHALL spawn via NetworkObject.Spawn()
- **AND** ownership SHALL be assigned to the requesting client

#### Scenario: Despawn instruction
- **GIVEN** a visual script with "Network: Despawn" instruction
- **WHEN** the instruction executes
- **THEN** the target NetworkObject SHALL despawn

### Requirement: Network Visual Scripting Conditions

The package SHALL provide Conditions for checking network state in visual scripts.

#### Scenario: IsServer condition
- **GIVEN** a visual script with "Is Server" condition
- **WHEN** evaluated on a server/host
- **THEN** the condition SHALL return true

#### Scenario: IsOwner condition
- **GIVEN** a visual script with "Is Owner" condition on a NetworkObject
- **WHEN** evaluated by the owning client
- **THEN** the condition SHALL return true
- **AND** non-owners SHALL receive false

### Requirement: Network Visual Scripting Triggers

The package SHALL provide Triggers for network events.

#### Scenario: On local player spawned trigger
- **GIVEN** a GameObject with "On Local Player Spawned" trigger
- **WHEN** the local player's character spawns
- **THEN** the trigger SHALL fire
- **AND** attached instructions SHALL execute

#### Scenario: On ownership changed trigger
- **GIVEN** a networked character with "On Ownership Changed" trigger
- **WHEN** ownership transfers to a different client
- **THEN** the trigger SHALL fire on both old and new owners

### Requirement: Prefab Templates

The package SHALL provide ready-to-use prefab templates for common multiplayer setups.

#### Scenario: Player_Network prefab structure
- **GIVEN** the Player_Network.prefab template
- **WHEN** examined in Unity Editor
- **THEN** it SHALL contain:
  - NetworkCharacter (or Character) component
  - NetworkObject component
  - NetworkTransform component
  - NetworkAnimator component
  - NetworkCharacterAdapter component

#### Scenario: Network_Manager prefab
- **GIVEN** the Network_Manager.prefab template
- **WHEN** added to a scene
- **THEN** it SHALL provide basic NetworkManager configuration
- **AND** it SHALL be ready for host/client connections

---

## Non-Functional Requirements

### Editability

- All source files in the package SHALL be editable (not read-only)
- The package SHALL NOT be delivered via Unity Package Manager (to ensure editability)
- Changes to package files SHALL persist across Unity restarts

### Compatibility

- The package SHALL be compatible with Unity 6 (6000.0+)
- The package SHALL be compatible with Unity Netcode for GameObjects 2.0+
- The package SHALL be compatible with GameCreator 2.0+
- Single-player projects using GameCreator SHALL NOT be affected by this package

### Performance

- NetworkVariable updates SHALL occur at a configurable rate (default 30 Hz)
- Motion synchronization SHALL use <5 KB/s per character average
- No additional garbage collection allocations SHALL occur per frame

---

## Dependencies

### External

- **Unity.Netcode.Runtime** (2.0+) - Core networking
- **Unity.Netcode.Components** - NetworkTransform, NetworkAnimator
- **GameCreator.Runtime.Characters** - Character, Motion, Driver
- **GameCreator.Runtime.Common** - Utilities, Args
- **GameCreator.Runtime.VisualScripting** - Instructions, Conditions, Triggers

### Internal

- **character-system** - Relies on IsNetworkSpawned implementation
- **network-synchronization** - Implements variable sync patterns
- **spawn-system** - Uses network spawn helpers
- **visual-scripting** - Extends with network nodes

---

## See Also

- `design.md` - Technical implementation details
- `tasks.md` - Implementation checklist
- `openspec/specs/character-system/spec.md` - Character system specification
- `openspec/specs/network-synchronization/spec.md` - Network sync patterns
