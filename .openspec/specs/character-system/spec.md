# Character System Specification

**Capability:** character-system
**Status:** Baseline (v1.0)
**Last Updated:** 2025-11-07

## Overview

The Character System integrates GameCreator's Character component with Unity Netcode for GameObjects to enable networked character control while preserving all GameCreator Character features (motion, rotation, IK, ragdoll, footsteps, etc.).

**Core Innovation:** Character override system using `IsNetworkSpawned` flag to prevent CharacterController conflicts during network spawn.

---

## Requirements

### Requirement: Character Network Spawn Flag

The Character component SHALL provide an `IsNetworkSpawned` boolean property that prevents CharacterController modifications when set to true, allowing Unity Netcode to manage character positioning.

#### Scenario: Network character initialization
- **GIVEN** a Character prefab with NetworkObject component
- **WHEN** the prefab is spawned via NetworkObject.Spawn()
- **AND** `character.IsNetworkSpawned` is set to true before spawn
- **THEN** the Character SHALL NOT modify CharacterController.center
- **AND** NetworkTransform SHALL control character position
- **AND** all other Character features SHALL function normally

#### Scenario: Single-player character initialization
- **GIVEN** a Character prefab without NetworkObject component
- **WHEN** the prefab is instantiated normally
- **AND** `character.IsNetworkSpawned` is false (default)
- **THEN** the Character SHALL initialize CharacterController.center normally
- **AND** all Character features SHALL function as standard GameCreator behavior

### Requirement: Character Motion Synchronization

Networked characters SHALL synchronize motion state (velocity, direction, grounded status) from owner to all clients.

#### Scenario: Owner movement replication
- **GIVEN** a networked character controlled by client A
- **WHEN** client A moves the character forward
- **THEN** character.Motion.MoveToDirection() SHALL execute on client A
- **AND** the movement SHALL be replicated to all other clients
- **AND** the character SHALL appear to move smoothly on remote clients

#### Scenario: Server authority enforcement
- **GIVEN** a networked character
- **WHEN** a non-owner client attempts to call character.Motion methods
- **THEN** the call SHALL be ignored or queued as server RPC
- **AND** only the server or owner SHALL apply motion changes

### Requirement: Character Driver Compatibility

The Character driver system (UnitDriverController) SHALL respect the `IsNetworkSpawned` flag and skip CharacterController modifications for networked characters.

#### Scenario: Driver respects network flag
- **GIVEN** a Character with `IsNetworkSpawned = true`
- **WHEN** UnitDriverController.OnStartup() executes
- **THEN** CharacterController.center modifications SHALL be skipped
- **AND** driver SHALL still manage character input and motion
- **AND** other driver features (rotation, jump, dash) SHALL work normally

#### Scenario: Driver normal operation for non-networked
- **GIVEN** a Character with `IsNetworkSpawned = false`
- **WHEN** UnitDriverController.OnStartup() executes
- **THEN** CharacterController.center SHALL be set as per GameCreator defaults
- **AND** all driver features SHALL work as standard GameCreator

### Requirement: Character Animation Synchronization

Character animation state SHALL synchronize across network using NetworkAnimator.

#### Scenario: Animation parameter sync
- **GIVEN** a networked character with Animator component
- **WHEN** the owner triggers animation (walk, jump, attack)
- **THEN** animation parameters SHALL replicate to all clients
- **AND** remote clients SHALL play matching animations
- **AND** animation SHALL remain in sync with character motion

### Requirement: Character IK Synchronization

Character IK (Inverse Kinematics) SHALL function correctly for both owner and remote clients.

#### Scenario: IK positioning on remote clients
- **GIVEN** a networked character with IK enabled (foot placement, look-at)
- **WHEN** the character moves on uneven terrain
- **THEN** IK adjustments SHALL apply on all clients
- **AND** feet SHALL align with ground on both owner and remote views
- **AND** IK SHALL use local terrain data (not replicated)

### Requirement: Character Ragdoll Network Transition

Character ragdoll state SHALL synchronize across network when triggered.

#### Scenario: Ragdoll activation sync
- **GIVEN** a networked character in normal state
- **WHEN** ragdoll is activated (death, explosion, etc.)
- **THEN** ragdoll state SHALL replicate to all clients
- **AND** ragdoll physics SHALL apply on all clients
- **AND** final rest position SHALL sync via NetworkTransform

### Requirement: Character Ground Detection

Networked characters SHALL detect ground correctly on all clients for footstep sounds and grounded state.

#### Scenario: Ground detection replication
- **GIVEN** a networked character walking on terrain
- **WHEN** character transitions between grounded and airborne
- **THEN** grounded state SHALL replicate to remote clients
- **AND** footstep sounds SHALL play on appropriate clients
- **AND** ground detection SHALL use local physics queries (not replicated)

### Requirement: Network Character Ownership Transfer

Character ownership SHALL transfer cleanly between clients without state corruption.

#### Scenario: Ownership transfer preserves state
- **GIVEN** a networked character owned by client A
- **WHEN** ownership transfers to client B
- **THEN** character motion state SHALL persist
- **AND** `IsNetworkSpawned` flag SHALL remain true
- **AND** client B SHALL gain control immediately
- **AND** no CharacterController reset SHALL occur

### Requirement: Character Network Despawn

Characters SHALL despawn cleanly from network without errors or state leaks.

#### Scenario: Clean despawn on disconnect
- **GIVEN** a networked character owned by client A
- **WHEN** client A disconnects
- **THEN** character SHALL despawn via NetworkObject.Despawn()
- **AND** all Character components SHALL clean up properly
- **AND** no lingering references SHALL remain

### Requirement: Multi-Character Support

Multiple networked characters SHALL coexist in the same scene without interference.

#### Scenario: Multiple players in scene
- **GIVEN** 4 networked player characters in scene
- **WHEN** all characters move simultaneously
- **THEN** each character SHALL update independently
- **AND** no character SHALL interfere with others' state
- **AND** all characters SHALL remain visible and synchronized

---

## Non-Functional Requirements

### Performance

- Character network updates SHALL occur at 30 Hz (30 times per second)
- Character state replication SHALL use <5 KB/s per character average
- Character spawn SHALL complete within 100ms from spawn request
- Character motion SHALL have <50ms perceived latency (client prediction)

### Compatibility

- Character system SHALL work with all GameCreator Character features:
  - Motion (walk, run, strafe)
  - Rotation (toward camera, toward direction)
  - Jump, dash, climb
  - IK (foot placement, look-at, hand IK)
  - Ragdoll (full body, partial)
  - Footsteps (material-based sounds)
  - States (idle, moving, airborne, busy)

- Character system SHALL NOT require GameCreator source code modifications (except Character.cs fork)

### Reliability

- Character spawn SHALL succeed 100% of time with valid spawn point
- Character network state SHALL recover from temporary network interruptions
- Character SHALL NOT fall through ground on high-latency connections

---

## Dependencies

### Internal

- **network-synchronization** - NetworkVariable, RPC, NetworkTransform
- **spawn-system** - Smart spawn, ground detection
- **visual-scripting** - Character control instructions

### External

- **GameCreator.Runtime.Characters** - Character, Motion, Driver, IK, Ragdoll
- **Unity.Netcode.Components** - NetworkTransform, NetworkAnimator
- **Unity.Netcode.Runtime** - NetworkObject, NetworkBehaviour
- **UnityEngine.CharacterController** - Physics-based character movement

---

## Migration Notes

### From Standard GameCreator to Networked

**Character Prefab Setup:**
1. Add NetworkObject component
2. Add NetworkTransform component (sync position/rotation)
3. Add NetworkAnimator component (sync animation)
4. Add NetworkGameCreatorCharacter component (multiplayer wrapper)
5. Set `character.IsNetworkSpawned = true` in spawn script

**Code Changes:**
- Replace direct Character access with ownership checks
- Use NetworkVariables for character stats (health, stamina)
- Replace Instantiate() with NetworkObject.Spawn()
- Add server authority checks before state changes

### Breaking Changes

None - system is additive. Non-networked GameCreator projects work unchanged.

---

## Open Questions

None currently.

---

## See Also

- `design.md` - Technical implementation details
- `.serena/memories/_CRITICAL/GameCreator_Character_Complete_API.md` - Complete Character API reference
- `claudedocs/CHARACTER_COMPONENT_FIX.md` - Character override troubleshooting
