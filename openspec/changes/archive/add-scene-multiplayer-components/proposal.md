# Change: Add Multiplayer Components to Scene GameObjects

## Why

The current Unity scene lacks the required multiplayer components for GameCreator integration with Unity Netcode. Each networked GameObject needs specific components to work correctly in multiplayer, and the Player Network Prefab requires proper configuration for seamless multiplayer character synchronization.

**Problem:**
- Scene GameObjects missing NetworkObject components
- Player prefab lacks unified NetworkPlayerController setup
- Animation, stats, inventory, and perception not synchronized
- Missing server authority configuration for anti-cheat

## What Changes

### Scene Infrastructure
- [ ] Add `NetworkManager` to scene (if missing)
- [ ] Configure NetworkManager with player prefab reference
- [ ] Add `NetworkSpawnPointIntegration` for spawn point management

### Player Network Prefab Components (Required Stack)
**Core Components (Required):**
1. **NetworkObject** - Netcode identity (auto-added by RequireComponent)
2. **Character** - GameCreator character component (MUST remain enabled)
3. **CharacterController** - Unity physics controller
4. **Animator** - Animation controller

**Multiplayer Sync Components (Required):**
5. **NetworkCharacterAdapter** - Position/rotation sync (owner authority)
   - Uses adaptive send rate (12Hz base, reduces for idle)
   - Delta compression for bandwidth optimization
   - Velocity prediction for smoother remote interpolation
6. **NetworkGameCreatorAnimator** - Animation parameter sync (60Hz)
7. **NetworkPlayerController** - Unified module coordinator
   - Auto-detects and initializes all sync modules
   - Provides PlayerName, IsReady, TeamId NetworkVariables
   - Coordinates server authority settings

**Optional Sync Modules (Auto-detected by NetworkPlayerController):**
8. **NetworkTraitsAdapter** - Stats/attributes sync (requires Traits component)
9. **NetworkInventorySync** - Inventory/equipment sync (requires Bag)
10. **NetworkPerceptionSync** - Awareness/targeting sync (requires Perception)

### Interactable Objects (Scene GameObjects)
For each interactable GameObject that needs multiplayer sync:
- **NetworkObject** - Required for any networked GameObject
- **NetworkTriggerAuthority** - For triggers that execute across network
- **NetworkActionAuthority** - For GameCreator Actions that need sync

### NPCs
- **NetworkObject** - Netcode identity
- **NetworkNPCCharacter** - NPC-specific sync (no player input)
- **NetworkNPCBehavior** - AI behavior synchronization

### Pickups/Items
- **NetworkObject** - Netcode identity
- **NetworkPickup** - Pickup state synchronization

## Impact

- **Affected specs**: multiplayer-character-sync, gamecreator-integration
- **Affected code**:
  - Player Network Prefab (needs component stack)
  - Scene hierarchy (needs NetworkManager)
  - Any interactable GameObjects
- **Breaking**: None - additive changes only

## Architecture Notes

### CRITICAL: No NetworkTransform
Per `.serena/memories/CRITICAL/002_network_architecture_never_forget.md`:
- **DO NOT** add NetworkTransform to characters
- NetworkCharacterAdapter handles sync via state-based approach
- NetworkTransform conflicts with CharacterController and causes jitter

### Component Execution Order
```
Character (GameCreator)     → -1 (ApplicationManager.EXECUTION_ORDER_DEFAULT)
NetworkCharacterAdapter     → -50 (EXECUTION_ORDER_FIRST)
NetworkMultiplayerCharacter → -40 (EXECUTION_ORDER_FIRST + 10)
NetworkPlayerController     → -50 (EXECUTION_ORDER_FIRST)
```

### Authority Model
- **Owner Authority**: Movement, rotation, input
- **Server Authority**: Stats, inventory, team assignment (anti-cheat)

## Unity MCP Integration

**Note**: Unity MCP server was unavailable during proposal creation. When available, use these tools:
- `Scene_GetHierarchy` - Analyze current scene structure
- `GameObject_Find` - Locate specific GameObjects
- `GameObject_AddComponent` - Add required components
- `Assets_Prefab_Open/Save` - Modify Player Network Prefab

## Manual Setup Instructions

If Unity MCP is unavailable, manually add components in Unity Editor:

### 1. Scene Setup
1. Create empty GameObject "NetworkManager"
2. Add `NetworkManager` component
3. Configure Player Prefab reference
4. Add spawn points with NetworkSpawnPointIntegration

### 2. Player Prefab Setup
1. Open Player Network Prefab
2. Ensure components in this order:
   - Character (enabled!)
   - CharacterController
   - Animator
   - NetworkObject
   - NetworkCharacterAdapter
   - NetworkGameCreatorAnimator
   - NetworkPlayerController
3. Remove any NetworkTransform (if present)
4. Remove NetworkGameCreatorCharacterV2 (if present - it disables Character!)

### 3. Validation
Use context menu: **NetworkPlayerController → Debug Player State** to verify setup
