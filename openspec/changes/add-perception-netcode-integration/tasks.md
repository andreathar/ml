# Tasks: Perception Netcode Integration

## Phase 1: Core Network Components (12 tasks)

### 1.1 NetworkPerception Component
- [ ] Create `NetworkPerception.cs` extending NetworkBehaviour
- [ ] Add NetworkVariable for awareness state synchronization
- [ ] Implement NetworkList for tracked targets data
- [ ] Add deferred initialization pattern (following NetworkCharacter)
- [ ] Implement OnNetworkSpawn/OnNetworkDespawn lifecycle

### 1.2 NetworkPerceptionSync Helper
- [ ] Create `NetworkPerceptionSync.cs` for ownership callbacks
- [ ] Wire OnOwnershipChanged to NetworkPerception
- [ ] Handle server vs client authority modes

### 1.3 NetworkPerceptionRegistry
- [ ] Create `NetworkPerceptionRegistry.cs` static class
- [ ] Implement Register/Unregister for NetworkPerception
- [ ] Add lookup by NetworkObjectId
- [ ] Add lookup by associated NetworkCharacter
- [ ] Implement AllPerceptions readonly list

### 1.4 NetworkPerceptionEvents
- [ ] Create `NetworkPerceptionEvents.cs` singleton
- [ ] Add static events for awareness changes
- [ ] Add static events for stage changes
- [ ] Add static events for noise/scent detection
- [ ] Implement Notify* broadcast methods

## Phase 2: Server-Authoritative Awareness (8 tasks)

### 2.1 Awareness Synchronization
- [ ] Define TrackedTargetData struct for NetworkList
- [ ] Implement awareness delta compression
- [ ] Add sync threshold configuration
- [ ] Create SetAwarenessServerRpc
- [ ] Create AddAwarenessServerRpc
- [ ] Create NotifyAwarenessChangeClientRpc

### 2.2 Tracker Network Integration
- [ ] Hook into Cortex.EventAwarenessChangeLevel
- [ ] Hook into Cortex.EventAwarenessChangeStage
- [ ] Sync Tracker creation on Track()
- [ ] Sync Tracker removal on Untrack()

## Phase 3: Visual Scripting Events (8 tasks)

### 3.1 Network Awareness Triggers
- [ ] Create `EventNetworkOnAwarenessChange.cs`
- [ ] Create `EventNetworkOnAwarenessStage.cs`
- [ ] Add network filtering (server-only, owner-only options)
- [ ] Set Target in Args for downstream instructions

### 3.2 Network Sensory Triggers
- [ ] Create `EventNetworkOnNoiseHeard.cs`
- [ ] Create `EventNetworkOnEvidenceNoticed.cs`
- [ ] Create `EventNetworkOnTargetTracked.cs`
- [ ] Create `EventNetworkOnTargetUntracked.cs`

## Phase 4: Visual Scripting Instructions (6 tasks)

### 4.1 Awareness Instructions
- [ ] Create `InstructionNetworkSetAwareness.cs` (server-only)
- [ ] Create `InstructionNetworkAddAwareness.cs` (server-only)
- [ ] Create `InstructionNetworkRelayAwareness.cs` (server-only)

### 4.2 Sensory Instructions
- [ ] Create `InstructionNetworkEmitNoise.cs`
- [ ] Create `InstructionNetworkEmitScent.cs`
- [ ] Create `InstructionNetworkTrackTarget.cs`

## Phase 5: Visual Scripting Conditions (4 tasks)

- [ ] Create `ConditionNetworkIsTracking.cs`
- [ ] Create `ConditionNetworkAwarenessStage.cs`
- [ ] Create `ConditionNetworkAwarenessLevel.cs`
- [ ] Create `ConditionNetworkCanSense.cs`

## Phase 6: Visual Scripting Properties (4 tasks)

- [ ] Create `GetNetworkAwarenessLevel.cs`
- [ ] Create `GetNetworkAwarenessStage.cs`
- [ ] Create `GetNetworkTrackedTarget.cs`
- [ ] Create `GetNetworkLastHeardPosition.cs`

## Phase 7: Sensory Stimulus Network Layer (6 tasks)

### 7.1 Noise Synchronization
- [ ] Create `NetworkStimulusNoise.cs`
- [ ] Implement server-validated noise emission
- [ ] Add EmitNoiseServerRpc
- [ ] Add PropagateNoiseClientRpc

### 7.2 Scent Synchronization
- [ ] Create `NetworkStimulusScent.cs`
- [ ] Implement server-validated scent emission

## Phase 8: Evidence Network Layer (4 tasks)

- [ ] Create `NetworkEvidence.cs` extending Evidence
- [ ] Sync IsTampered state across network
- [ ] Create `InstructionNetworkTamperEvidence.cs`
- [ ] Create `InstructionNetworkInvestigateEvidence.cs`

## Phase 9: Documentation & Testing (4 tasks)

- [ ] Update NETWORK_VISUAL_SCRIPTING.md with perception components
- [ ] Add perception patterns section
- [ ] Add perception troubleshooting section
- [ ] Create multiplayer perception test scene

## Summary

| Phase | Tasks | Priority |
|-------|-------|----------|
| Phase 1: Core Components | 12 | Critical |
| Phase 2: Awareness Sync | 8 | Critical |
| Phase 3: VS Events | 8 | High |
| Phase 4: VS Instructions | 6 | High |
| Phase 5: VS Conditions | 4 | Medium |
| Phase 6: VS Properties | 4 | Medium |
| Phase 7: Stimulus Sync | 6 | Medium |
| Phase 8: Evidence Sync | 4 | Low |
| Phase 9: Documentation | 4 | High |

**Total: 56 tasks**

## Dependencies Graph

```
Phase 1 (Core Components)
    ↓
Phase 2 (Awareness Sync)
    ↓
┌───┴───┐
↓       ↓
Phase 3  Phase 7 (Stimulus)
(Events)     ↓
    ↓     Phase 8 (Evidence)
Phase 4
(Instructions)
    ↓
┌───┴───┐
↓       ↓
Phase 5  Phase 6
(Conditions) (Properties)
    ↓
Phase 9 (Documentation)
```
