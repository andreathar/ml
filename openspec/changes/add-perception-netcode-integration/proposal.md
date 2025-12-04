# Perception Netcode Integration

## Summary

Integrate the GameCreator Perception module with Unity Netcode for GameObjects to enable multiplayer AI awareness, sensory detection, and evidence tracking across networked games.

## Motivation

The existing Netcode integration successfully handles character synchronization, spawning, and visual scripting events. However, the Perception module—which provides AI awareness, hearing, sight, smell, and evidence detection—operates locally and is unaware of network context. In a multiplayer game:

1. **Awareness State Divergence**: Each client runs Perception independently, leading to different AI awareness levels across clients
2. **Sensory Event Desync**: Noise/scent stimuli only affect local Perception components, causing inconsistent AI behavior
3. **Evidence State Conflicts**: Evidence tampering state is local-only, creating gameplay inconsistencies
4. **Server Authority Missing**: No mechanism for server-authoritative AI perception decisions

## Analysis of Existing Netcode Integration Pattern

The successful Netcode integration follows these key patterns that we should replicate:

### 1. Component Architecture
```
NetworkCharacter (extends Character)
├── Wraps base component with network-aware properties
├── IsNetworkSpawned flag for conditional behavior
├── IsLocalOwner for ownership checks
├── Deferred initialization for network spawn timing
└── Network lifecycle callbacks (OnNetworkSpawn/Despawn)
```

### 2. Global Event Broadcasting (NetworkSessionEvents)
```csharp
// Static events that visual scripting triggers can subscribe to
public static event Action<NetworkCharacter> EventLocalPlayerSpawned;
public static event Action<NetworkCharacter> EventNPCSpawned;

// Called by network components to broadcast
public static void NotifyPlayerSpawned(NetworkCharacter character, bool isLocalOwner)
```

### 3. Registry Pattern (NetworkCharacterRegistry)
```csharp
// Efficient lookup by ClientId, NetworkObjectId, or InstanceId
public static NetworkCharacter GetPlayerByClientId(ulong clientId)
public static IReadOnlyList<NetworkCharacter> AllNPCs { get; }
```

### 4. Visual Scripting Events
```csharp
// Subscribe to global static events
protected internal override void OnEnable(Trigger trigger)
{
    NetworkSessionEvents.EventLocalPlayerSpawned += OnLocalPlayerSpawned;
}

// Set Target in Args for downstream instructions
private void OnLocalPlayerSpawned(NetworkCharacter player)
{
    this.m_Args = new Args(this.m_Trigger.gameObject, player.gameObject);
    _ = this.m_Trigger.Execute(this.m_Args);
}
```

## Perception Module Architecture Analysis

### Core Components
| Component | Purpose | Network Sync Needs |
|-----------|---------|-------------------|
| `Perception` | Main component, manages Cortex + Sensors | Server-authoritative state |
| `Cortex` | Tracks awareness per-target via Trackers | Awareness levels, stage changes |
| `Tracker` | Per-target awareness (0-1 float, stage enum) | Float value, last increase time |
| `TSensor` | Base sensor class | Active state |
| `SensorHear` | Noise detection | Noise intensity, position |
| `SensorSee` | Visual detection | Target visibility |
| `SensorSmell` | Scent detection | Scent intensity |
| `SensorFeel` | Proximity detection | Detection state |
| `Evidence` | Investigation targets | IsTampered state |

### Key State to Synchronize
1. **Awareness Level** (`Tracker.Awareness`: 0-1 float)
2. **Awareness Stage** (`AwareStage`: None/Suspicious/Alert/Aware)
3. **Tracked Targets** (Dictionary<int, Tracker>)
4. **Evidence State** (Dictionary<string, bool>)
5. **Noise Intensity** (per-tag float values)
6. **Last Heard Position** (Vector3)

### Events to Network
```csharp
// Perception.cs events to broadcast across network
EventChangeAwarenessLevel(GameObject target, float level)
EventChangeAwarenessStage(GameObject target, AwareStage stage)
EventTrack(GameObject target)
EventUntrack(GameObject target)
EventNoticeEvidence(GameObject evidence)

// SensorHear events
EventHearNoise(string noiseTag)
EventReceiveNoise(string tag, float intensity)
```

## Proposed Architecture

### 1. NetworkPerception Component
```csharp
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Perception))]
public class NetworkPerception : NetworkBehaviour
{
    // Synchronized state
    private NetworkVariable<float>[] m_AwarenessLevels; // Per tracked target
    private NetworkList<TrackedTargetData> m_TrackedTargets;
    private NetworkList<EvidenceData> m_EvidenceState;

    // Server-authoritative perception updates
    [ServerRpc]
    public void AddAwarenessServerRpc(ulong targetNetworkId, float amount);

    // Client notifications
    [ClientRpc]
    private void NotifyAwarenessChangeClientRpc(ulong targetId, float level, AwareStage stage);
}
```

### 2. NetworkPerceptionEvents (Global Event Manager)
```csharp
public class NetworkPerceptionEvents : MonoBehaviour
{
    // Global events for visual scripting
    public static event Action<Perception, GameObject, float> EventAwarenessChanged;
    public static event Action<Perception, GameObject, AwareStage> EventAwarenessStageChanged;
    public static event Action<Perception, StimulusNoise> EventNoiseHeard;
    public static event Action<Perception, Evidence> EventEvidenceNoticed;

    // Broadcast methods
    public static void NotifyAwarenessChanged(Perception perception, GameObject target, float level);
}
```

### 3. NetworkPerceptionRegistry
```csharp
public static class NetworkPerceptionRegistry
{
    public static NetworkPerception GetByNetworkObjectId(ulong id);
    public static IReadOnlyList<NetworkPerception> AllPerceptions { get; }
    public static NetworkPerception GetPerceptionFor(NetworkCharacter character);
}
```

### 4. Network Visual Scripting Components

#### New Network-Aware Triggers
| Trigger | Category | Description |
|---------|----------|-------------|
| On Network Awareness Changed | Network/Perception | Awareness level changed (any client) |
| On Network Awareness Stage | Network/Perception | Stage threshold crossed |
| On Network Noise Heard | Network/Perception | Server-validated noise detection |
| On Network Evidence Noticed | Network/Perception | Evidence state change |

#### New Network Instructions
| Instruction | Category | Server-Only |
|-------------|----------|-------------|
| Set Network Awareness | Network/Perception | Yes |
| Add Network Awareness | Network/Perception | Yes |
| Relay Network Awareness | Network/Perception | Yes |
| Emit Network Noise | Network/Perception | No (local, server validates) |

#### New Network Conditions
| Condition | Description |
|-----------|-------------|
| Is Tracking Target (Network) | Server-authoritative tracking check |
| Awareness Stage Is (Network) | Check synced awareness stage |

### 5. Sync Strategy

**Server-Authoritative Model**:
- Server owns all NPC Perception components
- Player perceptions owned by respective clients (for local predictions)
- Awareness changes validated and applied by server
- RPCs broadcast significant events to all clients

**Delta Compression**:
```csharp
// Only sync meaningful changes
private void SyncAwareness(float newLevel)
{
    if (Mathf.Abs(m_LastSyncedLevel - newLevel) > SYNC_THRESHOLD)
    {
        m_AwarenessLevel.Value = newLevel;
        m_LastSyncedLevel = newLevel;
    }
}
```

## Implementation Phases

### Phase 1: Core Network Components
- [ ] NetworkPerception component with NetworkVariable state
- [ ] NetworkPerceptionSync helper (like NetworkCharacterSync)
- [ ] NetworkPerceptionRegistry for lookups
- [ ] NetworkPerceptionEvents for global broadcasting

### Phase 2: Visual Scripting Integration
- [ ] Network-aware awareness triggers
- [ ] Network-aware perception instructions
- [ ] Network-aware perception conditions
- [ ] Network-aware perception properties (getters)

### Phase 3: Sensory Synchronization
- [ ] NetworkStimulusNoise (server-validated noise emission)
- [ ] NetworkStimulusScent (server-validated scent emission)
- [ ] Stimulus propagation RPCs

### Phase 4: Evidence & Advanced Features
- [ ] NetworkEvidence component
- [ ] Evidence tampering sync
- [ ] Relay awareness/evidence over network

## File Structure

```
Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/
├── Runtime/
│   ├── Components/
│   │   ├── Perception/
│   │   │   ├── NetworkPerception.cs
│   │   │   ├── NetworkPerceptionSync.cs
│   │   │   └── NetworkPerceptionRegistry.cs
│   │   └── NetworkPerceptionEvents.cs
│   ├── VisualScripting/
│   │   ├── Events/
│   │   │   ├── Perception/
│   │   │   │   ├── EventNetworkOnAwarenessChange.cs
│   │   │   │   ├── EventNetworkOnAwarenessStage.cs
│   │   │   │   ├── EventNetworkOnNoiseHeard.cs
│   │   │   │   └── EventNetworkOnEvidenceNoticed.cs
│   │   ├── Instructions/
│   │   │   └── Perception/
│   │   │       ├── InstructionNetworkSetAwareness.cs
│   │   │       ├── InstructionNetworkAddAwareness.cs
│   │   │       └── InstructionNetworkEmitNoise.cs
│   │   ├── Conditions/
│   │   │   └── Perception/
│   │   │       ├── ConditionNetworkIsTracking.cs
│   │   │       └── ConditionNetworkAwarenessStage.cs
│   │   └── Properties/
│   │       └── Perception/
│   │           ├── GetNetworkAwarenessLevel.cs
│   │           └── GetNetworkTrackedTarget.cs
```

## Risks and Mitigations

| Risk | Mitigation |
|------|------------|
| High sync bandwidth for many trackers | Delta compression, sync thresholds, interest management |
| Perception updates conflicting with local predictions | Server-authoritative with client hints for responsiveness |
| Initialization timing issues | Follow NetworkCharacter deferred init pattern |
| Breaking existing local-only perception | Additive components, no invasive changes to core Perception |

## Success Criteria

1. NPC awareness state synchronized across all clients
2. Server-authoritative perception decisions
3. Visual scripting triggers work identically to local perception events
4. No breaking changes to existing Perception module
5. Performance: <1ms overhead per perception update

## Dependencies

- Existing Netcode integration (GameCreator.Netcode.Runtime)
- Perception module (GameCreator.Runtime.Perception)
- Unity Netcode for GameObjects 2.7.0+

## Estimated Scope

- **Components**: 5 new C# classes
- **Visual Scripting**: 10-15 new Event/Instruction/Condition classes
- **Documentation**: Update NETWORK_VISUAL_SCRIPTING.md
- **Testing**: Multiplayer perception test scenarios
