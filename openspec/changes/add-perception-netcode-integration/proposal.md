# Perception Netcode Integration - Complete Networking Architecture

## Summary

Make the GameCreator Perception module **totally networked** for multiplayer games. This includes all five senses (See, Hear, Smell, Feel), awareness tracking, evidence system, environmental modifiers (Luminance, Camouflage, Din), and UI synchronization.

## Current Implementation Status

### Already Implemented (Phase 1-2 Complete)
| Component | Status | Location |
|-----------|--------|----------|
| `NetworkPerception` | Done | `Runtime/Components/Perception/` |
| `NetworkPerceptionSync` | Done | `Runtime/Components/Perception/` |
| `NetworkPerceptionRegistry` | Done | `Runtime/Components/Perception/` |
| `NetworkPerceptionEvents` | Done | `Runtime/Components/Perception/` |
| `NetworkNoiseEmitter` | Done | `Runtime/Components/Perception/` |
| Awareness Events | Done | `Runtime/VisualScripting/Events/Perception/` |
| Awareness Instructions | Done | `Runtime/VisualScripting/Instructions/Perception/` |
| Awareness Conditions | Done | `Runtime/VisualScripting/Conditions/Perception/` |

### Missing for "Totally Networked"
| Feature | Priority | Complexity |
|---------|----------|------------|
| **NetworkScentEmitter** | High | Medium |
| **NetworkEnvironmentSync** (Luminance, Din, Dissipation) | High | Medium |
| **NetworkEvidence** | High | Low |
| **NetworkCamouflage** | Medium | Low |
| **Network UI Components** | Medium | Medium |
| **Feel Sensor Sync** | Low | Low |

## Motivation

The existing implementation handles **awareness and hearing** but a multiplayer game needs **all perception features** synchronized:

1. **Scent Tracking**: NPCs tracking players by smell must work consistently across all clients
2. **Environmental State**: Ambient luminance, global din, scent dissipation must be server-authoritative
3. **Evidence System**: Tampered evidence state must sync for stealth/investigation gameplay
4. **Camouflage**: Player stealth modifiers must sync for fair NPC detection
5. **UI Consistency**: Awareness indicators must show server-authoritative values

## Architecture Overview

### Network Topology
```
┌─────────────────────────────────────────────────────────────────────────┐
│                           SERVER (Authority)                             │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────┐  │
│  │ NetworkPerception│  │NetworkScentEmit │  │NetworkEnvironmentSync   │  │
│  │ (per NPC)        │  │ (singleton)     │  │ (singleton)             │  │
│  │ - Awareness      │  │ - Scent events  │  │ - AmbientLuminance      │  │
│  │ - TrackedTargets │  │ - Validation    │  │ - GlobalDin             │  │
│  │ - Evidence       │  │                 │  │ - ScentDissipation      │  │
│  └────────┬────────┘  └────────┬────────┘  └───────────┬─────────────┘  │
│           │                    │                       │                 │
│           └────────────────────┼───────────────────────┘                 │
│                                │                                         │
│                       [ClientRpc Broadcast]                              │
└────────────────────────────────┼─────────────────────────────────────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  CLIENT 1       │    │  CLIENT 2       │    │  CLIENT 3       │
│  - Local UI     │    │  - Local UI     │    │  - Local UI     │
│  - Predictions  │    │  - Predictions  │    │  - Predictions  │
│  - Visual FX    │    │  - Visual FX    │    │  - Visual FX    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## New Components Required

### 1. NetworkScentEmitter (High Priority)
```csharp
/// <summary>
/// Singleton that handles network-synchronized scent emission.
/// Mirrors NetworkNoiseEmitter pattern for smell stimuli.
/// </summary>
[AddComponentMenu("Game Creator/Network/Network Scent Emitter")]
[RequireComponent(typeof(NetworkObject))]
public class NetworkScentEmitter : NetworkBehaviour
{
    public void EmitScent(Vector3 position, float radius, string tag, float intensity);
    
    [Rpc(SendTo.Server)]
    private void EmitScentServerRpc(Vector3 position, float radius, 
        FixedString32Bytes tag, float intensity, RpcParams rpcParams = default);
    
    [Rpc(SendTo.ClientsAndHost)]
    private void BroadcastScentClientRpc(Vector3 position, float radius,
        FixedString32Bytes tag, float intensity, ulong sourceClientId);
}
```

### 2. NetworkEnvironmentSync (High Priority)
```csharp
/// <summary>
/// Synchronizes global environmental perception state across network.
/// Single authority source for ambient conditions.
/// </summary>
[AddComponentMenu("Game Creator/Network/Network Environment Sync")]
[RequireComponent(typeof(NetworkObject))]
public class NetworkEnvironmentSync : NetworkBehaviour
{
    // Synchronized environmental state
    private NetworkVariable<float> m_AmbientLuminance = new(1f);
    private NetworkVariable<float> m_GlobalDin = new(0f);
    private NetworkVariable<float> m_ScentDissipation = new(1f);
    
    // Public accessors for local systems
    public static float AmbientLuminance => Instance?.m_AmbientLuminance.Value ?? 1f;
    public static float GlobalDin => Instance?.m_GlobalDin.Value ?? 0f;
    public static float ScentDissipation => Instance?.m_ScentDissipation.Value ?? 1f;
    
    // Server-only modification
    [Rpc(SendTo.Server)]
    public void SetAmbientLuminanceRpc(float value);
    [Rpc(SendTo.Server)]
    public void SetGlobalDinRpc(float value);
    [Rpc(SendTo.Server)]
    public void SetScentDissipationRpc(float value);
}
```

### 3. NetworkEvidence (High Priority)
```csharp
/// <summary>
/// Network wrapper for Evidence component.
/// Synchronizes tampered state across all clients.
/// </summary>
[AddComponentMenu("Game Creator/Network/Network Evidence")]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Evidence))]
public class NetworkEvidence : NetworkBehaviour
{
    private NetworkVariable<bool> m_IsTampered = new();
    private NetworkVariable<FixedString64Bytes> m_EvidenceTag = new();
    
    [NonSerialized] private Evidence m_Evidence;
    
    public bool IsTampered => m_IsTampered.Value;
    
    [Rpc(SendTo.Server)]
    public void TamperEvidenceRpc(RpcParams rpcParams = default);
    
    [Rpc(SendTo.Server)]
    public void RestoreEvidenceRpc(RpcParams rpcParams = default);
    
    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyEvidenceTamperedRpc(bool isTampered, ulong actorClientId);
}
```

### 4. NetworkCamouflage (Medium Priority)
```csharp
/// <summary>
/// Synchronizes player/NPC camouflage modifier across network.
/// Ensures fair detection calculations on all clients.
/// </summary>
[AddComponentMenu("Game Creator/Network/Network Camouflage")]
[RequireComponent(typeof(NetworkObject))]
public class NetworkCamouflage : NetworkBehaviour
{
    private NetworkVariable<float> m_CamouflageValue = new(0f);
    
    public float Value => m_CamouflageValue.Value;
    
    [Rpc(SendTo.Server)]
    public void SetCamouflageRpc(float value);
}
```

## New Visual Scripting Components

### Instructions (Missing)
| Instruction | Category | Description |
|-------------|----------|-------------|
| `InstructionNetworkEmitScent` | Network/Perception | Emit networked scent stimulus |
| `InstructionNetworkSetAmbientLuminance` | Network/Perception | Set global light level |
| `InstructionNetworkSetGlobalDin` | Network/Perception | Set global noise floor |
| `InstructionNetworkSetScentDissipation` | Network/Perception | Set scent decay rate |
| `InstructionNetworkTamperEvidence` | Network/Perception | Mark evidence as tampered |
| `InstructionNetworkRestoreEvidence` | Network/Perception | Reset evidence state |
| `InstructionNetworkRelayAwareness` | Network/Perception | Share awareness between NPCs |
| `InstructionNetworkRelayEvidence` | Network/Perception | Share evidence knowledge |
| `InstructionNetworkSetCamouflage` | Network/Perception | Set stealth modifier |

### Events (Missing)
| Event | Category | Description |
|-------|----------|-------------|
| `EventNetworkOnScentSmelled` | Network/Perception | NPC smelled a scent |
| `EventNetworkOnEvidenceTampered` | Network/Perception | Evidence state changed |
| `EventNetworkOnEnvironmentChanged` | Network/Perception | Global env value changed |
| `EventNetworkOnFeel` | Network/Perception | Proximity detection triggered |

### Conditions (Missing)
| Condition | Description |
|-----------|-------------|
| `ConditionNetworkCanSmellScent` | Check if scent is detectable |
| `ConditionNetworkEvidenceIsTampered` | Check evidence state |
| `ConditionNetworkCanSee` | Check visibility with network luminance |

### Properties (Missing)
| Property | Description |
|----------|-------------|
| `GetNetworkAmbientLuminance` | Get synced luminance value |
| `GetNetworkGlobalDin` | Get synced din value |
| `GetNetworkCamouflageValue` | Get synced camouflage |
| `GetNetworkEvidenceState` | Get evidence tampered state |

## Network UI Components

### NetworkIndicatorAwarenessUI
Replaces local `IndicatorAwarenessUI` to display server-authoritative awareness values:
```csharp
public class NetworkIndicatorAwarenessUI : MonoBehaviour
{
    // Uses NetworkPerceptionRegistry instead of local Perception
    private void Update()
    {
        var networkPerception = NetworkPerceptionRegistry.GetByGameObject(m_Target);
        if (networkPerception == null) return;
        
        // Display synced awareness from NetworkPerception
        float awareness = networkPerception.GetAwareness(m_TrackedTarget);
        UpdateUI(awareness);
    }
}
```

### NetworkLuminanceUI / NetworkNoiseUI / NetworkSmellUI
Similar pattern - read from `NetworkEnvironmentSync` singleton instead of local managers.

## Implementation Phases

### Phase 3: Scent Networking (NEW)
- [ ] Create `NetworkScentEmitter.cs` singleton
- [ ] Add scent validation and rate limiting
- [ ] Create `InstructionNetworkEmitScent.cs`
- [ ] Create `EventNetworkOnScentSmelled.cs`
- [ ] Create `ConditionNetworkCanSmellScent.cs`
- [ ] Hook into `SensorSmell.OnReceiveScent()`

### Phase 4: Environment Synchronization (NEW)
- [ ] Create `NetworkEnvironmentSync.cs` singleton
- [ ] Sync `LuminanceManager.AmbientLuminance`
- [ ] Sync `HearManager.GlobalDin`
- [ ] Sync `SmellManager.Dissipation`
- [ ] Create environment VS instructions
- [ ] Create `EventNetworkOnEnvironmentChanged.cs`

### Phase 5: Evidence Networking (NEW)
- [ ] Create `NetworkEvidence.cs` component
- [ ] Sync `IsTampered` state
- [ ] Create `InstructionNetworkTamperEvidence.cs`
- [ ] Create `InstructionNetworkRestoreEvidence.cs`
- [ ] Create `EventNetworkOnEvidenceTampered.cs`
- [ ] Update `NetworkPerception` to use synced evidence

### Phase 6: Camouflage & Modifiers (NEW)
- [ ] Create `NetworkCamouflage.cs` component
- [ ] Create `InstructionNetworkSetCamouflage.cs`
- [ ] Hook into `SensorSee` detection calculations
- [ ] Add visibility modifier sync

### Phase 7: Network UI Components (NEW)
- [ ] Create `NetworkIndicatorAwarenessUI.cs`
- [ ] Create `NetworkLuminanceUI.cs`
- [ ] Create `NetworkNoiseUI.cs`
- [ ] Create `NetworkSmellUI.cs`
- [ ] Create network-aware UI prefabs

### Phase 8: Feel Sensor & Polish (NEW)
- [ ] Hook `SensorFeel` into network events
- [ ] Create `EventNetworkOnFeel.cs`
- [ ] Performance optimization pass
- [ ] Bandwidth profiling and tuning

## File Structure (Updated)

```
Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/
├── Runtime/
│   ├── Components/
│   │   ├── Perception/
│   │   │   ├── NetworkPerception.cs         ✓ EXISTS
│   │   │   ├── NetworkPerceptionSync.cs     ✓ EXISTS
│   │   │   ├── NetworkPerceptionRegistry.cs ✓ EXISTS
│   │   │   ├── NetworkPerceptionEvents.cs   ✓ EXISTS
│   │   │   ├── NetworkNoiseEmitter.cs       ✓ EXISTS
│   │   │   ├── NetworkScentEmitter.cs       ← NEW
│   │   │   ├── NetworkEvidence.cs           ← NEW
│   │   │   └── NetworkCamouflage.cs         ← NEW
│   │   └── NetworkEnvironmentSync.cs        ← NEW
│   ├── UI/
│   │   └── Components/
│   │       ├── NetworkIndicatorAwarenessUI.cs  ← NEW
│   │       ├── NetworkLuminanceUI.cs           ← NEW
│   │       ├── NetworkNoiseUI.cs               ← NEW
│   │       └── NetworkSmellUI.cs               ← NEW
│   └── VisualScripting/
│       ├── Events/
│       │   └── Perception/
│       │       ├── EventNetworkOnAwarenessChange.cs    ✓ EXISTS
│       │       ├── EventNetworkOnAwarenessStage.cs     ✓ EXISTS
│       │       ├── EventNetworkOnNoiseHeard.cs         ✓ EXISTS
│       │       ├── EventNetworkOnEvidenceNoticed.cs    ✓ EXISTS
│       │       ├── EventNetworkOnTargetTracked.cs      ✓ EXISTS
│       │       ├── EventNetworkOnTargetUntracked.cs    ✓ EXISTS
│       │       ├── EventNetworkOnScentSmelled.cs       ← NEW
│       │       ├── EventNetworkOnEvidenceTampered.cs   ← NEW
│       │       ├── EventNetworkOnEnvironmentChanged.cs ← NEW
│       │       └── EventNetworkOnFeel.cs               ← NEW
│       ├── Instructions/
│       │   └── Perception/
│       │       ├── InstructionNetworkAddAwareness.cs   ✓ EXISTS
│       │       ├── InstructionNetworkSetAwareness.cs   ✓ EXISTS
│       │       ├── InstructionNetworkTrackTarget.cs    ✓ EXISTS
│       │       ├── InstructionNetworkUntrackTarget.cs  ✓ EXISTS
│       │       ├── InstructionNetworkEmitNoise.cs      ✓ EXISTS
│       │       ├── InstructionNetworkEmitScent.cs      ← NEW
│       │       ├── InstructionNetworkSetAmbientLuminance.cs  ← NEW
│       │       ├── InstructionNetworkSetGlobalDin.cs   ← NEW
│       │       ├── InstructionNetworkSetScentDissipation.cs  ← NEW
│       │       ├── InstructionNetworkTamperEvidence.cs ← NEW
│       │       ├── InstructionNetworkRestoreEvidence.cs ← NEW
│       │       ├── InstructionNetworkRelayAwareness.cs ← NEW
│       │       ├── InstructionNetworkRelayEvidence.cs  ← NEW
│       │       └── InstructionNetworkSetCamouflage.cs  ← NEW
│       └── Conditions/
│           └── Perception/
│               ├── ConditionNetworkAwarenessLevel.cs   ✓ EXISTS
│               ├── ConditionNetworkAwarenessStage.cs   ✓ EXISTS
│               ├── ConditionNetworkIsTracking.cs       ✓ EXISTS
│               ├── ConditionNetworkCanSmellScent.cs    ← NEW
│               ├── ConditionNetworkEvidenceIsTampered.cs ← NEW
│               └── ConditionNetworkCanSee.cs           ← NEW
```

## Risks and Mitigations

| Risk | Mitigation |
|------|------------|
| High bandwidth for scent trails | Spatial quantization, interest management |
| Luminance sync overhead (many lights) | Only sync ambient, not per-light |
| Evidence sync conflicts | Server-authoritative, last-write-wins |
| UI flickering from sync delays | Client-side smoothing, prediction |
| Initialization race conditions | Deferred init, spawn ordering |

## Success Criteria

1. **All 5 senses networked**: See, Hear, Smell, Feel awareness synced
2. **Environmental state synced**: Luminance, Din, Dissipation consistent
3. **Evidence system multiplayer-safe**: Tampered state authoritative
4. **Zero breaking changes**: Existing single-player perception works
5. **Visual Scripting parity**: Network triggers mirror local triggers
6. **Performance**: <2ms overhead per perception update
7. **Bandwidth**: <1KB/s per NPC perception

## Estimated Scope (Updated)

- **New Components**: 8 C# classes
- **New Visual Scripting**: 15 Event/Instruction/Condition classes
- **New UI Components**: 4 network-aware UI classes
- **Documentation**: Full perception networking guide
- **Testing**: Multiplayer perception test scenarios
- **Prefabs**: Network perception manager prefab
