# Spec Deltas: Perception Netcode Integration

This document defines the specification changes required for the Perception Netcode integration.

## New Spec: `perception-synchronization`

### Overview
Defines how GameCreator Perception module state is synchronized across the network.

### Scope
- NetworkPerception component behavior
- Awareness state synchronization
- Tracker data replication
- Evidence state sync
- Sensory stimulus network propagation

### Key Definitions

#### Server-Authoritative Awareness
```yaml
awareness_sync:
  authority: server
  update_frequency: on_change  # Not tick-based
  delta_threshold: 0.01        # Min change to trigger sync
  compression: delta           # Only sync changes
```

#### Tracked Target Data
```yaml
tracked_target:
  fields:
    - target_network_id: ulong    # NetworkObject.NetworkObjectId
    - awareness: float            # 0-1 normalized
    - stage: byte                 # AwareStage enum
    - last_increase_time: float   # For forget calculation
  replication: NetworkList<TrackedTargetData>
```

#### Evidence Data
```yaml
evidence:
  fields:
    - tag: FixedString32Bytes
    - is_tampered: bool
  replication: NetworkList<EvidenceData>
```

### Network Messages

#### ServerRpcs
| RPC | Parameters | Description |
|-----|------------|-------------|
| SetAwarenessServerRpc | targetNetworkId, awareness | Set awareness for target |
| AddAwarenessServerRpc | targetNetworkId, delta, max | Add to awareness |
| TrackTargetServerRpc | targetNetworkId | Start tracking target |
| UntrackTargetServerRpc | targetNetworkId | Stop tracking target |
| EmitNoiseServerRpc | NoiseEventData | Server-validated noise emission |
| TamperEvidenceServerRpc | evidenceNetworkId | Mark evidence as tampered |

#### ClientRpcs
| RPC | Parameters | Description |
|-----|------------|-------------|
| NotifyAwarenessChangeClientRpc | targetNetworkId, level, stage | Broadcast awareness change |
| NotifyNoiseHeardClientRpc | perceptionNetworkId, NoiseEventData | Broadcast noise detection |
| NotifyEvidenceNoticedClientRpc | perceptionNetworkId, evidenceNetworkId | Broadcast evidence discovery |

---

## Spec Updates: `visual-scripting`

### New Events (Network/Perception category)

| Event | Description | Args.Self | Args.Target |
|-------|-------------|-----------|-------------|
| On Network Awareness Change | Awareness level changed | Perception GO | Tracked target |
| On Network Awareness Stage | Stage threshold crossed | Perception GO | Tracked target |
| On Network Target Tracked | Started tracking target | Perception GO | New target |
| On Network Target Untracked | Stopped tracking target | Perception GO | Old target |
| On Network Noise Heard | Noise detected (server-validated) | Perception GO | Noise source |
| On Network Evidence Noticed | Evidence state changed | Perception GO | Evidence GO |

### New Instructions (Network/Perception category)

| Instruction | Server-Only | Description |
|-------------|-------------|-------------|
| Set Network Awareness | Yes | Set awareness level for target |
| Add Network Awareness | Yes | Add to awareness level |
| Subtract Network Awareness | Yes | Subtract from awareness |
| Track Network Target | Yes | Start tracking a target |
| Untrack Network Target | Yes | Stop tracking a target |
| Emit Network Noise | No* | Emit noise (server validates) |
| Relay Network Awareness | Yes | Copy awareness to another Perception |

*Client sends to server for validation

### New Conditions (Network/Perception category)

| Condition | Description |
|-----------|-------------|
| Is Network Tracking | Check if Perception tracks target |
| Network Awareness Stage Is | Compare awareness stage |
| Network Awareness Level | Compare awareness level |
| Can Network Sense | Check if can sense target type |

### New Properties (Network/Perception category)

| Property | Returns | Description |
|----------|---------|-------------|
| Get Network Awareness Level | Decimal | Get synced awareness for target |
| Get Network Awareness Stage | AwareStage | Get synced stage for target |
| Get Network Tracked Target | GameObject | Get highest awareness target |
| Get Network Last Heard Position | Vector3 | Position of last heard noise |
| Get Network Perception | NetworkPerception | Get component reference |

---

## Spec Updates: `network-synchronization`

### Add Perception Sync Section

```yaml
perception_sync:
  component: NetworkPerception
  requires:
    - NetworkObject
    - Perception
  authority: server  # Server owns NPC perception state
  sync_mode: reliable  # Awareness changes are important

  network_variables:
    tracked_targets:
      type: NetworkList<TrackedTargetData>
      write_permission: Server
      read_permission: Everyone

    evidences:
      type: NetworkList<EvidenceData>
      write_permission: Server
      read_permission: Everyone

  lifecycle:
    on_network_spawn:
      - Register with NetworkPerceptionRegistry
      - Hook local Perception events
      - If server: sync local state to NetworkLists
      - If client: apply NetworkList state to local

    on_network_despawn:
      - Unhook Perception events
      - Unregister from registry
```

---

## Spec Updates: `spawn-system`

### NPC Prefab Requirements Update

```yaml
npc_prefab:
  required_components:
    - NetworkObject
    - NetworkCharacter
    - NetworkTransform
    - NetworkAnimator
  optional_components:
    - NetworkPerception  # NEW: For AI awareness sync
    - NetworkCharacterAdapter  # Only for player-owned characters

  perception_setup:
    when: NPC has Perception component
    add: NetworkPerception component
    configuration:
      server_authoritative: true
      sync_on_spawn: true
```

---

## New Spec: `perception-events`

### Global Event Broadcasting

```yaml
network_perception_events:
  component: NetworkPerceptionEvents
  singleton: true
  placement: NetworkManagers GameObject

  events:
    awareness:
      - EventAwarenessChanged(Perception, GameObject, float)
      - EventAwarenessStageChanged(Perception, GameObject, AwareStage)
      - EventTargetTracked(Perception, GameObject)
      - EventTargetUntracked(Perception, GameObject)

    sensory:
      - EventNoiseHeard(Perception, StimulusNoise)
      - EventEvidenceNoticed(Perception, Evidence)

  usage:
    visual_scripting_triggers:
      subscribe_on: OnEnable
      unsubscribe_on: OnDisable
      pattern: Same as NetworkSessionEvents
```

---

## New Spec: `perception-registry`

### NetworkPerceptionRegistry

```yaml
network_perception_registry:
  type: static class
  purpose: Efficient lookup of NetworkPerception components

  storage:
    by_network_id: Dictionary<ulong, NetworkPerception>
    by_instance_id: Dictionary<int, NetworkPerception>
    all_perceptions: List<NetworkPerception>

  methods:
    register:
      when: OnNetworkSpawn
      indexes: NetworkObjectId, InstanceId

    unregister:
      when: OnNetworkDespawn
      removes: All index entries

    lookups:
      - GetByNetworkId(ulong) → NetworkPerception
      - GetByGameObject(GameObject) → NetworkPerception
      - GetForCharacter(NetworkCharacter) → NetworkPerception
      - GetGameObjectByNetworkId(ulong) → GameObject
```

---

## Assembly Definition Updates

### GameCreator.Netcode.Runtime.asmdef

Add reference:
```json
{
  "references": [
    // ... existing references
    "GameCreator.Runtime.Perception"
  ]
}
```

---

## Scene Setup Updates

### NetworkManagers GameObject

```yaml
network_managers:
  children:
    - NetworkManagersBootstrap
    - NetworkInitializationManager
    - NetworkSessionEvents
    - NetworkPerceptionEvents  # NEW
```

### NPC Prefab Updates

```yaml
npc_with_perception:
  components:
    - NetworkObject
    - NetworkCharacter
    - NetworkTransform
    - NetworkAnimator
    - Perception (existing)
    - NetworkPerception  # NEW: Add to existing NPCs with Perception
```

---

## Migration Notes

### Existing Projects

1. **No breaking changes**: NetworkPerception is additive
2. **Add NetworkPerceptionEvents** to NetworkManagers if using perception
3. **Add NetworkPerception** to NPC prefabs that have Perception component
4. **Update triggers**: Replace local perception triggers with network versions for multiplayer consistency

### Backwards Compatibility

- Local-only Perception continues to work unchanged
- NetworkPerception is optional - only needed for multiplayer sync
- Existing visual scripting continues to work (but won't sync)
