# Design: Network Character Property System

## Context

GameCreator uses a polymorphic property system where any "Position" field can be configured to get its value from various sources (constant, transform, character, player, etc.). This is controlled via dropdown menus in the Inspector.

The current architecture:
- `PropertyGetPosition` is the field type used in components
- `PropertyTypeGetPosition` is the abstract base class for position providers
- Concrete implementations like `GetPositionCharactersPlayer` provide the actual values

For multiplayer, we need property providers that understand network ownership.

### Host vs Client Topology

Unity Netcode supports two player types that affect "local player" resolution:

| Type | Description | ClientId | IsHost | IsServer | IsClient |
|------|-------------|----------|--------|----------|----------|
| **Host** | Server + Client combined | 0 | true | true | true |
| **Client** | Pure client connection | 1, 2, 3... | false | false | true |

Both Host and Client have a "local player" - the NetworkCharacter they own and control. The key differentiator:

- **`NetworkObject.IsOwner`** - Returns true for the character owned by THIS client (works for both Host and Client)
- **`NetworkObject.OwnerClientId`** - The ClientId of the owner (0 for Host-owned, 1+ for Client-owned)
- **`NetworkManager.Singleton.LocalClientId`** - This client's ID (0 if Host, 1+ if Client)

The registry must handle:
1. **Host mode**: Host's local player is the one where `OwnerClientId == 0`
2. **Client mode**: Client's local player is the one where `OwnerClientId == LocalClientId`
3. **Dedicated Server**: No local player (server-only, no client)

## Goals

1. **Seamless Integration**: New property types appear in the same dropdowns as existing ones
2. **Zero Breaking Changes**: Existing projects continue working unchanged
3. **Performance**: No per-frame allocations or expensive lookups
4. **Editor Support**: Properties work in edit mode for preview

## Non-Goals

1. Modifying GameCreator Core code (beyond our existing NetworkCharacter extension)
2. Automatic migration of existing scenes (designers choose new properties manually)
3. Supporting non-Netcode networking solutions

## Decisions

### Decision 1: Use NetworkCharacterRegistry for Caching

**What**: Create a static registry that tracks all active NetworkCharacters.

**Why**: `FindObjectsByType<NetworkCharacter>()` is expensive. Characters self-register on spawn.

**Alternatives considered**:
- Per-property caching: Would duplicate cache across many property instances
- NetworkManager.SpawnedObjects iteration: Still requires type checking each object

### Decision 2: Separate Property Types (Not Extending Existing)

**What**: Create new `GetPositionNetworkLocalPlayer` rather than modifying `GetPositionCharactersPlayer`.

**Why**:
- Clear separation of single-player vs multiplayer behavior
- No risk of breaking existing single-player projects
- Designers explicitly opt into network behavior

**Alternatives considered**:
- Modifying `GetPositionCharactersPlayer` to detect multiplayer: Implicit behavior change, harder to debug
- Subclassing existing types: GameCreator's serialization relies on concrete type names

### Decision 3: Property Location in Netcode Package

**What**: All new properties go in `GameCreator.Netcode.Runtime.VisualScripting` namespace.

**Why**:
- Keeps network dependencies isolated
- Matches existing `GetGameObjectNetworkLocalPlayer` location
- Clear assembly boundary

## Architecture

```
GameCreator.Netcode.Runtime.VisualScripting/
├── Properties/
│   ├── GetGameObjectNetworkLocalPlayer.cs (existing)
│   ├── GetGameObjectNetworkCharacterByClientId.cs (new)
│   ├── GetPositionNetworkLocalPlayer.cs (new)
│   ├── GetRotationNetworkLocalPlayer.cs (new)
│   ├── GetScaleNetworkLocalPlayer.cs (new)
│   └── GetLocationNetworkLocalPlayer.cs (new)
├── Lists/
│   └── GetListGameObjectNetworkCharacters.cs (new)
└── Instructions/
    └── InstructionCollectNetworkCharacters.cs (new)

GameCreator.Netcode.Runtime/
└── Components/
    ├── NetworkCharacter.cs (existing)
    └── NetworkCharacterRegistry.cs (new - static utility)
```

## NetworkCharacterRegistry Design

```csharp
public static class NetworkCharacterRegistry
{
    private static readonly List<NetworkCharacter> s_Characters = new();
    private static readonly Dictionary<ulong, NetworkCharacter> s_CharactersByClientId = new();
    private static NetworkCharacter s_LocalPlayerCache;
    private static bool s_LocalPlayerCacheDirty = true;

    /// <summary>All registered NetworkCharacters (spawned and active)</summary>
    public static IReadOnlyList<NetworkCharacter> All => s_Characters;

    /// <summary>
    /// The local player's NetworkCharacter.
    /// - For Host: The character owned by ClientId 0
    /// - For Client: The character owned by this client's LocalClientId
    /// - For Dedicated Server: null (no local player)
    /// </summary>
    public static NetworkCharacter LocalPlayer
    {
        get
        {
            if (s_LocalPlayerCacheDirty)
            {
                s_LocalPlayerCache = FindLocalPlayer();
                s_LocalPlayerCacheDirty = false;
            }
            return s_LocalPlayerCache;
        }
    }

    /// <summary>True if running as Host (Server + Client, ClientId == 0)</summary>
    public static bool IsHost => NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;

    /// <summary>True if running as pure Client (not Host, ClientId > 0)</summary>
    public static bool IsClient => NetworkManager.Singleton != null &&
                                   NetworkManager.Singleton.IsClient &&
                                   !NetworkManager.Singleton.IsHost;

    /// <summary>True if running as Dedicated Server (no local player)</summary>
    public static bool IsDedicatedServer => NetworkManager.Singleton != null &&
                                            NetworkManager.Singleton.IsServer &&
                                            !NetworkManager.Singleton.IsClient;

    public static void Register(NetworkCharacter character)
    {
        if (!s_Characters.Contains(character))
        {
            s_Characters.Add(character);
            var networkObject = character.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                s_CharactersByClientId[networkObject.OwnerClientId] = character;
            }
            InvalidateLocalPlayerCache();
        }
    }

    public static void Unregister(NetworkCharacter character) { ... }

    public static NetworkCharacter GetByClientId(ulong clientId)
    {
        return s_CharactersByClientId.TryGetValue(clientId, out var character) ? character : null;
    }

    public static void InvalidateLocalPlayerCache() => s_LocalPlayerCacheDirty = true;

    private static NetworkCharacter FindLocalPlayer()
    {
        if (NetworkManager.Singleton == null) return null;
        if (IsDedicatedServer) return null; // No local player on dedicated server
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        return GetByClientId(localClientId);
    }

    // Clean up on domain reload / scene unload
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState() { ... }
}
```

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Registry not cleaned up on scene unload | NetworkCharacter.OnDestroy calls Unregister |
| Local player cache stale after ownership transfer | Subscribe to NetworkObject.OnOwnershipChanged |
| Multiple NetworkCharacters claim IsPlayer | Registry tracks only the one with IsLocalOwner |

## Migration Plan

1. **Phase 1**: Add new property types (no changes to existing behavior)
2. **Phase 2**: Update example scenes to demonstrate usage
3. **Phase 3**: Document best practices for multiplayer property selection

No rollback needed - changes are purely additive.

## Open Questions

None - design follows established GameCreator patterns.
