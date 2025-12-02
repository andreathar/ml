# GameCreator Invasive Integration Pattern

**Priority:** P0 CRITICAL
**Last Updated:** 2025-11-23
**Applies To:** Character networking, player ownership, multiplayer gameplay

## Core Principle: Invasive Integration

GameCreator Multiplayer uses **invasive integration** - we directly integrate into GameCreator's Character class and components rather than wrapping or extending them. This provides seamless multiplayer without API breaking changes.

## IsNetworkOwner Pattern

### The Problem
GameCreator's Character class needs to know which player controls it for input processing, but it has no concept of networking.

### The Solution
We added `IsNetworkOwner` property directly to the Character class:

```csharp
// In Character.cs (invasive integration)
public bool IsNetworkOwner
{
    get
    {
        if (!NetworkManager.Singleton.IsClient || !NetworkManager.Singleton.IsServer)
            return true; // Offline mode - always owner

        if (this.Player == null)
            return false; // No player assigned

        return this.Player.IsOwner; // Netcode ownership check
    }
}
```

### Usage Pattern

**Input Processing:**
```csharp
// In PlayerInputReader or similar
if (character.IsNetworkOwner)
{
    // Only process input if we own this character
    character.Motion.MoveToDirection(direction);
}
```

**Network Spawning:**
```csharp
// CRITICAL: Set before spawning with Netcode
character.IsNetworkSpawned = true;
var networkObject = character.GetComponent<NetworkObject>();
networkObject.SpawnWithOwnership(clientId);
```

## IsNetworkSpawned Flag

### Why It Exists
GameCreator components may initialize before NetworkObject spawns. We need to know if a GameObject is network-managed.

### Pattern
```csharp
// Set this flag BEFORE Netcode spawn
character.IsNetworkSpawned = true;

// GameCreator components can now check:
if (character.IsNetworkSpawned && !character.IsNetworkOwner)
{
    // Suppress local-only behavior on non-owners
    return;
}
```

## Ownership Transfer

### When Character Changes Players
```csharp
// Server-side ownership transfer
[ServerRpc(RequireOwnership = false)]
public void TransferOwnershipServerRpc(ulong newClientId)
{
    var networkObject = GetComponent<NetworkObject>();
    networkObject.ChangeOwnership(newClientId);

    // Update Character.Player reference
    var newPlayer = FindPlayerByClientId(newClientId);
    character.Player = newPlayer;
}
```

## Common Pitfalls

### ❌ WRONG: Checking IsOwner on Character
```csharp
// Character doesn't have IsOwner - that's on NetworkObject!
if (character.IsOwner) // COMPILE ERROR
```

### ✅ RIGHT: Use IsNetworkOwner
```csharp
if (character.IsNetworkOwner) // Correct property
```

### ❌ WRONG: Forgetting IsNetworkSpawned
```csharp
networkObject.Spawn(); // Character doesn't know it's networked!
```

### ✅ RIGHT: Set flag before spawn
```csharp
character.IsNetworkSpawned = true;
networkObject.Spawn();
```

### ❌ WRONG: Non-owner processing input
```csharp
// This runs on all clients!
character.Motion.MoveToDirection(input);
```

### ✅ RIGHT: Guard with ownership check
```csharp
if (character.IsNetworkOwner)
{
    character.Motion.MoveToDirection(input);
}
```

## Integration Points

**Affected GameCreator Classes:**
- `Character` - Added IsNetworkOwner, IsNetworkSpawned properties
- `CharacterController` - Respects ownership for input
- `CharacterMotion` - Server authority for movement commands
- `CharacterJump` - Owner-only input, server-validated execution

**Multiplayer Components:**
- `NetworkCharacterAdapter` - Bridges Character ↔ NetworkObject
- `Player_Network.prefab` - Reference prefab with proper setup

## Architecture Decision Rationale

**Why Invasive?**
1. **No API Changes:** GameCreator users don't change their code
2. **Seamless Integration:** Multiplayer "just works" with existing features
3. **Performance:** No wrapper overhead, direct property access
4. **Maintainability:** Single codebase, not separate "multiplayer character"

**Trade-off:**
- Requires GameCreator source code access (we have it)
- Couples multiplayer tightly to GameCreator version
- Must maintain integration across GameCreator updates

## Testing Pattern

```csharp
[UnityTest]
public IEnumerator Character_WhenSpawned_IsNetworkOwnerTrue()
{
    // Arrange
    var character = SpawnCharacter(clientId: 0);
    character.IsNetworkSpawned = true;

    // Act
    var networkObject = character.GetComponent<NetworkObject>();
    networkObject.SpawnAsPlayerObject(0);
    yield return new WaitForSeconds(0.1f);

    // Assert
    Assert.IsTrue(character.IsNetworkOwner);
}
```

## Related Documentation

- `.serena/memories/CRITICAL/002_network_architecture_never_forget.md` - NetworkTransform removal
- `openspec/specs/character-system/spec.md` - Character system specification
- `Assets/Plugins/GameCreator_Multiplayer/Runtime/Character/NetworkCharacterAdapter.cs` - Implementation

## Quick Reference

| Scenario | Pattern |
|----------|---------|
| Check if character is mine | `if (character.IsNetworkOwner)` |
| Before Netcode spawn | `character.IsNetworkSpawned = true` |
| Process input | Guard with `IsNetworkOwner` check |
| Transfer ownership | Server RPC calling `ChangeOwnership()` |
| Offline mode | `IsNetworkOwner` returns `true` automatically |
