# Slash Commands Reference

**Category:** TOOLS - Workflow Automation
**Last Updated:** 2025-11-23
**Source:** `.claude/commands/` directory

## GameCreator Multiplayer Commands

### /gc:multiplayer-component

**Purpose:** Scaffold complete NetworkBehaviour for GameCreator with RPC patterns, variable sync, and proper namespaces.

**Usage:**
```
/gc:multiplayer-component ComponentName
```

**Output:**
- NetworkBehaviour class with GameCreator namespace
- Proper RPC method templates (ServerRpc/ClientRpc)
- NetworkVariable examples with callbacks
- Character integration pattern
- IsNetworkOwner checks

**Example:**
```csharp
// Generated from: /gc:multiplayer-component PlayerInventorySync

using Unity.Netcode;
using GameCreator.Runtime.Characters;
using GameCreator.Multiplayer.Runtime;

namespace GameCreator.Multiplayer.Runtime.Inventory
{
    public class PlayerInventorySync : NetworkBehaviour
    {
        private NetworkVariable<int> m_ItemCount = new();

        [ServerRpc]
        public void AddItemServerRpc(int itemId, ServerRpcParams rpcParams = default)
        {
            // Server logic
        }

        [ClientRpc]
        private void SyncInventoryClientRpc(ClientRpcParams clientRpcParams = default)
        {
            // Client sync
        }
    }
}
```

### /gc:rpc-pattern

**Purpose:** Generate RPC method with smart targeting, proper naming, and INetworkSerializable structs.

**Usage:**
```
/gc:rpc-pattern RPCName [ServerRpc|ClientRpc]
```

**Output:**
- Properly named RPC method
- INetworkSerializable struct if needed
- Smart targeting with ClientRpcParams (for ClientRpc)
- Server validation pattern (for ServerRpc)

**Example (ServerRpc):**
```csharp
public struct MoveData : INetworkSerializable
{
    public Vector3 Direction;
    public float Speed;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Direction);
        serializer.SerializeValue(ref Speed);
    }
}

[ServerRpc]
public void MoveCharacterServerRpc(MoveData data, ServerRpcParams rpcParams = default)
{
    // Validate
    if (!IsValidMoveData(data)) return;

    // Execute
    character.Motion.MoveToDirection(data.Direction);
}
```

### /gc:visual-script

**Purpose:** Create GameCreator Instruction/Condition pair for visual scripting with proper Task signature.

**Usage:**
```
/gc:visual-script InstructionName
```

**Output:**
- Instruction class with `Task Run(Args args)` signature
- Corresponding Condition class with `bool Check(Args args)`
- Proper namespaces and using directives
- Icon attribution from GameCreator
- Category tags

**Example:**
```csharp
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using System.Threading.Tasks;

namespace GameCreator.Multiplayer.Runtime.VisualScripting
{
    [Title("Network Spawn Character")]
    [Category("Multiplayer/Character")]
    [Image(typeof(IconPlayer))]
    public class InstructionNetworkSpawn : Instruction
    {
        protected override Task Run(Args args)
        {
            // No CancellationToken!
            Character character = args.Self.Get<Character>();
            character.IsNetworkSpawned = true;

            var networkObject = character.GetComponent<NetworkObject>();
            networkObject.Spawn();

            return DefaultResult;
        }
    }

    public class ConditionIsNetworkSpawned : Condition
    {
        protected override bool Check(Args args)
        {
            Character character = args.Self.Get<Character>();
            return character != null && character.IsNetworkSpawned;
        }
    }
}
```

## Unity Netcode Commands

### /netcode:rpc

**Purpose:** Generate pure Unity Netcode ClientRpc/ServerRpc with proper naming and serialization.

**Usage:**
```
/netcode:rpc RPCName [ServerRpc|ClientRpc]
```

**Output:**
- Standalone RPC method (no GameCreator dependencies)
- Proper RPC attribute
- INetworkSerializable struct template
- Validation pattern for ServerRpc

**Example:**
```csharp
[ServerRpc(RequireOwnership = true)]
public void PerformActionServerRpc(ActionData data, ServerRpcParams rpcParams = default)
{
    // Validate
    if (!ValidateAction(data))
    {
        Debug.LogWarning($"Invalid action from client {rpcParams.Receive.SenderClientId}");
        return;
    }

    // Execute
    ExecuteAction(data);

    // Broadcast result
    NotifyActionClientRpc(data);
}

public struct ActionData : INetworkSerializable
{
    public int ActionId;
    public Vector3 Position;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ActionId);
        serializer.SerializeValue(ref Position);
    }
}
```

### /netcode:sync-variable

**Purpose:** Create NetworkVariable<T> with validation, callbacks, and proper permissions.

**Usage:**
```
/netcode:sync-variable VariableName Type
```

**Output:**
- NetworkVariable declaration
- OnValueChanged callback
- Property wrapper with validation
- Write permission settings

**Example:**
```csharp
private NetworkVariable<int> m_Health = new NetworkVariable<int>(
    value: 100,
    readPerm: NetworkVariableReadPermission.Everyone,
    writePerm: NetworkVariableWritePermission.Server
);

public int Health
{
    get => m_Health.Value;
    set
    {
        if (!IsServer) return;

        // Validate
        value = Mathf.Clamp(value, 0, MaxHealth);

        m_Health.Value = value;
    }
}

public override void OnNetworkSpawn()
{
    m_Health.OnValueChanged += OnHealthChanged;
}

public override void OnNetworkDespawn()
{
    m_Health.OnValueChanged -= OnHealthChanged;
}

private void OnHealthChanged(int previous, int current)
{
    Debug.Log($"Health changed: {previous} → {current}");

    // Update UI, trigger effects, etc.
    if (current <= 0)
    {
        OnCharacterDied();
    }
}
```

## Slash Command Best Practices

### When to Use Slash Commands

**✅ Use slash commands for:**
- Scaffolding new NetworkBehaviour components
- Generating boilerplate RPC methods
- Creating GameCreator visual scripting classes
- Setting up NetworkVariable patterns

**❌ Don't use slash commands for:**
- One-off code modifications
- Complex custom logic (command output is template)
- Non-multiplayer code
- Debugging existing code

### Customizing Generated Code

**Slash commands generate templates** - you MUST customize:

1. Replace placeholder logic with actual implementation
2. Add validation specific to your use case
3. Integrate with existing systems
4. Add error handling
5. Write tests

**Example customization:**
```csharp
// Generated by /gc:rpc-pattern
[ServerRpc]
public void PerformActionServerRpc(ActionData data)
{
    // TODO: Implement validation ← REPLACE THIS
    // TODO: Execute action         ← REPLACE THIS
}

// After customization
[ServerRpc]
public void PerformActionServerRpc(ActionData data)
{
    // ✅ Real validation
    if (!character.IsAlive || !CanPerformAction(data.ActionId))
    {
        SendErrorClientRpc("Action not available", GetClientRpcParams(OwnerClientId));
        return;
    }

    // ✅ Real implementation
    var action = ActionDatabase.Get(data.ActionId);
    action.Execute(character, data.Target);

    // ✅ Sync to clients
    NotifyActionClientRpc(data);
}
```

## Command Output Locations

All slash commands generate code **in-place** where you request it, typically:

```
Assets/Plugins/GameCreator_Multiplayer/Runtime/
├── Character/          # Character-related components
├── Networking/         # Pure networking code
├── VisualScripting/    # GameCreator Instructions/Conditions
└── Player/             # Player-specific systems
```

## Related Documentation

- `.serena/memories/CRITICAL/003_multiplayer_rpc_patterns.md` - RPC conventions
- `.serena/memories/CRITICAL/004_visual_scripting_task_signatures.md` - Task signatures
- `.claude/commands/` - Slash command implementations

## Quick Reference

| Command | Purpose | Output |
|---------|---------|--------|
| `/gc:multiplayer-component` | Full NetworkBehaviour scaffold | Class with RPCs + NetworkVariables |
| `/gc:rpc-pattern` | Single RPC method | ServerRpc or ClientRpc with struct |
| `/gc:visual-script` | Visual scripting pair | Instruction + Condition |
| `/netcode:rpc` | Pure Netcode RPC | Standalone RPC method |
| `/netcode:sync-variable` | NetworkVariable pattern | NetworkVariable + callbacks |
