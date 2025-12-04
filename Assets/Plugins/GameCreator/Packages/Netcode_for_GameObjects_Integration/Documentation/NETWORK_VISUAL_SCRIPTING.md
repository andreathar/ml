# GameCreator Netcode Visual Scripting Reference

## Overview

This document describes all network-related visual scripting components for GameCreator + Unity Netcode integration.

---

## TRIGGERS

### Session Events (Can be placed ANYWHERE in scene)

| Trigger | Category | Description | Args.Target |
|---------|----------|-------------|-------------|
| **On Host Started** | Network/Session | Fires when Host mode starts (server + client) | - |
| **On Client Started** | Network/Session | Fires when Client mode starts (connecting to host) | - |
| **On Server Started** | Network/Session | Fires when dedicated Server mode starts | - |
| **On Session Ended** | Network/Session | Fires when network session ends (disconnect) | - |

### Character Spawn Events (Can be placed ANYWHERE in scene)

| Trigger | Category | Description | Args.Target |
|---------|----------|-------------|-------------|
| **On Local Player Spawned** | Network/Characters | Fires when YOUR player spawns | Spawned player |
| **On Remote Player Spawned** | Network/Characters | Fires when OTHER players spawn | Spawned player |
| **On Any Player Spawned** | Network/Characters | Fires when ANY player spawns | Spawned player |
| **On NPC Spawned** | Network/Characters | Fires when any NPC spawns | Spawned NPC |
| **On Character Despawned** | Network/Characters | Fires when any character despawns | Despawned character |

### Object-Specific Events (MUST be on the prefab with NetworkCharacterAdapter)

| Trigger | Category | Description | Args.Target |
|---------|----------|-------------|-------------|
| **On Network Spawn** | Network/Object | Fires when THIS object spawns on network | This object |
| **On Network Despawn** | Network/Object | Fires when THIS object despawns | This object |
| **On Ownership Changed** | Network/Object | Fires when THIS object's ownership changes | This object |

### Game State Events (Can be placed ANYWHERE)

| Trigger | Category | Description |
|---------|----------|-------------|
| **On Game State Changed** | Network/Game | Fires when game state changes (Lobby, Playing, etc.) |
| **On Countdown Tick** | Network/Game | Fires each second during countdown |
| **On Countdown Complete** | Network/Game | Fires when countdown reaches zero |
| **On All Players Ready** | Network/Game | Fires when all connected players are ready |

### Connection Events (Can be placed ANYWHERE)

| Trigger | Category | Description |
|---------|----------|-------------|
| **On Client Connected** | Network/Connection | Fires when a client connects (server-side) |
| **On Client Disconnected** | Network/Connection | Fires when a client disconnects (server-side) |
| **On RPC Received** | Network/RPC | Fires when custom RPC message is received |
| **On Scene Loaded** | Network/Scene | Fires when network scene load completes |

### Perception Events (Can be placed ANYWHERE in scene)

| Trigger | Category | Description | Args.Self | Args.Target |
|---------|----------|-------------|-----------|-------------|
| **On Network Awareness Change** | Network/Perception | Awareness level changed | Perception GO | Tracked target |
| **On Network Awareness Stage** | Network/Perception | Stage crossed (Suspicious/Alert/Aware) | Perception GO | Tracked target |
| **On Network Target Tracked** | Network/Perception | Started tracking a target | Perception GO | New target |
| **On Network Target Untracked** | Network/Perception | Stopped tracking a target | Perception GO | Old target |
| **On Network Noise Heard** | Network/Perception | Noise detected (server-validated) | Perception GO | - |
| **On Network Evidence Noticed** | Network/Perception | Evidence tampering detected | Perception GO | Evidence GO |

---

## INSTRUCTIONS

### Connection Management

| Instruction | Category | Server-Only | Description |
|-------------|----------|-------------|-------------|
| **Start Host** | Network/Connection | No | Start as Host (server + client) |
| **Start Client** | Network/Connection | No | Start as Client (connect to host) |
| **Disconnect** | Network/Connection | No | Disconnect and shutdown network |

### Spawning (Server-Only)

| Instruction | Category | Description |
|-------------|----------|-------------|
| **Spawn Network Player** | Network/Spawn | Spawn player with client ownership |
| **Spawn Network NPC** | Network/Spawn | Spawn server-controlled NPC |
| **Despawn Network Object** | Network/Spawn | Remove object from network |

### Game State (Server-Only)

| Instruction | Category | Description |
|-------------|----------|-------------|
| **Set Game State** | Network/Game | Change game state (Lobby, Playing, etc.) |
| **Start Countdown** | Network/Game | Start synchronized countdown |
| **Set Ready** | Network/Game | Mark local player as ready/not ready |

### Communication

| Instruction | Category | Description |
|-------------|----------|-------------|
| **Send RPC** | Network/RPC | Send custom message to clients/server |
| **Change Ownership** | Network/Object | Transfer object ownership to another client |
| **Load Network Scene** | Network/Scene | Load scene for all clients (server-only) |

### Perception (Server-Authoritative)

| Instruction | Category | Server-Only | Description |
|-------------|----------|-------------|-------------|
| **Set Network Awareness** | Network/Perception | Yes | Set awareness level for a target |
| **Add Network Awareness** | Network/Perception | Yes | Add/subtract awareness level |
| **Track Network Target** | Network/Perception | Yes | Start tracking a target |
| **Untrack Network Target** | Network/Perception | Yes | Stop tracking a target |

---

## CONDITIONS

| Condition | Description |
|-----------|-------------|
| **Is Server** | True if running as server (Host or dedicated) |
| **Is Client** | True if running as client |
| **Is Host** | True if running as Host specifically |
| **Is Connected** | True if network is active |
| **Is Owner** | True if local client owns this NetworkObject |
| **Is Local Player** | True if this is the local player's character |
| **Is NPC** | True if this character is an NPC |
| **Is Spawned** | True if this NetworkObject is spawned |
| **Game State Is** | Check current game state |
| **All Players Ready** | True if all players marked ready |

### Perception Conditions

| Condition | Description |
|-----------|-------------|
| **Is Network Tracking** | True if Perception is tracking the target |
| **Network Awareness Stage Is** | Check if awareness stage matches (Suspicious/Alert/Aware) |
| **Network Awareness Level** | Compare awareness level (< <= == >= >) |

---

## PROPERTIES (Getters)

### GameObject Getters

| Property | Returns |
|----------|---------|
| **Network Local Player** | Local player's GameObject |
| **Network Player By Client ID** | Player GameObject by client ID |
| **Closest NPC** | Nearest NPC to reference point |
| **Closest Character** | Nearest character to reference point |

### Position/Location Getters

| Property | Returns |
|----------|---------|
| **Network Local Player Position** | Local player's position |
| **Network Local Player Location** | Local player's location (pos + rot) |
| **Network Character Position By Client ID** | Player position by client ID |

### Numeric Getters

| Property | Returns |
|----------|---------|
| **Player Count** | Number of connected players |
| **NPC Count** | Number of spawned NPCs |
| **Ready Player Count** | Number of players marked ready |
| **Countdown** | Current countdown value |
| **Game Time** | Synchronized game time |

---

## COMPONENT SETUP

### Player Prefab Requirements

```
Player_Network (Prefab)
├── NetworkObject (Required)
├── NetworkCharacter (Required - extends Character)
├── NetworkCharacterAdapter (Required - for ownership handling)
├── NetworkTransform (Required - position sync)
├── NetworkAnimator (Required - animation sync)
└── [Other components as needed]
```

### NPC Prefab Requirements

```
NPC_Character (Prefab)
├── NetworkObject (Required)
├── NetworkCharacter (Required - extends Character)
├── NetworkTransform (Required - position sync)
├── NetworkAnimator (Required - animation sync)
├── [NO NetworkCharacterAdapter - NPCs don't need it]
└── [Optional: Perception + NetworkPerception for AI awareness sync]
```

### NPC with Perception (AI Awareness Sync)

```
NPC_Character_WithPerception (Prefab)
├── NetworkObject (Required)
├── NetworkCharacter (Required - extends Character)
├── NetworkTransform (Required - position sync)
├── NetworkAnimator (Required - animation sync)
├── Perception (Required - GameCreator AI awareness)
└── NetworkPerception (Required - syncs awareness across network)
```

### Scene Manager Setup

```
NetworkManagers (GameObject)
├── NetworkManagersBootstrap
├── NetworkInitializationManager
├── NetworkSessionEvents
└── NetworkPerceptionEvents (Required for perception sync)

Network_Manager (GameObject - Unity's)
├── NetworkManager
└── UnityTransport

NetworkSpawnManager (GameObject)
├── NetworkObject
└── NetworkSpawnManager

NetworkGameStateManager (GameObject)
├── NetworkObject
└── NetworkGameStateManager

NetworkRPCManager (GameObject)
├── NetworkObject
└── NetworkRPCManager

NetworkSceneCoordinator (GameObject)
├── NetworkObject
└── NetworkSceneCoordinator
```

---

## COMMON PATTERNS

### Pattern 1: Spawn NPC When Local Player Spawns

```
Trigger: On Local Player Spawned (Network/Session)
  ├── Wait 1 second
  ├── Spawn Network NPC (prefab: NPC_Character, position: Marker_Spawn)
  └── [NPC is available as Target in subsequent instructions if using On NPC Spawned]
```

### Pattern 2: Move NPC on Patrol (Use separate trigger)

**IMPORTANT**: Network-spawned characters need a brief delay before their Motion system is fully initialized.
Always add a **Wait 0.5+ seconds** before movement instructions.

```
Trigger: On NPC Spawned (Network/Characters)
  ├── Wait 0.5 seconds    <-- REQUIRED: Allow Motion system to initialize
  ├── Move [Target] to Marker_1    <-- Target = the spawned NPC
  ├── Wait 2 seconds
  ├── Move [Target] to Marker_2
  └── Restart Instructions
```

### Pattern 3: Host-Only Logic

```
Trigger: On Host Started (Network/Session)
  ├── Set Game State: Lobby
  ├── Enable Server UI
  └── Spawn Initial NPCs
```

### Pattern 4: Client Connection Flow

```
Trigger: On Client Connected (Network/Connection)
  ├── Log "Player joined"
  └── Spawn Network Player (owner: [Connected Client ID])
```

### Pattern 5: NPC Detects Player (Network-Synced Perception)

**IMPORTANT**: Perception triggers require `NetworkPerceptionEvents` in the scene.
All perception events are server-authoritative and broadcast to all clients.

```
Trigger: On Network Awareness Stage (Network/Perception)
  ├── Filter: Stage = Alert or Aware
  ├── Filter: Perception = Any (or specific NPC)
  ├── Filter: Target = Any (or specific player)
  │
  ├── [Args.Self = the NPC with Perception]
  ├── [Args.Target = the detected player]
  │
  ├── Play Alert Animation on Self
  ├── Move Self toward Target
  └── [AI combat logic...]
```

### Pattern 6: Server-Controlled Awareness Increase

```
Trigger: On Host Started (Network/Session)
  └── [Server-side only logic]

Trigger: Custom game logic (e.g., player makes noise)
  ├── Condition: Is Server
  ├── Get all NPCs in range
  ├── For each NPC:
  │   └── Add Network Awareness (NPC Perception, Player, amount: 0.2)
  └── [Awareness synced automatically to all clients]
```

### Pattern 7: NPC Loses Track of Player

```
Trigger: On Network Awareness Stage (Network/Perception)
  ├── Filter: Stage = None
  ├── Filter: When = On Decrease (awareness dropped to None)
  │
  ├── Play Confused Animation on Self
  ├── Move Self to Last Known Position
  └── Wait and return to patrol
```

---

## IMPORTANT NOTES

1. **Server-Only Instructions**: Spawn, Despawn, SetGameState, LoadScene only work on server/host
2. **Args.Target**: Global spawn triggers set Target to the spawned character - use "Get Target" in instructions
3. **NetworkObject Required**: All networked prefabs must have NetworkObject component
4. **Naming**: Network-spawned objects have "(Clone)" removed automatically by NetworkCharacter
5. **Markers**: Scene markers work normally - reference them directly in Location fields
6. **Initialization Delay**: Network-spawned characters need ~0.5s for Motion system initialization. Always add a Wait instruction before Move commands in spawn triggers!
7. **Perception Sync**: Add `NetworkPerception` component to NPCs with `Perception` for network-synced AI awareness
8. **NetworkPerceptionEvents Required**: Add to your NetworkManagers GameObject for perception triggers to work
9. **Server-Authoritative Perception**: All awareness changes are processed by server and broadcast to clients

---

## TROUBLESHOOTING

### "Object reference not set" on Move instruction
- **Most common cause**: Character Motion system not initialized yet
  - **Solution**: Add a **Wait 0.5 seconds** instruction BEFORE the Move instruction
  - Network-spawned characters have deferred initialization for the Animator and Motion systems
- Check that the character reference is valid (use "Get Target" if trigger provides it)
- Check that the Marker exists in the scene and has a Marker component
- Verify the Marker is assigned in the instruction (not "None")

### Character not moving
- Add a Wait instruction before movement (Motion system needs time to initialize)
- Verify NetworkCharacter.Motion is not null (check console for warnings)
- Check if character is server-authoritative (NPCs)
- Ensure NavMesh is baked if using navigation

### NPC spawns but Move fails immediately
- This is almost always due to the Motion system not being ready
- Add "Wait 0.5 seconds" as the FIRST instruction after the spawn trigger
- The NetworkCharacter has deferred Awake initialization for network spawning

### Trigger not firing
- Verify trigger is on correct GameObject (prefab vs scene)
- Check that NetworkSessionEvents exists in scene
- Verify network is started before spawn events fire
- For "On Local Player Spawned" - can be placed ANYWHERE in scene (subscribes to global event)

### Double spawning
- Remove NetworkManager.PlayerPrefab (let GameCreator handle spawning)
- Or disable NetworkSpawnManager.AutoSpawnPlayer

### Perception triggers not firing
- Verify `NetworkPerceptionEvents` component exists in scene (add to NetworkManagers GameObject)
- Check that NPC has both `Perception` AND `NetworkPerception` components
- Ensure network is started before perception events occur
- Check filter settings in trigger (Perception, Target, Stage filters)

### Awareness not syncing across clients
- Add `NetworkPerception` component to the NPC prefab
- Verify NPC has `NetworkObject` component and is spawned
- Check that target (player) also has `NetworkObject` component
- Ensure server is running (awareness is server-authoritative)

### "NetworkPerception not found" warning
- Add `NetworkPerception` component to the GameObject with `Perception`
- Ensure the instruction is targeting the correct GameObject
