# Proposal: Multiplayer Skill Check Scene Conversion

## Summary

Convert the `7_Skill_Check` scene from a single-player GameCreator Stats + Factions demo to a fully functional multiplayer scene, reverse-engineering the process to document patterns for future GC module integration.

## Motivation

The scene currently has:
- **Stat Pickup System**: 12+ `Trigger_XP` orbs that give +35 Strength on pickup
- **Door Skill Check**: Requires 24+ Strength to open (needs ~1 pickup)
- **Faction NPCs on Pedestals**: Character_Goblin, Character_Elf, Character_Demon, Character_Orc, Character_Human
- **Faction-based behavior**: NPCs react based on player's faction membership
- **Network infrastructure**: NetworkManager, NetworkPlayerManager, spawn triggers already in place

Missing for multiplayer:
- **Pickup synchronization**: Only one player can pick up each orb, all clients see it disappear
- **Server-authoritative stats**: Strength changes must go through server
- **NPC network spawning**: NPCs need NetworkObject for sync
- **Faction state sync**: Faction membership and reputation across clients
- **Door state sync**: Open/closed state visible to all players

## Scope

### Already Existing (Reuse)
- `NetworkPickup.cs` - Server-authoritative pickup with respawn, validation, distributed authority
- `NetworkStatsSync.cs` - Batched health/mana/stamina sync (86% bandwidth reduction)
- `NetworkPlayerManager.cs` - Player spawning system
- `NetworkCharacterAdapter.cs` - Character movement sync

### In Scope (New/Extend)
1. **NetworkStatPickup** - Extend NetworkPickup for custom stat pickups (Strength +35)
2. **Custom Stats Support** - Extend NetworkStatsSync for arbitrary GameCreator stats
3. **NetworkDoor** - Door state sync with stat requirement validation
4. **NetworkFactionMemberSync** - Sync faction membership across network
5. **NetworkNPCManager** - Server-side NPC spawning for faction characters
6. **Network Faction Triggers** - Update `InstructionVariablesCollectFactionMembers` for network
7. **Scene prefab conversion** - Add NetworkObject to all interactive entities

### Out of Scope
- New faction UI (use existing Factions.UI package)
- Combat system (separate proposal)
- Persistence to Supabase (future enhancement)

## Technical Analysis

### Current Scene Structure (from Unity Screenshot)

```
7_Skill_Check.unity
├── NetworkManager (existing)
├── NetworkPlayerManager (existing)
├── LocalPlayerResolver (existing)
├── UnifiedRPCManager (existing)
├── NetworkDebugManager (existing)
├── EventSystem
├── Main_Camera
├── Light
├── Geometry (environment)
├── Tutorial_SkillCheck (UI text explaining mechanics)
├── Experience (stat display UI)
├── Trigger_XP (×12+) - Stat pickups giving +35 Strength
│   └── On Trigger Enter: Local Player → Add Strength +35 → Destroy Self
├── Door (requires Strength >= 24 to open)
├── SpawnPoint, SpawnPoint2 (Player spawn points)
├── Canvas_HOST_CLIENT (UI for Host/Client selection)
├── Trigger_OnSessionStart
├── Trigger_OnHostPlayerSpawned
├── Trigger_OnNPCSpawned
├── Trigger_OnClientPlayerSpawned
├── Character_Goblin (NPC on pedestal)
├── Character_Elf (NPC on pedestal)
├── Character_Demon (NPC on pedestal)
├── Character_Orc (NPC on pedestal)
├── Character_Human (NPC on pedestal)
├── Trigger_Hostile → InstructionVariablesCollectFactionMembers
├── Trigger_Neutral → InstructionVariablesCollectFactionMembers
├── Trigger_Friendly → InstructionVariablesCollectFactionMembers
├── ListVariables (stores collected faction members)
└── UI
```

### Existing Multiplayer Infrastructure

The project already has:
- `NetworkPlayerManager` - Complete player spawning system
- `NetworkCharacterAdapter` - Character sync (no NetworkTransform!)
- `InstructionNetworkStartHost/Client` - Visual scripting network start
- `InstructionNetworkSpawnHostPlayer/ClientPlayer` - Player spawning instructions
- `EventOnHostPlayerSpawned/ClientPlayerSpawned` - Spawn events

### Missing Components for Factions

1. **NetworkFactionMemberSync** - Sync `Member` component state
2. **NetworkFactionReputationSync** - Sync reputation values
3. **Network-aware `InstructionVariablesCollectFactionMembers`** - Only collect network-spawned members
4. **NPC NetworkObject setup** - Each NPC needs NetworkObject + NetworkCharacterAdapter

## Design Decisions

### Decision 1: Faction State Authority
**Choice**: Server-authoritative faction state
- Server owns all faction membership and reputation values
- Clients request faction changes via ServerRpc
- Faction events broadcast via ClientRpc

### Decision 2: NPC Ownership
**Choice**: Server owns all NPCs
- NPCs are spawned by server on scene load
- No ownership transfer (NPCs are always server-controlled)
- Clients see NPC state via NetworkVariables

### Decision 3: Faction Collection Pattern
**Choice**: Modify existing instruction to be network-aware
- Check `IsNetworkSpawned` before including in collection
- Add `NetworkFactionMemberSync` to all faction members
- Use spatial hash that only includes spawned members

## Implementation Approach

### Phase 1: Network Components
1. Create `NetworkFactionMemberSync.cs` - Syncs faction ID and membership state
2. Create `NetworkFactionReputationSync.cs` - Syncs reputation points
3. Create `NetworkNPCManager.cs` - Handles NPC spawning on server

### Phase 2: Scene Conversion
1. Add NetworkObject to all NPC characters
2. Add NetworkFactionMemberSync to all NPCs
3. Configure spawn points for NPCs
4. Update triggers to use network-aware conditions

### Phase 3: Visual Scripting Updates
1. Create `InstructionNetworkCollectFactionMembers` (or update existing)
2. Add `ConditionIsFactionMember` for network context
3. Add `EventOnFactionMemberSpawned` trigger

### Phase 4: Testing & Documentation
1. Test Host + Client faction interactions
2. Document patterns in decision trees
3. Update `gc-visual-scripting-sync.yaml` with faction examples

## Success Criteria

- [ ] Host can see all NPCs with correct factions
- [ ] Client joins and sees same NPCs synced
- [ ] Faction triggers (`Trigger_Friendly`, etc.) work for both Host and Client
- [ ] Stats on NPCs are synced (health, XP)
- [ ] Door skill check works in multiplayer
- [ ] No NetworkTransform used (only NetworkCharacterAdapter)

## References

- Decision Trees: `gc-visual-scripting-sync.yaml`, `gc-stats-sync.yaml`, `gc-character-sync.yaml`
- Critical Memory: `.serena/memories/CRITICAL/001_gamecreator_invasive_integration.md`
- Existing Infrastructure: `Assets/Plugins/GameCreator/Packages/MLCreator_Multiplayer/`
