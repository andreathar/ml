# GameCreator Netcode Integration

**Priority:** P0 CRITICAL
**Last Updated:** 2025-12-03
**Assembly:** GameCreator.Netcode.Runtime

## Overview

This project uses the **official GameCreator Netcode Integration** module, not a custom invasive integration. The module provides a clean bridge between GameCreator's Character system and Unity Netcode for GameObjects.

## Key Components

### NetworkCharacterAdapter

The primary bridge component that connects GameCreator Character with Unity NetworkBehaviour.

```csharp
[RequireComponent(typeof(NetworkObject))]
public class NetworkCharacterAdapter : NetworkBehaviour
{
    public Character Character { get; }
    public NetworkCharacter NetworkCharacter { get; }
    public new bool IsLocalPlayer => IsOwner && Character.IsPlayer;
}
```

**Usage:**
- Attach to any prefab with a Character component
- Requires NetworkObject component
- Automatically manages IsNetworkSpawned lifecycle

### NetworkCharacter

Extended Character class with network properties.

```csharp
public class NetworkCharacter : Character
{
    public bool IsNetworkSpawned { get; set; }
    public void BecomeLocalPlayer();
    public void BecomeRemotePlayer();
}
```

**Usage:**
- Use instead of Character for networked characters
- IsNetworkSpawned is set automatically by NetworkCharacterAdapter

## Spawn Lifecycle

```csharp
// NetworkCharacterAdapter handles this automatically:

OnNetworkSpawn()
├── NetworkCharacter.IsNetworkSpawned = true
├── if (IsOwner)
│   └── NetworkCharacter.BecomeLocalPlayer()  // Sets IsPlayer = true
└── else
    └── NetworkCharacter.BecomeRemotePlayer()

OnNetworkDespawn()
└── NetworkCharacter.IsNetworkSpawned = false
```

## Ownership Patterns

```csharp
// Check if this client owns the character
if (networkCharacterAdapter.IsOwner)
{
    // This client owns this character
}

// Check if this is the local player
if (networkCharacterAdapter.IsLocalPlayer)
{
    // IsOwner AND Character.IsPlayer
}

// In visual scripting, use:
// Condition: Network Is Owner
// Condition: Network Is Local Player
```

## Do's and Don'ts

✅ **DO:**
- Use NetworkCharacterAdapter for all networked characters
- Use NetworkCharacter instead of Character for network support
- Check IsOwner before local player logic
- Let the adapter manage IsNetworkSpawned

❌ **DON'T:**
- Modify GameCreator source code
- Manually set IsNetworkSpawned
- Add NetworkTransform to characters (use NetworkCharacterMotion)
- Skip the NetworkCharacterAdapter bridge

## Related Files

- `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/Runtime/Components/`
- `.serena/personas/netcode-core.llm.txt`
