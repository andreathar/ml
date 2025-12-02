# Tasks: Multiplayer Skill Check Scene Conversion

## Phase 0: Network Pickup System (Priority - Core Gameplay)

**EXISTING INFRASTRUCTURE:**
- `NetworkPickup.cs` - Already exists! Handles server-authoritative pickups with respawn support
- `NetworkStatsSync.cs` - Already exists! Handles health/mana/stamina batching
- Need: Extend to support custom GameCreator stats (Strength, etc.)

### 0.1 Network Stats Pickup Component
- [x] 0.1.1 Create `NetworkStatPickup.cs` extending NetworkPickup for stat-based pickups
- [x] 0.1.2 Add configurable stat name (e.g., "Strength") instead of item ID
- [x] 0.1.3 Add ServerRpc for stat pickup (`RequestPickupServerRpc`)
- [x] 0.1.4 Apply stat change server-side via GameCreator Traits API
- [x] 0.1.5 Sync stat change to all clients via ClientRpc

**Implementation:** `Runtime/Pickups/NetworkStatPickup.cs`
- Configurable stat ID and amount
- Auto-pickup on trigger enter
- Server-side validation (distance check)
- Respawn support

### 0.2 Extend NetworkStatsSync for Custom Stats
- [x] 0.2.1 Create `InstructionNetworkChangeStat.cs` for VS stat changes
- [x] 0.2.2 Create `NetworkStatChanger` helper component
- [x] 0.2.3 Integrate with GameCreator Traits component for reading/writing stats
- [ ] 0.2.4 Add batched custom stats NetworkVariable (deferred - basic version works)

**Implementation:** `Runtime/VisualScripting/Instructions/InstructionNetworkChangeStat.cs`

### 0.3 Convert Trigger_XP Pickups
- [ ] 0.3.1 Add NetworkObject to Trigger_XP prefab
- [ ] 0.3.2 Add NetworkStatPickup component (configured for Strength +35)
- [ ] 0.3.3 Update trigger to use network-aware pickup
- [ ] 0.3.4 Test: Only picker gets Strength, all clients see orb despawn

### 0.4 Door Network Sync
- [x] 0.4.1 Create `NetworkDoor.cs` with NetworkVariable for state (`m_IsOpen`)
- [x] 0.4.2 Add ServerRpc for door interaction (`TryInteractServerRpc`)
- [x] 0.4.3 Server validates player's Strength stat before opening
- [x] 0.4.4 Sync door animation/state to all clients via ClientRpc
- [ ] 0.4.5 Add NetworkDoor component to Door in scene

**Implementation:** `Runtime/Objects/NetworkDoor.cs`
- Configurable stat requirement (stat ID + value + comparison)
- Transform-based or Animator-based door animation
- Auto-close support
- Lock after open option

## Phase 1: Network Faction Components

### 1.1 NetworkFactionMemberSync Component
- [x] 1.1.1 Create `NetworkFactionMemberSync.cs` in `MLCreator_Multiplayer/Runtime/Modules_Sync/`
- [x] 1.1.2 Add NetworkVariable for faction ID (`m_FactionId`)
- [x] 1.1.3 Add NetworkVariable for membership status (`m_IsActive`)
- [x] 1.1.4 Sync with GameCreator `Member` component on spawn
- [x] 1.1.5 Add OnValueChanged callbacks to update local `Member` component

**Implementation:** `Runtime/Modules_Sync/NetworkFactionMemberSync.cs`
- Reflection-based GameCreator Factions integration
- Server-authoritative faction assignment
- Faction status queries (hostile/neutral/friendly)

### 1.2 NetworkFactionReputationSync Component
- [ ] 1.2.1 Create `NetworkFactionReputationSync.cs`
- [ ] 1.2.2 Add batched NetworkVariable for reputation values (like StatsBatch pattern)
- [ ] 1.2.3 Add ServerRpc for reputation changes
- [ ] 1.2.4 Add ClientRpc for reputation change events (for UI updates)

### 1.3 NetworkNPCManager Component
- [x] 1.3.1 Create `NetworkNPCManager.cs` for server-side NPC spawning
- [x] 1.3.2 Add NPC prefab registry with faction assignments
- [x] 1.3.3 Add spawn logic triggered on server start
- [x] 1.3.4 Add late-join sync (spawn NPCs for connecting clients)

**Implementation:** `Runtime/Characters/NPC/NetworkNPCManager.cs`
- Singleton pattern
- NPCSpawnConfig with prefab, faction, spawn point
- Scene NPC support (existing NPCs in scene)
- Query API (GetNPCsByFaction, GetAllNPCs)

## Phase 2: Scene Entity Conversion

### 2.1 NPC Prefab Setup
- [ ] 2.1.1 Create networked NPC prefab template with required components
- [ ] 2.1.2 Add NetworkObject component to each NPC in scene
- [ ] 2.1.3 Add NetworkCharacterAdapter to each NPC
- [ ] 2.1.4 Add NetworkFactionMemberSync to each NPC
- [ ] 2.1.5 Add NetworkStatsSync (reuse existing gc-stats-sync pattern)

### 2.2 Scene Hierarchy Updates
- [ ] 2.2.1 Convert Character_Elf to networked NPC
- [ ] 2.2.2 Convert Character_Demon to networked NPC
- [ ] 2.2.3 Convert Character_Goblin to networked NPC
- [ ] 2.2.4 Convert Character_Orc to networked NPC
- [ ] 2.2.5 Convert Character_Human to networked NPC

### 2.3 Interactive Objects
- [ ] 2.3.1 Verify Door NetworkObject is configured correctly
- [ ] 2.3.2 Add network sync to door state (open/closed)
- [ ] 2.3.3 Update Trigger_Door to use network-aware conditions

## Phase 3: Visual Scripting Network Updates

### 3.1 Faction Collection Instructions
- [ ] 3.1.1 Create `InstructionNetworkCollectFactionMembers.cs`
- [ ] 3.1.2 Filter to only include `IsNetworkSpawned` members
- [ ] 3.1.3 Use server-side spatial hash for consistency
- [ ] 3.1.4 Sync collected list to requesting client

### 3.2 Faction Conditions
- [ ] 3.2.1 Create `ConditionNetworkIsFactionMember.cs`
- [ ] 3.2.2 Create `ConditionNetworkFactionStatus.cs`
- [ ] 3.2.3 Add `ConditionIsNPCNetworkSpawned.cs`

### 3.3 Faction Events
- [ ] 3.3.1 Create `EventOnNetworkFactionMemberSpawned.cs`
- [ ] 3.3.2 Create `EventOnNetworkReputationChanged.cs`
- [ ] 3.3.3 Add `EventOnNPCFactionInteract.cs` (for NPC interaction with faction context)

### 3.4 Update Scene Triggers
- [ ] 3.4.1 Update Trigger_Friendly to use network-aware instruction
- [ ] 3.4.2 Update Trigger_Neutral to use network-aware instruction
- [ ] 3.4.3 Update Trigger_Hostile to use network-aware instruction
- [ ] 3.4.4 Update Trigger_Door skill check for multiplayer

## Phase 4: Integration & Testing

### 4.1 Scene Integration
- [ ] 4.1.1 Add NetworkNPCManager to scene
- [ ] 4.1.2 Configure NPC spawn points
- [ ] 4.1.3 Set up NPC prefab references
- [ ] 4.1.4 Test spawn sequence on Host start

### 4.2 Multiplayer Testing
- [ ] 4.2.1 Test Host sees all NPCs with correct factions
- [ ] 4.2.2 Test Client joins and sees synced NPCs
- [ ] 4.2.3 Test faction triggers work for Host
- [ ] 4.2.4 Test faction triggers work for Client
- [ ] 4.2.5 Test Door skill check in multiplayer

### 4.3 Documentation
- [ ] 4.3.1 Document NetworkFactionMemberSync usage pattern
- [ ] 4.3.2 Add faction sync examples to gc-visual-scripting-sync.yaml
- [ ] 4.3.3 Create Serena memory for Faction network integration

## Acceptance Criteria

- [ ] Host can spawn and see all 5 NPCs with correct factions
- [ ] Client connects and sees identical NPC state
- [ ] `Trigger_Friendly` collects correct NPCs for both Host and Client
- [ ] `Trigger_Hostile` collects correct NPCs for both Host and Client
- [ ] `Trigger_Neutral` collects correct NPCs for both Host and Client
- [ ] Door skill check (`ConditionCompareStat`) works for network players
- [ ] No NetworkTransform components used
- [ ] All faction state is server-authoritative
- [ ] XP/Stats sync correctly for all characters

## Technical Notes

### Key Constraints (from decision trees)
- **No NetworkTransform**: Use NetworkCharacterAdapter for all characters
- **Server-Authoritative**: All faction/stats mutations via ServerRpc
- **Task Signature**: `protected override Task Run(Args args)` - NO CancellationToken
- **RPC Naming**: All RPCs end in `ServerRpc` or `ClientRpc`

### Existing Code to Reuse
- `NetworkPlayerManager` - Reference for spawn patterns
- `NetworkCharacterAdapter` - Character sync (add to NPCs)
- `NetworkStatsSync` - Stats batching pattern
- `InstructionVariablesCollectFactionMembers` - Base for network version

### Files Created (Phase 0-1 Complete)
```
MLCreator_Multiplayer/Runtime/Pickups/
└── NetworkStatPickup.cs                    ✅ CREATED

MLCreator_Multiplayer/Runtime/Objects/
└── NetworkDoor.cs                          ✅ CREATED

MLCreator_Multiplayer/Runtime/Modules_Sync/
└── NetworkFactionMemberSync.cs             ✅ CREATED

MLCreator_Multiplayer/Runtime/Characters/NPC/
└── NetworkNPCManager.cs                    ✅ CREATED

MLCreator_Multiplayer/Runtime/VisualScripting/Instructions/
└── InstructionNetworkChangeStat.cs         ✅ CREATED
```

### Files to Create (Remaining)
```
MLCreator_Multiplayer/Runtime/Modules_Sync/
└── NetworkFactionReputationSync.cs

MLCreator_Multiplayer/Runtime/VisualScripting/Instructions/
└── InstructionNetworkCollectFactionMembers.cs

MLCreator_Multiplayer/Runtime/VisualScripting/Conditions/
├── ConditionNetworkIsFactionMember.cs
└── ConditionNetworkFactionStatus.cs

MLCreator_Multiplayer/Runtime/VisualScripting/Events/
├── EventOnNetworkFactionMemberSpawned.cs
└── EventOnNetworkReputationChanged.cs
```

## Implementation Summary

**Created 5 new components:**

1. **NetworkStatPickup** - Pickup that modifies GameCreator stats
   - Server-authoritative with client visual sync
   - Configurable stat ID, amount, and change type
   - Respawn support

2. **NetworkDoor** - Door with stat requirement validation
   - Configurable stat check (ID, value, comparison)
   - Transform or Animator animation
   - Auto-close and lock options

3. **NetworkFactionMemberSync** - Faction membership sync
   - Reflection-based GC Factions integration
   - Faction status queries
   - Server-authoritative faction assignment

4. **NetworkNPCManager** - Server-side NPC management
   - Prefab and scene NPC support
   - Auto-spawn on server start
   - Query API for faction-based NPC lookup

5. **InstructionNetworkChangeStat** - VS instruction for network stats
   - Server-authoritative stat changes
   - Works with Traits component
