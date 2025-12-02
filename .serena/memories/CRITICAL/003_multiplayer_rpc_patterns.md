# Multiplayer RPC Patterns and Conventions

**Priority:** P0 CRITICAL
**Last Updated:** 2025-11-23
**Applies To:** All NetworkBehaviour implementations, multiplayer networking

## RPC Naming Convention (MANDATORY)

### The Rule
**ALL RPC methods MUST end with `ClientRpc` or `ServerRpc`** - no exceptions.

```csharp
// ✅ CORRECT
[ServerRpc]
public void MovePlayerServerRpc(Vector3 direction) { }

[ClientRpc]
public void UpdateScoreClientRpc(int newScore) { }

// ❌ WRONG - will not compile with Netcode
[ServerRpc]
public void MovePlayer(Vector3 direction) { } // Missing "ServerRpc" suffix

[ClientRpc]
public void UpdateScore(int newScore) { } // Missing "ClientRpc" suffix
```

### Why This Matters
Unity Netcode's code generation **requires** this naming pattern. Without it:
- Code won't compile
- RPC calls won't route correctly
- Runtime errors and silent failures

## ServerRpc Pattern

### Purpose
**Client → Server communication** for authoritative actions.

### Standard Pattern
```csharp
[ServerRpc(RequireOwnership = true)] // Default: only owner can call
public void PerformActionServerRpc(ActionData data)
{
    // Validate input (CRITICAL - never trust client)
    if (!IsValidAction(data))
    {
        Debug.LogWarning($"Invalid action from client {OwnerClientId}");
        return;
    }

    // Execute server-authoritative logic
    ExecuteAction(data);

    // Optionally broadcast result to all clients
    NotifyActionResultClientRpc(data);
}
```

### RequireOwnership Parameter
```csharp
// Owner-only (default - secure)
[ServerRpc(RequireOwnership = true)]
public void MoveMyCharacterServerRpc(Vector3 direction) { }

// Any client can call (use with caution!)
[ServerRpc(RequireOwnership = false)]
public void InteractWithWorldObjectServerRpc(ulong objectId) { }
```

### Common Use Cases
- Movement commands (owner sends, server executes)
- Action requests (pickup, interact, attack)
- Inventory changes (client requests, server validates)
- World modifications (client proposes, server authorizes)

## ClientRpc Pattern

### Purpose
**Server → Client(s) communication** for state updates and notifications.

### Standard Pattern
```csharp
[ClientRpc]
public void UpdateGameStateClientRpc(GameStateData state)
{
    // All clients receive this
    // Update local game state representation
    localGameState = state;

    // Trigger UI updates, visual effects, etc.
    OnGameStateChanged?.Invoke(state);
}
```

### Targeted ClientRpc
```csharp
[ClientRpc]
public void ShowNotificationClientRpc(string message, ClientRpcParams clientRpcParams = default)
{
    // Only sent to specific clients via clientRpcParams
    ShowNotification(message);
}

// Calling with target:
var clientParams = new ClientRpcParams
{
    Send = new ClientRpcSendParams
    {
        TargetClientIds = new ulong[] { targetClientId }
    }
};
ShowNotificationClientRpc("You won!", clientParams);
```

### Common Use Cases
- Visual effect triggers (explosions, sounds)
- UI notifications (score updates, messages)
- State synchronization (game phase changes)
- Event broadcasting (player joined, objective completed)

## INetworkSerializable Pattern

### When to Use
**Any custom struct passed via RPC must implement INetworkSerializable.**

### Implementation Pattern
```csharp
public struct ActionData : INetworkSerializable
{
    public int ActionId;
    public Vector3 Position;
    public float Intensity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ActionId);
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Intensity);
    }
}

// Usage in RPC
[ServerRpc]
public void PerformActionServerRpc(ActionData data) { }
```

### Built-in Serializable Types
These work without INetworkSerializable:
- Primitives: `int`, `float`, `bool`, `string`
- Unity types: `Vector3`, `Quaternion`, `Color`
- Arrays of serializable types: `int[]`, `Vector3[]`

### Complex Data Pattern
```csharp
public struct InventoryUpdate : INetworkSerializable
{
    public int[] ItemIds;
    public int[] ItemCounts;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Serialize array length first
        int length = ItemIds?.Length ?? 0;
        serializer.SerializeValue(ref length);

        // Allocate if deserializing
        if (serializer.IsReader)
        {
            ItemIds = new int[length];
            ItemCounts = new int[length];
        }

        // Serialize array contents
        for (int i = 0; i < length; i++)
        {
            serializer.SerializeValue(ref ItemIds[i]);
            serializer.SerializeValue(ref ItemCounts[i]);
        }
    }
}
```

## Server Authority Validation

### CRITICAL: Never Trust Client Input

```csharp
[ServerRpc]
public void PurchaseItemServerRpc(int itemId, int cost)
{
    // ❌ WRONG - trusting client-provided cost
    if (playerGold >= cost)
    {
        playerGold -= cost;
        GiveItem(itemId);
    }

    // ✅ CORRECT - validate cost server-side
    int actualCost = GetItemCost(itemId); // Server-side lookup
    if (playerGold >= actualCost)
    {
        playerGold -= actualCost;
        GiveItem(itemId);
    }
}
```

### Validation Pattern
```csharp
[ServerRpc]
public void PerformActionServerRpc(ActionRequest request)
{
    // 1. Validate sender owns the object
    if (OwnerClientId != request.ClientId)
    {
        Debug.LogWarning("Client attempting action on non-owned object");
        return;
    }

    // 2. Validate action is possible
    if (!CanPerformAction(request.ActionType))
    {
        SendErrorClientRpc("Action not available", GetClientRpcParams(OwnerClientId));
        return;
    }

    // 3. Validate parameters
    if (request.TargetPosition.y > MaxHeight)
    {
        Debug.LogWarning("Invalid target position");
        return;
    }

    // 4. Execute validated action
    ExecuteAction(request);
}
```

## GameCreator Integration Patterns

### Character Movement RPC
```csharp
[ServerRpc]
public void MoveCharacterServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
{
    // Only owner can move their character (RequireOwnership = true by default)
    if (character.IsNetworkOwner)
    {
        character.Motion.MoveToDirection(direction);
    }
}
```

### Inventory Sync RPC
```csharp
[ServerRpc]
public void AddInventoryItemServerRpc(int itemId, int amount)
{
    // Server validates and modifies inventory
    var item = ItemDatabase.Get(itemId);
    if (item != null && amount > 0 && amount <= 100)
    {
        character.Inventory.Add(item, amount);

        // Sync to owner client
        SyncInventoryClientRpc(GetInventoryState(), GetClientRpcParams(OwnerClientId));
    }
}

[ClientRpc]
private void SyncInventoryClientRpc(InventoryState state, ClientRpcParams clientRpcParams = default)
{
    // Update local inventory representation
    UpdateLocalInventory(state);
}
```

### Visual Scripting RPC (GameCreator Instructions)
```csharp
// In a NetworkBehaviour component
[ServerRpc]
public void TriggerInstructionServerRpc(string instructionKey)
{
    // Execute GameCreator Instruction on server
    var instruction = InstructionDatabase.Get(instructionKey);
    _ = instruction.Run(character.gameObject);

    // Broadcast visual effects to all clients
    PlayEffectClientRpc(instructionKey);
}

[ClientRpc]
private void PlayEffectClientRpc(string effectKey)
{
    // Visual-only effects on clients
    PlayVisualEffect(effectKey);
}
```

## Common Pitfalls

### ❌ Forgetting RPC Suffix
```csharp
[ServerRpc]
public void DoSomething() { } // Won't compile - needs ServerRpc suffix
```

### ❌ RPC in Non-NetworkBehaviour
```csharp
public class MyClass // Not a NetworkBehaviour!
{
    [ServerRpc]
    public void DoThingServerRpc() { } // Won't work - must inherit NetworkBehaviour
}
```

### ❌ Calling RPC Before Spawn
```csharp
var obj = Instantiate(prefab);
obj.GetComponent<MyNetworkBehaviour>().DoSomethingServerRpc(); // ERROR - not spawned yet

// ✅ CORRECT
var obj = Instantiate(prefab);
obj.GetComponent<NetworkObject>().Spawn();
obj.GetComponent<MyNetworkBehaviour>().DoSomethingServerRpc();
```

### ❌ Non-Serializable RPC Parameter
```csharp
public class ComplexData { } // No INetworkSerializable

[ServerRpc]
public void SendDataServerRpc(ComplexData data) { } // Won't serialize!

// ✅ CORRECT
public struct ComplexData : INetworkSerializable { }
```

## Testing Pattern

```csharp
[UnityTest]
public IEnumerator ServerRpc_WhenCalled_ExecutesOnServer()
{
    // Arrange
    var networkBehaviour = SpawnNetworkBehaviour();
    bool serverExecuted = false;
    networkBehaviour.OnServerRpcExecuted += () => serverExecuted = true;

    // Act
    networkBehaviour.TestServerRpc();
    yield return new WaitForSeconds(0.1f); // Network round trip

    // Assert
    Assert.IsTrue(serverExecuted, "ServerRpc should execute on server");
}
```

## Quick Reference

| Pattern | Signature | Use Case |
|---------|-----------|----------|
| Basic ServerRpc | `[ServerRpc] void MethodServerRpc()` | Owner→Server action |
| Any-client ServerRpc | `[ServerRpc(RequireOwnership = false)]` | Non-owner→Server |
| Basic ClientRpc | `[ClientRpc] void MethodClientRpc()` | Server→All clients |
| Targeted ClientRpc | `MethodClientRpc(data, clientRpcParams)` | Server→Specific clients |
| Custom struct | `struct Data : INetworkSerializable` | Complex RPC parameters |

## Related Documentation

- `.serena/memories/CRITICAL/001_gamecreator_invasive_integration.md` - Character ownership
- `.serena/memories/CRITICAL/002_network_architecture_never_forget.md` - Architecture decisions
- `Assets/Plugins/GameCreator_Multiplayer/Runtime/` - Implementation examples
