# Network Architecture: Never Forget These Decisions

**Priority:** P0 CRITICAL
**Last Updated:** 2025-11-23
**Applies To:** All multiplayer networking, character synchronization

## CRITICAL: NetworkTransform Removed from Player Prefabs

### The Decision
**We INTENTIONALLY removed NetworkTransform from Player_Network.prefab and all character prefabs.**

### Why This Is CRITICAL
You will see compilation errors about NetworkTransform. **THIS IS EXPECTED AND GOOD.**

```csharp
// This code WILL NOT compile - that's the point!
var netTransform = character.GetComponent<NetworkTransform>();
// ERROR: NetworkTransform component does not exist on character prefabs
```

### The Rationale

**Problem with NetworkTransform:**
1. **Conflicts with CharacterController:** Unity's CharacterController already handles physics and position
2. **Authority Confusion:** NetworkTransform wants to control position, but GameCreator's motion system already does
3. **Jitter and Fighting:** Two systems trying to control the same Transform causes visual artifacts
4. **Unnecessary Overhead:** We don't need full transform sync - character motion is command-based

**Our Solution:**
We use **NetworkCharacterAdapter** instead, which:
- Syncs character state (grounded, jumping, velocity) not raw transform
- Respects CharacterController as the source of truth
- Uses server-authoritative commands for movement
- Eliminates position fighting and jitter

### The Architecture

```
Player Prefab Components:
├── NetworkObject ✅ (for Netcode identity)
├── Character ✅ (GameCreator character)
├── CharacterController ✅ (Unity physics)
├── NetworkCharacterAdapter ✅ (our sync solution)
└── ❌ NO NetworkTransform (intentionally removed)
```

### Code Pattern

**❌ WRONG - Don't try to add NetworkTransform:**
```csharp
// DON'T DO THIS - it will conflict
var netTransform = character.gameObject.AddComponent<NetworkTransform>();
```

**✅ RIGHT - Use NetworkCharacterAdapter:**
```csharp
// This is the correct pattern
var adapter = character.GetComponent<NetworkCharacterAdapter>();
adapter.SyncCharacterState(); // Syncs via character state, not transform
```

### Expected Compilation Behavior

**If you see these errors, it means the architecture is WORKING:**
```
Error: 'NetworkTransform' component not found on Player_Network prefab
Error: Cannot find component 'NetworkTransform' on character
```

**These are GOOD errors - they prevent accidental re-introduction of NetworkTransform.**

### Migration Notes

**If old code references NetworkTransform:**

1. **Remove the NetworkTransform component reference**
2. **Replace with NetworkCharacterAdapter calls**
3. **Use character state sync instead of transform sync**

Example:
```csharp
// OLD (wrong):
var netTransform = GetComponent<NetworkTransform>();
netTransform.Teleport(position, rotation, transform.localScale);

// NEW (correct):
character.transform.position = position;
character.transform.rotation = rotation;
// NetworkCharacterAdapter automatically syncs character state
```

## Server Authority Pattern

### Movement Commands
All movement is **server-authoritative**:

```csharp
// Client sends command
[ServerRpc]
public void MoveServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
{
    // Server validates and executes
    if (IsValidMovement(direction))
    {
        character.Motion.MoveToDirection(direction);
        // NetworkCharacterAdapter syncs result to all clients
    }
}
```

### State Synchronization
Character state (not transform) is synced:

```csharp
// Synced via NetworkVariables
private NetworkVariable<bool> m_IsGrounded = new();
private NetworkVariable<Vector3> m_Velocity = new();

// Not syncing:
// - transform.position (CharacterController owns this)
// - transform.rotation (interpolated locally)
```

## IsNetworkSpawned Flag

### Critical Initialization Pattern

**ALWAYS set this flag before spawning:**
```csharp
// Step 1: Set the flag
character.IsNetworkSpawned = true;

// Step 2: Spawn with Netcode
var networkObject = character.GetComponent<NetworkObject>();
networkObject.Spawn();

// If you forget Step 1, character components won't know they're networked!
```

### Why It Matters
GameCreator components may initialize before NetworkObject.OnNetworkSpawn() is called. The flag tells them to defer network-specific setup.

## Performance Optimizations (UPDATED 2025-11-29)

### Optimization Patterns Applied
Reference: `code_optimizations_2025_11.md` for complete implementation details.

#### Network Authority Manager Singleton
```csharp
// Thread-safe singleton with performance optimizations
public class NetworkAuthorityManager : MonoBehaviour
{
    private static NetworkAuthorityManager _instance;
    private static readonly object _lock = new object();

    public static NetworkAuthorityManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindFirstObjectByType<NetworkAuthorityManager>();
                        if (_instance == null)
                        {
                            var go = new GameObject("NetworkAuthorityManager");
                            _instance = go.AddComponent<NetworkAuthorityManager>();
                            DontDestroyOnLoad(go);
                        }
                    }
                }
            }
            return _instance;
        }
    }

    // Optimized null checking with early returns
    public bool IsClientPlayer()
    {
        if (!NetworkManager.Singleton?.IsConnectedClient ?? true) return false;
        return !NetworkManager.Singleton.IsHost;
    }
}
```

#### RPC Batching and Pooling
```csharp
public class NetworkCharacterAdapter : NetworkBehaviour
{
    // Object pooling for network events
    private readonly Queue<MovementEvent> _movementPool = new();

    // Batch RPC processing for performance
    [ServerRpc(RequireOwnership = false)]
    private void ProcessMovementBatchServerRpc(MovementEvent[] events, ServerRpcParams rpcParams = default)
    {
        foreach (var evt in events)
        {
            ProcessMovement(evt);
            ReturnToPool(evt); // Pool management
        }
    }
}
```

#### HashSet for Listener Management
```csharp
public class EventBus : MonoBehaviour
{
    // O(1) lookups instead of List.Contains()
    private readonly HashSet<INetworkEventListener> _listeners = new();

    public void RegisterListener(INetworkEventListener listener)
    {
        if (listener != null && _listeners.Add(listener))
        {
            Debug.Log($"[EventBus] Registered listener: {listener.GetType().Name}");
        }
    }
}
```

## Architecture Decisions Log

### Decision 1: No NetworkTransform
- **Date:** 2025-11-16
- **Reason:** Conflicts with CharacterController, causes jitter
- **Alternative:** NetworkCharacterAdapter with state-based sync
- **Status:** Enforced via removed component

### Decision 2: Server Authority for Movement
- **Date:** 2025-11-16
- **Reason:** Prevent cheating, ensure consistency
- **Implementation:** ServerRpc for all movement commands
- **Status:** Active

### Decision 3: IsNetworkSpawned Flag
- **Date:** 2025-11-16
- **Reason:** GameCreator initialization timing issues
- **Implementation:** Manual flag before NetworkObject.Spawn()
- **Status:** Required for all character spawns

### Decision 4: Performance Optimizations (2025-11-29)
- **Date:** 2025-11-29
- **Reason:** Improve network performance and reduce allocations
- **Implementation:** Object pooling, RPC batching, HashSet lookups, early returns
- **Status:** Active - see `code_optimizations_2025_11.md`

## Testing Validation

**Test that validates NetworkTransform absence:**
```csharp
[Test]
public void PlayerPrefab_DoesNotHave_NetworkTransform()
{
    var prefab = Resources.Load<GameObject>("Player_Network");
    var netTransform = prefab.GetComponent<NetworkTransform>();

    Assert.IsNull(netTransform,
        "Player prefab must NOT have NetworkTransform - use NetworkCharacterAdapter instead");
}
```

## Related Documentation

- `.serena/memories/CRITICAL/001_gamecreator_invasive_integration.md` - IsNetworkOwner pattern
- `openspec/changes/player-prefab-networktransform-removal/` - Original change proposal
- `Assets/Plugins/GameCreator_Multiplayer/Runtime/Character/NetworkCharacterAdapter.cs` - Implementation

## Quick Reference

| Question | Answer |
|----------|--------|
| Should I add NetworkTransform? | **NO - intentionally removed** |
| Why compilation errors? | **Expected - prevents re-addition** |
| How to sync character? | Use **NetworkCharacterAdapter** |
| Movement authority? | **Server-authoritative via ServerRpc** |
| Before spawning character? | Set `IsNetworkSpawned = true` |

## Emergency Rollback

**If you absolutely must revert (NOT recommended):**

1. This breaks the architecture - only do if critical production issue
2. Add NetworkTransform back to prefab
3. Disable NetworkCharacterAdapter
4. Expect jitter, position fighting, and authority conflicts
5. **File a bug report immediately - this is a regression**
