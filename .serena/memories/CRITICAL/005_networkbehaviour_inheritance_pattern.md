# NetworkBehaviour Inheritance Pattern: Avoid Field Conflicts

**Priority:** P0 CRITICAL
**Last Updated:** 2025-11-27
**Applies To:** All Unity Netcode multiplayer code, NetworkBehaviour subclasses

## CRITICAL: Never Redeclare Base Class Fields

### The Problem

When you inherit from `NetworkBehaviour`, Unity's serialization system includes all fields from the base class. If you declare a field with the same name as a base class property (even with `m_` prefix), Unity will throw a serialization conflict error.

**Common Error Message:**
```
SerializationException: Duplicate field name 'm_NetworkObject' in type hierarchy
```

### The Anti-Pattern (DO NOT DO THIS)

```csharp
public class MyController : NetworkBehaviour
{
    // ❌ WRONG: Conflicts with NetworkBehaviour.NetworkObject property!
    [SerializeField] private NetworkObject m_NetworkObject;

    // ❌ WRONG: Shadows inherited property!
    private bool m_IsOwner;

    // ❌ WRONG: Duplicates base class functionality!
    private ulong m_OwnerClientId;
}
```

### The Correct Pattern

```csharp
public class MyController : NetworkBehaviour
{
    // ✅ Use unique, descriptive names for YOUR fields
    private Character m_Character;
    private Animator m_Animator;
    private NetworkVariable<FixedString64Bytes> m_PlayerName = new();

    public override void OnNetworkSpawn()
    {
        // ✅ Use inherited properties directly - they're already there!
        if (IsOwner)
        {
            Debug.Log($"I own this! ClientId: {OwnerClientId}");
        }

        // ✅ Access NetworkObject via inherited property
        Debug.Log($"NetworkObjectId: {NetworkObject.NetworkObjectId}");
    }
}
```

## Inherited Properties (DO NOT REDECLARE)

These properties come from `NetworkBehaviour` automatically:

| Property | Type | What It Does |
|----------|------|--------------|
| `NetworkObject` | `NetworkObject` | The NetworkObject for this behaviour |
| `NetworkManager` | `NetworkManager` | Reference to NetworkManager singleton |
| `IsOwner` | `bool` | True if local client owns this |
| `IsServer` | `bool` | True if running on server |
| `IsClient` | `bool` | True if running on client |
| `IsHost` | `bool` | True if host (server + client) |
| `IsLocalPlayer` | `bool` | True if local player object |
| `OwnerClientId` | `ulong` | Client ID of owner |
| `IsSpawned` | `bool` | True if NetworkObject spawned |
| `HasNetworkObject` | `bool` | True if NetworkObject valid |

## Quick Fix Checklist

If you see Unity serialization errors in a NetworkBehaviour subclass:

1. ✅ Check for fields named `m_NetworkObject`, `m_IsOwner`, `m_IsServer`, `m_IsClient`, `m_OwnerClientId`
2. ✅ Remove any such fields - use `this.PropertyName` instead
3. ✅ Use unique names for YOUR component references (e.g., `m_Character`, `m_Weapon`)
4. ✅ Use `NetworkVariable<T>` for synced state, not cached local copies

## AI Assistant Instructions

When generating NetworkBehaviour subclasses:

1. **NEVER** add these fields: `m_NetworkObject`, `m_IsOwner`, `m_IsServer`, `m_IsClient`, `m_OwnerClientId`
2. **ALWAYS** use inherited properties: `this.NetworkObject`, `this.IsOwner`, etc.
3. **USE** descriptive, unique names for custom fields: `m_Character`, `m_PlayerName`, `m_TeamId`
4. **VERIFY** field names don't shadow base class properties

## Related Documentation

- `.serena/memories/CRITICAL/002_network_architecture_never_forget.md` - NetworkTransform removal
- `openspec/changes/add-networkbehaviour-inheritance-guidelines/` - Formal specification
- `Assets/Plugins/GameCreator_Multiplayer/Runtime/Components/NetworkPlayerController.cs` - Correct example

## Example: NetworkPlayerController (Correct Implementation)

Our `NetworkPlayerController` correctly uses unique field names:

```csharp
// ✅ Unique names for OUR components
private Character m_Character;
private Animator m_Animator;

// ✅ Unique names for module references
private NetworkCharacterAdapter m_CharacterAdapter;
private NetworkGameCreatorAnimator m_AnimatorSync;

// ✅ Descriptive NetworkVariable names
private NetworkVariable<FixedString64Bytes> m_PlayerName = new();
private NetworkVariable<bool> m_IsReady = new();
private NetworkVariable<int> m_TeamId = new();

// ✅ Uses inherited properties, not cached copies
public override void OnNetworkSpawn()
{
    if (IsOwner)  // ✅ Inherited property
    {
        Debug.Log($"Owner: {OwnerClientId}");  // ✅ Inherited property
    }
}
```
