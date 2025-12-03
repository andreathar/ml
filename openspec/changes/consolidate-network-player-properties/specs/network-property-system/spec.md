# Network Property System Specification

**Capability:** network-property-system
**Status:** Proposed
**Last Updated:** 2025-12-03

## Overview

The Network Property System extends GameCreator's polymorphic property framework to support multiplayer scenarios. It provides property types that resolve to the correct network-local or remote character based on client context.

---

## ADDED Requirements

### Requirement: Network Local Player Position Property

The system SHALL provide a `GetPositionNetworkLocalPlayer` property type that returns the world position of the character owned by the local client.

#### Scenario: Local player position in multiplayer
- **GIVEN** multiple NetworkCharacters are spawned in a multiplayer session
- **WHEN** a PropertyGetPosition is configured with "Network Local Player Position"
- **THEN** it SHALL return the position of the NetworkCharacter where `IsLocalOwner == true`
- **AND** the result SHALL differ for each connected client

#### Scenario: Fallback when not in multiplayer
- **GIVEN** the game is running in single-player mode (no NetworkManager)
- **WHEN** a PropertyGetPosition is configured with "Network Local Player Position"
- **THEN** it SHALL fallback to `ShortcutPlayer.Transform.position`
- **AND** it SHALL log no errors

### Requirement: Network Local Player Rotation Property

The system SHALL provide a `GetRotationNetworkLocalPlayer` property type that returns the world rotation of the character owned by the local client.

#### Scenario: Local player rotation in multiplayer
- **GIVEN** a NetworkCharacter owned by the local client exists
- **WHEN** a PropertyGetRotation is configured with "Network Local Player Rotation"
- **THEN** it SHALL return the Quaternion rotation of that character's transform
- **AND** updates SHALL reflect real-time character rotation

### Requirement: Network Local Player Scale Property

The system SHALL provide a `GetScaleNetworkLocalPlayer` property type that returns the local scale of the character owned by the local client.

#### Scenario: Local player scale access
- **GIVEN** a NetworkCharacter owned by the local client exists
- **WHEN** a PropertyGetScale is configured with "Network Local Player Scale"
- **THEN** it SHALL return the Vector3 localScale of that character's transform

### Requirement: Network Local Player Location Property

The system SHALL provide a `GetLocationNetworkLocalPlayer` property type that returns both position and rotation as a Location struct.

#### Scenario: Spawn at local player location
- **GIVEN** a NetworkCharacter owned by the local client exists
- **WHEN** an instruction spawns an object at "Network Local Player Location"
- **THEN** the object SHALL spawn at the character's position with the character's rotation

### Requirement: Network Character By ClientId Property

The system SHALL provide a `GetGameObjectNetworkCharacterByClientId` property type that returns the NetworkCharacter owned by a specified client.

#### Scenario: Target another player's character
- **GIVEN** clients A, B, and C are connected with ClientIds 0, 1, and 2
- **WHEN** client A uses GetGameObjectNetworkCharacterByClientId with ClientId=1
- **THEN** it SHALL return client B's NetworkCharacter GameObject
- **AND** if client B disconnects, it SHALL return null

### Requirement: Network Character Position By ClientId Property

The system SHALL provide a `GetPositionNetworkCharacterByClientId` property type for direct position access by client ID.

#### Scenario: Get remote player position for targeting
- **GIVEN** a client wants to aim at another player
- **WHEN** GetPositionNetworkCharacterByClientId is evaluated with the target's ClientId
- **THEN** it SHALL return that player's current world position
- **AND** it SHALL update every frame for smooth tracking

### Requirement: Network Characters List Property

The system SHALL provide a `GetListGameObjectNetworkCharacters` property type that returns all spawned NetworkCharacters as a list.

#### Scenario: Populate list for nearest enemy calculation
- **GIVEN** 4 NetworkCharacters are spawned in the scene
- **WHEN** a ListVariables is configured with "Network Characters List"
- **THEN** it SHALL contain all 4 NetworkCharacter GameObjects
- **AND** the list SHALL update when characters spawn or despawn

### Requirement: Network Character Registry

The system SHALL maintain a central registry of all active NetworkCharacters to avoid expensive FindObjectsByType calls.

#### Scenario: Character registration on spawn
- **WHEN** a NetworkCharacter's OnNetworkSpawn is called
- **THEN** it SHALL register itself with NetworkCharacterRegistry
- **AND** the registry SHALL immediately include it in All and LocalPlayer queries

#### Scenario: Character unregistration on despawn
- **WHEN** a NetworkCharacter's OnNetworkDespawn is called
- **THEN** it SHALL unregister from NetworkCharacterRegistry
- **AND** subsequent queries SHALL NOT include the despawned character

#### Scenario: Local player cache invalidation
- **WHEN** ownership of a NetworkCharacter changes
- **THEN** the registry SHALL invalidate its LocalPlayer cache
- **AND** the next LocalPlayer query SHALL re-evaluate ownership

### Requirement: Editor Preview Support

All network property types SHALL provide sensible preview values in the Unity Editor.

#### Scenario: Preview in edit mode
- **GIVEN** the game is not running (edit mode)
- **WHEN** a property field displays its current value preview
- **THEN** it SHALL show the position/rotation/scale of any Character marked as Player
- **AND** it SHALL NOT throw NullReferenceException

---

## Dependencies

### Internal
- `character-system` - NetworkCharacter base class
- `network-synchronization` - NetworkObject, ownership patterns
- `visual-scripting` - Property serialization framework

### External
- `GameCreator.Runtime.Common` - PropertyTypeGetPosition, PropertyGetPosition
- `Unity.Netcode` - NetworkManager, NetworkObject

---

## See Also

- `design.md` - Technical architecture and caching strategy
- `GetGameObjectNetworkLocalPlayer.cs` - Reference implementation
- `.serena/memories/CRITICAL/network_architecture_never_forget.md` - Network patterns
