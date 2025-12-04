# Design: Fix Netcode Perception Integration

## Context

The Game Creator Perception module was designed for single-player. The existing `NetworkPerception` component synchronizes awareness levels but not noise emissions. When a networked player performs actions that emit noise (jumping, landing, walking), only local NPCs respond.

### Current Architecture

```
[Player Lands] → [OnLand Trigger] → [InstructionPerceptionEmitNoise]
                                              ↓
                                    [SpatialHashPerception.Find()] (LOCAL ONLY)
                                              ↓
                                    [Local Perception.OnReceiveNoise()]
```

NPCs on remote clients never receive the noise because `SpatialHashPerception` only queries the local Unity instance.

## Goals

1. Fix character animation glitches caused by position manipulation conflicts
2. Enable network-synchronized noise emission
3. Provide real-time debug visibility for perception events
4. Maintain server-authoritative design for cheat prevention

## Non-Goals

- Replacing the local perception system (still valid for single-player)
- Changing the existing awareness synchronization
- Client-side prediction for noise (server is authoritative)

## Decisions

### Decision 1: Position Adjustment Strategy

**What**: Remove direct `Transform.localPosition` manipulation in `NetworkUnitDriverController`

**Why**: The current code modifies position to compensate for height changes, but this conflicts with NetworkTransform. The mannequin position adjustment (`ApplyMannequinPosition()`) already handles visual alignment.

**Alternative considered**: Queue position adjustments for after NetworkTransform sync - rejected as overly complex and the adjustment isn't necessary when NetworkTransform handles position.

### Decision 2: Server-Authoritative Noise

**What**: All network noise emissions go through ServerRpc → validation → ClientRpc broadcast

**Why**:
- Prevents clients from spamming fake noise to grief other players
- Server can rate-limit noise emissions
- Consistent with existing server-authoritative perception design

**Flow**:
```
[Client: Player Lands] → [InstructionNetworkEmitNoise]
                                    ↓
                         [NetworkNoiseEmitter.EmitNoiseServerRpc()]
                                    ↓
                         [Server: Validate + Store]
                                    ↓
                         [Server: BroadcastNoiseClientRpc()]
                                    ↓
                    [All Clients: SpatialHashPerception.Find() + OnReceiveNoise()]
```

### Decision 3: Centralized NetworkNoiseEmitter

**What**: Create a single `NetworkNoiseEmitter` NetworkBehaviour (spawned once) rather than adding RPCs to each Character

**Why**:
- Avoids adding more complexity to `NetworkCharacter`
- Single point for noise validation and rate limiting
- Can be spawned as a singleton by NetworkManager

**Alternative considered**: Add RPCs directly to `NetworkPerception` - rejected because noise emission is a separate concern from perception tracking.

### Decision 4: Debug UI Architecture

**What**: `PerceptionDebugUI` subscribes to static `NetworkPerceptionEvents` and displays formatted entries

**Why**:
- Decoupled from perception components
- Can be added/removed without affecting gameplay
- Static events allow global subscription without references

**Structure**:
```csharp
public class PerceptionDebugUI : MonoBehaviour
{
    [SerializeField] private ScrollRect m_ScrollView;
    [SerializeField] private PerceptionDebugUIItem m_ItemPrefab;
    [SerializeField] private int m_MaxEntries = 50;
    [SerializeField] private PerceptionDebugFilter m_Filter;

    private void OnEnable()
    {
        NetworkPerceptionEvents.EventNoiseHeard += OnNoiseHeard;
        NetworkPerceptionEvents.EventAwarenessChanged += OnAwarenessChanged;
    }
}
```

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Noise spam from malicious clients | Server-side rate limiting (max 10 noises/second per player) |
| Network latency causing delayed NPC response | Acceptable trade-off; NPCs respond within ~100ms is fine |
| Debug UI performance with many events | Max entry limit + pooling for UI items |
| Breaking existing OnLand triggers | Provide both instructions; existing triggers continue to work locally |

## Migration Plan

1. **Phase 1**: Fix `NetworkUnitDriverController` position issue (no migration needed)
2. **Phase 2**: Add `NetworkNoiseEmitter` and `InstructionNetworkEmitNoise`
3. **Phase 3**: Add `PerceptionDebugUI`
4. **Phase 4**: Update scene triggers to use network version

**Rollback**: Each phase is independent; can revert any phase without affecting others.

## Open Questions

1. Should `InstructionPerceptionEmitNoise` automatically detect network context and use RPC?
   - **Proposed**: No, keep separate instructions for explicit control

2. Should debug UI be enabled in release builds?
   - **Proposed**: Yes, but hidden by default with keyboard toggle (F8)

## Implementation Notes

### NetworkNoiseEmitter Singleton Pattern

```csharp
[RequireComponent(typeof(NetworkObject))]
public class NetworkNoiseEmitter : NetworkBehaviour
{
    public static NetworkNoiseEmitter Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    [Rpc(SendTo.Server)]
    public void EmitNoiseServerRpc(Vector3 position, float radius,
        FixedString32Bytes tag, float intensity, RpcParams rpcParams = default)
    {
        // Validate sender, rate limit, then broadcast
        BroadcastNoiseClientRpc(position, radius, tag, intensity);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void BroadcastNoiseClientRpc(Vector3 position, float radius,
        FixedString32Bytes tag, float intensity)
    {
        // Process through local SpatialHashPerception
        var perceptions = new List<ISpatialHash>();
        SpatialHashPerception.Find(position, radius, perceptions);

        foreach (var hash in perceptions)
        {
            if (hash is Perception perception)
            {
                var sensor = perception.GetSensor<SensorHear>();
                sensor?.OnReceiveNoise(new StimulusNoise(tag.ToString(), position, radius, intensity));
            }
        }

        // Fire event for UI
        NetworkPerceptionEvents.NotifyNoiseHeard(position, radius, tag.ToString(), intensity);
    }
}
```

### Debug Message Format

```
[12:34:56] Guard_01 heard Host_Player [footstep] at 8.2m
[12:34:57] Guard_02 heard Host_Player [footstep] at 12.5m
[12:34:58] Guard_01 awareness of Host_Player: 0.45 → Suspicious
[12:35:01] Guard_01 noticed Evidence_Corpse
```
