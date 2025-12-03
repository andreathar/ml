# Netcode for GameObjects Integration

This package provides integration between **GameCreator 2** and **Unity Netcode for GameObjects (NGO)**, enabling multiplayer character synchronization while preserving all GameCreator features.

## Requirements

- Unity 6 (6000.0+)
- Unity Netcode for GameObjects 2.0+
- GameCreator 2.0+

## Installation

This package is already installed at:
```
Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/
```

All files are **editable** - you can modify them to fit your project needs.

## Quick Start

### 1. Setup NetworkCharacter

Replace the standard `Character` component with `NetworkCharacter` on your player prefab:

1. Remove existing `Character` component
2. Add `Network Character` component (Game Creator > Characters > Network Character)
3. Configure motion, driver, and other settings as normal

### 2. Add Network Components

Add these components to your networked character prefab:

| Component | Purpose |
|-----------|---------|
| `NetworkObject` | Required for all networked objects |
| `NetworkTransform` | Syncs position/rotation |
| `NetworkAnimator` | Syncs animation parameters |
| `NetworkCharacterAdapter` | Bridges Character and network systems |
| `NetworkCharacterMotion` | Syncs motion state (optional) |

### 3. Configure Driver

To use the network-aware driver that skips CharacterController.center conflicts:

1. In the Character's Kernel settings
2. Change Driver from "Character Controller" to "Network Character Controller"

## Components

### NetworkCharacter

Extends `Character` with:
- `IsNetworkSpawned` property - When true, CharacterController.center is not modified
- Network lifecycle awareness
- Backward compatible with single-player

### NetworkCharacterAdapter

The main bridge component:
- Sets `IsNetworkSpawned` automatically on network spawn/despawn
- Manages ownership
- Fires events: `EventNetworkSpawned`, `EventNetworkDespawned`, `EventOwnershipChanged`

### NetworkCharacterMotion

Synchronizes motion state:
- MoveDirection via NetworkVariable
- LinearSpeed via NetworkVariable
- IsGrounded state
- Jump/Dash via RPC

### NetworkVariablesSync

Synchronizes GameCreator variables:
- Configure variables by name
- Set sync mode: Everyone, OwnerOnly, ServerOnly
- Float, Int, Bool, String support

## Visual Scripting

### Instructions (Actions)

| Instruction | Description |
|-------------|-------------|
| Spawn Network Player | Spawns a player prefab with ownership |
| Despawn Network Object | Removes an object from the network |
| Change Network Ownership | Transfers ownership to another client |
| Start Network Host | Starts as host (server + client) |
| Start Network Client | Connects as client |
| Disconnect Network | Shuts down network |

### Conditions

| Condition | Description |
|-----------|-------------|
| Is Server | True if running as server |
| Is Client | True if running as client |
| Is Host | True if running as both server and client |
| Is Owner | True if local client owns the object |
| Is Spawned | True if object is spawned on network |
| Is Connected | True if connected to network |

### Triggers (Events)

| Trigger | Description |
|---------|-------------|
| On Client Connected | When any client connects |
| On Client Disconnected | When any client disconnects |
| On Local Player Spawned | When your player character spawns |
| On Ownership Changed | When ownership transfers |
| On Network Spawn | When this object spawns on network |
| On Network Despawn | When this object despawns |

## Prefab Templates

Located in the package root:
- `Network_Manager.prefab` - Basic NetworkManager setup
- `Network_Player_Manager.prefab` - Player spawn handling
- `Network_Spawn_Manager.prefab` - Spawn point management
- `Player_Network.prefab` - Network-ready player template

## Architecture

```
NetworkCharacter (extends Character)
    └── IsNetworkSpawned property
            │
            ▼
NetworkUnitDriverController (extends UnitDriverController)
    └── Skips CharacterController.center reset when IsNetworkSpawned=true
            │
            ▼
NetworkCharacterAdapter (NetworkBehaviour)
    └── Sets IsNetworkSpawned on OnNetworkSpawn/OnNetworkDespawn
    └── Manages ownership events
            │
            ▼
NetworkCharacterMotion (NetworkBehaviour)
    └── Syncs motion state via NetworkVariables
    └── Handles Jump/Dash via RPCs
```

## The CharacterController Conflict

GameCreator's `UnitDriverController.UpdatePhysicProperties()` resets `CharacterController.center = Vector3.zero` every frame. This conflicts with `NetworkTransform` which needs to control the character's position.

**Solution**: `NetworkUnitDriverController` overrides this behavior and skips the center reset when `IsNetworkSpawned = true`.

## Customization

All source files are editable. Common customizations:

### Add Custom NetworkVariables

Extend `NetworkCharacterMotion` to sync additional state:

```csharp
public class MyNetworkCharacterMotion : NetworkCharacterMotion
{
    private NetworkVariable<float> m_Health = new NetworkVariable<float>();

    // ... implement sync logic
}
```

### Custom Ownership Logic

Override `NetworkCharacterAdapter`:

```csharp
public class MyNetworkCharacterAdapter : NetworkCharacterAdapter
{
    protected override void OnLocalPlayerSpawned()
    {
        base.OnLocalPlayerSpawned();
        // Custom initialization
    }
}
```

## Troubleshooting

### Character Falls Through Ground
- Ensure `NetworkTransform` is configured correctly
- Check that `IsNetworkSpawned` is being set (debug log)

### Movement Not Syncing
- Add `NetworkCharacterMotion` component
- Check sync rate settings

### Ownership Not Transferring
- Only server can transfer ownership
- Use `NetworkCharacterAdapter.TransferOwnership()`

## Version History

- **1.0.0** - Initial release with core integration

## License

This package is part of the MLCreator project.
