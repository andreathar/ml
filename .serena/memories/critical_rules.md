# Critical Project Rules (P0)

**Project:** ML - GameCreator Core + Netcode Integration
**Last Updated:** 2025-12-03

## File Organization - ZERO ROOT POLLUTION
- AI reports → `claudedocs/reports/`
- AI action items → `claudedocs/action-items/`
- AI guides → `claudedocs/guides/`
- User docs → `docs/`
- Temp files → `.temp/`

## Assembly References

**GameCreator.Netcode.Runtime** - Primary networking assembly:
- `NetworkCharacterAdapter` - Character ↔ NetworkBehaviour bridge
- `NetworkCharacter` - Extended Character with IsNetworkSpawned
- `NetworkVariablesSync` - Variables synchronization
- `NetworkCharacterMotion` - Motion synchronization

**Namespace:** `GameCreator.Netcode.Runtime`

## Network Architecture

### NetworkCharacterAdapter Pattern
```csharp
// Bridge between GameCreator Character and Unity Netcode
[RequireComponent(typeof(NetworkObject))]
public class NetworkCharacterAdapter : NetworkBehaviour
{
    public Character Character { get; }
    public NetworkCharacter NetworkCharacter { get; }
    public new bool IsLocalPlayer => IsOwner && Character.IsPlayer;
}
```

### Ownership Checks
```csharp
// Use IsOwner from NetworkBehaviour
if (IsOwner)
{
    // Local player logic
}

// Or use NetworkCharacterAdapter
if (networkCharacterAdapter.IsLocalPlayer)
{
    // Local player logic
}
```

### Spawn Lifecycle
```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();
    
    if (IsOwner)
    {
        // Local player initialization
        networkCharacter.BecomeLocalPlayer();
    }
    else
    {
        // Remote player initialization
        networkCharacter.BecomeRemotePlayer();
    }
}
```

## RPC Patterns (Netcode 2.x)

### New Rpc Attribute Syntax
```csharp
// Server RPC (client → server)
[Rpc(SendTo.Server)]
private void MyServerRpc() { }

// Client RPC (server → clients)
[Rpc(SendTo.Everyone)]
private void MyClientRpc() { }

// With params
[Rpc(SendTo.Server, RequireOwnership = false)]
private void RequestRpc(RpcParams rpcParams = default)
{
    ulong senderId = rpcParams.Receive.SenderClientId;
}
```

### Legacy ServerRpc/ClientRpc (still supported)
```csharp
[ServerRpc]
private void OldStyleServerRpc() { }

[ClientRpc]
private void OldStyleClientRpc() { }
```

## GameCreator Visual Scripting Signatures

### Instructions (EXACT - NO CancellationToken)
```csharp
protected override Task Run(Args args)
{
    // Implementation
    return DefaultResult;
}
```

### Conditions
```csharp
protected override bool Check(Args args)
{
    return condition;
}
```

## NetworkVariablesSync Usage

```csharp
// Sync modes
public enum NetworkSyncMode
{
    Everyone,    // All clients can read
    OwnerOnly,   // Only owner can read/write
    ServerOnly   // Only server can read/write
}

// Setting values
networkVariablesSync.SetFloat("health", 100f);
networkVariablesSync.SetInt("score", 50);
networkVariablesSync.SetBool("isReady", true);

// Getting values
float health = networkVariablesSync.GetFloat("health");
int score = networkVariablesSync.GetInt("score");
bool ready = networkVariablesSync.GetBool("isReady");
```

## Key Visual Scripting Components

### Instructions
- `InstructionNetworkStartHost` - Start hosting
- `InstructionNetworkStartClient` - Connect as client
- `InstructionNetworkSpawnPlayer` - Spawn player character
- `InstructionNetworkDespawn` - Despawn network object
- `InstructionNetworkChangeOwnership` - Transfer ownership
- `InstructionNetworkDisconnect` - Disconnect

### Conditions
- `ConditionNetworkIsHost` / `IsClient` / `IsServer`
- `ConditionNetworkIsOwner` / `IsLocalPlayer`
- `ConditionNetworkIsConnected` / `IsSpawned`

### Triggers
- `EventNetworkOnClientConnected` / `Disconnected`
- `EventNetworkOnLocalPlayerSpawned`
- `EventNetworkOnNetworkSpawn` / `Despawn`
- `EventNetworkOnOwnershipChanged`

## NEVER Do

- ❌ Add `CancellationToken` to Instruction.Run()
- ❌ Use old `ServerRpc`/`ClientRpc` attributes without understanding new `Rpc` syntax
- ❌ Create files at project root (use proper directories)
- ❌ Ignore ownership checks in multiplayer code
- ❌ Skip `OnNetworkSpawn()` lifecycle

## ALWAYS Do

- ✅ Use `NetworkCharacterAdapter` for character networking
- ✅ Check `IsOwner` before local player logic
- ✅ Use `Rpc(SendTo.Server)` for new RPCs
- ✅ Implement `OnNetworkSpawn()` / `OnNetworkDespawn()` properly
- ✅ Use `NetworkVariablesSync` for variable synchronization
