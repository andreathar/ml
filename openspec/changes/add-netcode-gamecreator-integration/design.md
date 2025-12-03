# Design: Netcode for GameObjects Integration with GameCreator

## Context

GameCreator 2 is a visual scripting framework for Unity that provides Character, Motion, Driver, and other systems. Unity Netcode for GameObjects (NGO) is the official Unity multiplayer networking solution. Integrating these two systems requires careful handling of:

1. **CharacterController conflicts**: GameCreator's UnitDriverController resets `CharacterController.center` every frame, conflicting with NetworkTransform positioning
2. **Ownership model**: GameCreator assumes local control, NGO uses server-authoritative or distributed authority
3. **Variable sync**: GameCreator Local Variables need to synchronize across network
4. **Visual scripting**: Network operations need to be accessible via Instructions/Conditions

**Stakeholders**: Game developers using GameCreator for multiplayer games

**Constraints**:
- Cannot modify read-only GameCreator packages installed via Unity Package Manager
- Must maintain backward compatibility with single-player GameCreator projects
- Must follow NGO best practices (server authority, client prediction)

## Goals / Non-Goals

### Goals
- Provide editable source files for GameCreator-Netcode integration
- Enable multiplayer character synchronization with minimal setup
- Preserve all GameCreator Character features (motion, IK, ragdoll, footsteps)
- Provide visual scripting nodes for common network operations
- Support both host/client and dedicated server topologies

### Non-Goals
- Modifying the original GameCreator Core package
- Supporting alternative networking solutions (Mirror, Photon, etc.)
- Automated network optimization (bandwidth, tick rate tuning)
- Lobby/matchmaking systems (out of scope for this package)

## Decisions

### Decision 1: Inheritance vs Composition for Character Override

**Decision**: Use **inheritance** with a `NetworkCharacter` class extending `Character`

**Alternatives considered**:
1. **Composition** (wrapper component): Would require proxying all Character properties, complex lifecycle management
2. **Partial classes**: GameCreator doesn't use partial classes, would require source modification
3. **Reflection/patching**: Fragile, breaks with GameCreator updates

**Rationale**: Inheritance allows:
- Adding `IsNetworkSpawned` as a virtual property
- Overriding `Awake()` to handle network spawn timing
- Clean integration with existing Character API
- Future extensibility for other network features

### Decision 2: Driver Override Strategy

**Decision**: Create `NetworkUnitDriverController` extending `UnitDriverController`

```csharp
protected override void UpdatePhysicProperties()
{
    // Skip CharacterController.center reset for networked characters
    if (Character is NetworkCharacter nc && nc.IsNetworkSpawned)
    {
        // Only update height/radius, not center
        UpdateHeightAndRadius();
        return;
    }

    base.UpdatePhysicProperties();
}
```

**Rationale**: Minimal override that targets the exact conflict point (line 264-267 in original).

### Decision 3: Motion Synchronization Architecture

**Decision**: Use **NetworkVariables** for continuous state, **RPCs** for commands

| Data Type | Sync Method | Direction |
|-----------|-------------|-----------|
| Position/Rotation | NetworkTransform | Owner → All |
| MoveDirection | NetworkVariable<Vector3> | Owner → All |
| LinearSpeed | NetworkVariable<float> | Owner → All |
| IsGrounded | NetworkVariable<bool> | Owner → All |
| Jump command | ServerRpc | Client → Server |
| Dash command | ServerRpc | Client → Server |

**Alternatives considered**:
- Full state RPC (high bandwidth, laggy)
- Custom serialization (complex, error-prone)

### Decision 4: Assembly Structure

**Decision**: Two assemblies - Runtime and Editor

```
GameCreator.Netcode.Runtime.asmdef
├── References:
│   ├── Unity.Netcode.Runtime
│   ├── Unity.Netcode.Components
│   ├── GameCreator.Runtime.Characters
│   ├── GameCreator.Runtime.Common
│   └── GameCreator.Runtime.VisualScripting
└── Platforms: Any

GameCreator.Netcode.Editor.asmdef
├── References:
│   ├── GameCreator.Netcode.Runtime
│   └── GameCreator.Editor.*
└── Platforms: Editor only
```

### Decision 5: IsNetworkSpawned Flag Lifecycle

**Decision**: Set `IsNetworkSpawned = true` in `NetworkCharacterAdapter.OnNetworkSpawn()`

```csharp
public class NetworkCharacterAdapter : NetworkBehaviour
{
    private NetworkCharacter m_Character;

    public override void OnNetworkSpawn()
    {
        m_Character = GetComponent<NetworkCharacter>();
        if (m_Character != null)
        {
            m_Character.IsNetworkSpawned = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (m_Character != null)
        {
            m_Character.IsNetworkSpawned = false;
        }
    }
}
```

**Rationale**: Ties flag to actual network state, automatically handles spawn/despawn.

## Risks / Trade-offs

### Risk 1: GameCreator Updates Break Integration
- **Impact**: High - integration may stop working after GC updates
- **Mitigation**:
  - Pin GameCreator version in package manifest
  - Document known compatible versions
  - Use minimal surface area for overrides

### Risk 2: Performance Overhead from NetworkVariables
- **Impact**: Medium - extra network traffic for motion state
- **Mitigation**:
  - Use compressed NetworkVariable types where possible
  - Implement dead reckoning/interpolation on clients
  - Make sync rate configurable

### Risk 3: Complexity for End Users
- **Impact**: Medium - requires understanding both GC and NGO
- **Mitigation**:
  - Provide ready-to-use prefab templates
  - Create visual scripting nodes for common operations
  - Write comprehensive setup documentation

## Migration Plan

### For Existing Single-Player Projects

1. **No changes required** - existing Character components continue to work
2. Optional: Swap Character for NetworkCharacter on prefabs intended for multiplayer

### For New Multiplayer Projects

1. Import Netcode for GameObjects Integration package
2. Replace `Character` with `NetworkCharacter` on player prefabs
3. Add `NetworkObject`, `NetworkTransform`, `NetworkAnimator` components
4. Add `NetworkCharacterAdapter` component
5. Use `Player_Network.prefab` as template

### Rollback

- Remove NetworkCharacter components, revert to standard Character
- Remove NetworkCharacterAdapter components
- No data migration required (stateless)

## Open Questions

1. **Should we support relay/transport abstraction?** - Currently assumes direct NGO transport
2. **How to handle GameCreator States with network?** - State changes may need sync
3. **What about Inventory/Stats modules?** - Future work, not in initial scope
