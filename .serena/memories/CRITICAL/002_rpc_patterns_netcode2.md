# RPC Patterns - Netcode 2.x

**Priority:** P0 CRITICAL
**Last Updated:** 2025-12-03
**Requires:** Unity Netcode for GameObjects 2.x (UNITY_NETCODE_2 define)

## New Rpc Attribute (Preferred)

Netcode 2.x introduces a unified `[Rpc]` attribute that replaces the old `[ServerRpc]` and `[ClientRpc]`.

### Basic Syntax

```csharp
// Client → Server
[Rpc(SendTo.Server)]
private void DoActionServerRpc()
{
    // Runs on server
}

// Server → All Clients
[Rpc(SendTo.Everyone)]
private void NotifyAllClientsRpc()
{
    // Runs on all clients
}

// Server → Owner Only
[Rpc(SendTo.Owner)]
private void NotifyOwnerRpc()
{
    // Runs only on owner client
}
```

### SendTo Options

| Option | Direction | Description |
|--------|-----------|-------------|
| `SendTo.Server` | Client → Server | Standard server RPC |
| `SendTo.Everyone` | Server → All | Broadcast to all clients |
| `SendTo.Owner` | Server → Owner | Only to owning client |
| `SendTo.NotOwner` | Server → Others | All clients except owner |
| `SendTo.ClientsAndHost` | Server → All | Includes host client |
| `SendTo.Me` | Self | Local execution only |

### Ownership and Parameters

```csharp
// Allow non-owners to call (default requires ownership)
[Rpc(SendTo.Server, RequireOwnership = false)]
private void RequestRpc(RpcParams rpcParams = default)
{
    // Get sender info
    ulong senderId = rpcParams.Receive.SenderClientId;
    
    // Validate and process
    if (CanProcessRequest(senderId))
    {
        ProcessRequest();
    }
}
```

### With Custom Data

```csharp
// Send data with RPC
[Rpc(SendTo.Server)]
private void SendDataServerRpc(int value, Vector3 position)
{
    // Process on server
}

// With network serializable struct
[Rpc(SendTo.Everyone)]
private void SyncStateRpc(MyNetworkData data)
{
    // Apply state on all clients
}

// Custom struct must implement INetworkSerializable
public struct MyNetworkData : INetworkSerializable
{
    public int Value;
    public Vector3 Position;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) 
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref Value);
        serializer.SerializeValue(ref Position);
    }
}
```

## Legacy Syntax (Still Supported)

The old `[ServerRpc]` and `[ClientRpc]` attributes still work:

```csharp
// Legacy server RPC
[ServerRpc]
private void OldStyleServerRpc()
{
    // Must end with "ServerRpc"
}

// Legacy client RPC
[ClientRpc]
private void OldStyleClientRpc()
{
    // Must end with "ClientRpc"
}

// With require ownership = false
[ServerRpc(RequireOwnership = false)]
private void RequestServerRpc(ServerRpcParams rpcParams = default)
{
    ulong senderId = rpcParams.Receive.SenderClientId;
}
```

## Common Patterns

### Request/Response

```csharp
// Client requests action
[Rpc(SendTo.Server, RequireOwnership = false)]
private void RequestActionRpc(int actionId, RpcParams rpcParams = default)
{
    ulong clientId = rpcParams.Receive.SenderClientId;
    
    if (ValidateRequest(clientId, actionId))
    {
        ExecuteAction(actionId);
        ConfirmActionRpc(actionId); // Notify all
    }
}

// Server confirms to all clients
[Rpc(SendTo.Everyone)]
private void ConfirmActionRpc(int actionId)
{
    // Update UI, play effects, etc.
}
```

### Ownership Transfer

```csharp
[Rpc(SendTo.Server, RequireOwnership = false)]
private void RequestOwnershipRpc(RpcParams rpcParams = default)
{
    ulong requesterId = rpcParams.Receive.SenderClientId;
    
    // Server validates and transfers
    NetworkObject.ChangeOwnership(requesterId);
}
```

## Do's and Don'ts

✅ **DO:**
- Use `[Rpc(SendTo.X)]` for new code
- Add `RpcParams rpcParams = default` when you need sender info
- Use `RequireOwnership = false` for non-owner requests
- Validate all incoming RPC data on server

❌ **DON'T:**
- Mix old and new RPC syntax in same class
- Trust client data without validation
- Forget to handle network serialization for custom types
- Skip ownership checks when required

## Related Files

- `.serena/personas/netcode-core.llm.txt`
- `.serena/memories/critical_rules.md`
