# Design: Multiplayer Component Architecture

## Context

GameCreator characters require a specific component stack to work correctly in Unity Netcode multiplayer. The architecture was designed to:
1. Avoid conflicts between NetworkTransform and CharacterController
2. Keep Character component enabled (required for animation)
3. Provide server authority for cheat-sensitive data
4. Support modular sync (stats, inventory, perception)

## Goals / Non-Goals

**Goals:**
- Establish consistent multiplayer component stack for all networked characters
- Document required vs optional components
- Provide clear setup instructions for scene GameObjects
- Ensure animation sync works correctly

**Non-Goals:**
- Changing the fundamental architecture (already established)
- Adding new sync modules (use existing ones)
- Modifying GameCreator core (invasive integration already done)

## Decisions

### Decision 1: NetworkPlayerController as Unified Coordinator
**What**: Use NetworkPlayerController as the single entry point for networked character setup
**Why**:
- Consolidates module initialization logic
- Auto-detects optional modules (Traits, Inventory, Perception)
- Provides consistent NetworkVariables (PlayerName, IsReady, TeamId)
- Ensures Character stays enabled

**Alternatives considered**:
- Individual components without coordinator → scattered logic, easy to misconfigure
- NetworkMultiplayerCharacter alone → lacks module coordination

### Decision 2: No NetworkTransform on Characters
**What**: Explicitly prevent NetworkTransform on character prefabs
**Why**:
- NetworkTransform conflicts with CharacterController physics
- Causes position "fighting" and jitter
- NetworkCharacterAdapter provides state-based sync instead

**Migration**: Remove any existing NetworkTransform, use NetworkCharacterAdapter

### Decision 3: Adaptive Send Rate
**What**: NetworkCharacterAdapter uses adaptive send rate based on movement
**Why**:
- Idle players: 3Hz (75% bandwidth reduction)
- Walking players: 6Hz (50% bandwidth reduction)
- Running players: 12Hz (full rate)

**Trade-off**: Slight latency increase for slow-moving players vs significant bandwidth savings

### Decision 4: Server Authority for Sensitive Data
**What**: Stats, inventory, team assignment use server authority
**Why**: Prevents client-side cheating

**Owner authority used for**: Movement, rotation, animations (client prediction)

## Component Stack Diagram

```
Player Network Prefab
├── Character (GameCreator) ─────────────── MUST BE ENABLED
│   └── UnitAnimimKinematic ──────────────── Animation driver
├── CharacterController (Unity) ──────────── Physics
├── Animator ─────────────────────────────── Animation state
├── NetworkObject (Netcode) ──────────────── Network identity
├── NetworkCharacterAdapter ──────────────── Position/rotation sync
│   ├── NetworkVariable<Vector3> position
│   ├── NetworkVariable<Quaternion> rotation
│   └── NetworkVariable<Vector3> velocity
├── NetworkGameCreatorAnimator ───────────── Animation sync
│   └── Syncs animator parameters at 60Hz
├── NetworkPlayerController ──────────────── Module coordinator
│   ├── NetworkVariable<string> playerName
│   ├── NetworkVariable<bool> isReady
│   └── NetworkVariable<int> teamId
└── [Optional Modules]
    ├── NetworkTraitsAdapter ─────────────── Stats sync (if Traits present)
    ├── NetworkInventorySync ─────────────── Inventory sync (if Bag present)
    └── NetworkPerceptionSync ────────────── Perception sync (if Perception present)
```

## Data Flow

### Owner (Local Player)
```
Input → Character.Motion → CharacterController.Move() → Transform
                                    ↓
                        NetworkCharacterAdapter.SyncTransformToNetwork()
                                    ↓
                        NetworkVariable.Value = position/rotation
                                    ↓
                              Network Broadcast
```

### Remote (Other Players)
```
Network Receive → NetworkVariable.OnValueChanged
                           ↓
            NetworkCharacterAdapter.InterpolateRemoteTransform()
                           ↓
                    Transform.position/rotation
                           ↓
            NetworkGameCreatorAnimator syncs parameters
                           ↓
                    Animator plays animations
```

## Risks / Trade-offs

### Risk 1: Component Missing on Prefab
**Mitigation**: NetworkPlayerController auto-adds required components in InitializeModules()

### Risk 2: Character Component Disabled
**Mitigation**: NetworkPlayerController.Update() forces Character.enabled = true

### Risk 3: Legacy V2 Component Present
**Mitigation**: ValidateSetup() logs error if NetworkGameCreatorCharacterV2 detected

### Risk 4: NetworkTransform Re-Added
**Mitigation**:
- Documentation clearly states NO NetworkTransform
- Test validates prefab doesn't have NetworkTransform
- Compilation errors prevent accidental usage (component removed)

## Open Questions

1. **Q**: Should we add editor validation to prevent NetworkTransform on character prefabs?
   **A**: Consider adding OnValidate() check in NetworkCharacterAdapter

2. **Q**: Should NetworkPlayerController be auto-added by editor script?
   **A**: Could add SetupPlayerNetworkPrefab editor tool for one-click setup

## Implementation Order

1. **Phase 1**: Scene infrastructure (NetworkManager, spawn points)
2. **Phase 2**: Player prefab component stack
3. **Phase 3**: Scene interactables
4. **Phase 4**: NPCs
5. **Phase 5**: Pickups
6. **Phase 6**: Validation and testing
