# Tasks: Add Multiplayer Components to Scene GameObjects

## Status: REQUIRES UNITY EDITOR

**Note**: Unity MCP server is unavailable. These tasks require manual execution in Unity Editor.
The runtime code is complete - only scene/prefab configuration remains.

---

## 1. Scene Infrastructure Setup (Unity Editor Required)
- [ ] 1.1 Verify NetworkManager exists in scene
- [ ] 1.2 Configure NetworkManager player prefab reference (drag `Player_Network.prefab`)
- [ ] 1.3 Add NetworkSpawnPointIntegration to spawn points
- [ ] 1.4 Configure network transport settings

**Quick Start:** In your main scene, ensure you have a GameObject with NetworkManager component.

---

## 2. Player Network Prefab Configuration (AUTOMATED!)

### 🚀 AUTOMATED SETUP (Recommended)
Run this menu command in Unity Editor:
```
GameCreator → Multiplayer → Setup Player Network Prefab
```
This automatically adds: NetworkCombatAdapter, NetworkTraitsAdapter, Traits, Bag, Perception

### Manual Verification After Automated Setup
- [ ] 2.1 Run menu: `GameCreator → Multiplayer → Verify Player Network Prefab`
- [ ] 2.2 Verify Character component is present and ENABLED
- [ ] 2.3 Verify no NetworkTransform is present (conflicts with NetworkCharacterAdapter!)
- [ ] 2.4 Verify no NetworkGameCreatorCharacterV2 (legacy - breaks animation!)
- [ ] 2.5 Assign a Class asset to Traits component (MANUAL)
- [ ] 2.6 Configure Bag settings (capacity, shape, equipment slots) (MANUAL)

### Component Stack (Reference)
Required components on Player_Network prefab:
1. NetworkObject (Netcode identity)
2. Character (GameCreator - MUST stay enabled)
3. CharacterController (Unity physics)
4. Animator (animation controller)
5. NetworkCharacterAdapter (position/rotation sync)
6. NetworkGameCreatorAnimator (animation sync)
7. NetworkPlayerController (unified coordinator)
8. NetworkCombatAdapter (combat sync)
9. NetworkTraitsAdapter (stats sync)

---

## 3. Scene GameObjects - Interactables (Unity Editor Required)
- [ ] 3.1 Identify all interactable GameObjects in scene
- [ ] 3.2 For each interactable, add NetworkObject component
- [ ] 3.3 Add NetworkTriggerAuthority to trigger-based interactables
- [ ] 3.4 Add NetworkActionAuthority to action-based interactables
- [ ] 3.5 Configure authority settings (server vs owner)

**Components available**: `NetworkTriggerAuthority.cs`, `NetworkActionAuthority.cs`

---

## 4. Scene GameObjects - NPCs (Unity Editor Required)
- [ ] 4.1 Identify all NPC GameObjects in scene
- [ ] 4.2 For each NPC, add NetworkObject component
- [ ] 4.3 Add NetworkNPCCharacter component
- [ ] 4.4 Add NetworkNPCBehavior component (if AI-controlled)
- [ ] 4.5 Configure NPC network settings

**Reference prefab**: `Runtime/NPC/NPC.prefab`

---

## 5. Scene GameObjects - Pickups/Items (Unity Editor Required)
- [ ] 5.1 Identify all pickup/item GameObjects in scene
- [ ] 5.2 For each pickup, add NetworkObject component
- [ ] 5.3 Add NetworkPickup component
- [ ] 5.4 Configure pickup network settings

**Component available**: `Runtime/Objects/NetworkPickup.cs`

---

## 6. Validation & Testing (Unity Play Mode)
- [ ] 6.1 Run NetworkPlayerController context menu: "Debug Player State"
- [ ] 6.2 Verify no NetworkTransform components on characters
- [ ] 6.3 Test host gameplay (local player movement)
- [ ] 6.4 Test client connection and spawning
- [ ] 6.5 Verify remote player animation sync
- [ ] 6.6 Verify position/rotation interpolation smoothness
- [ ] 6.7 Test interactable objects across network
- [ ] 6.8 Verify pickup synchronization
- [ ] 6.9 Document any issues found

---

## 7. Documentation Update
- [x] 7.1 Update tasks.md with setup instructions (DONE - this file)
- [ ] 7.2 Document component configuration options
- [ ] 7.3 Add troubleshooting section for common issues

---

## Quick Reference: Where Components Live

| Component | Path |
|-----------|------|
| NetworkCharacterAdapter | `Runtime/Components/NetworkCharacterAdapter.cs` |
| NetworkGameCreatorAnimator | `Runtime/Components/NetworkGameCreatorAnimator.cs` |
| NetworkPlayerController | `Runtime/Components/NetworkPlayerController.cs` |
| NetworkTraitsAdapter | `Runtime/Components/NetworkTraitsAdapter.cs` |
| NetworkInventorySync | `Runtime/Components/NetworkInventorySync.cs` |
| NetworkPerceptionSync | `Runtime/Modules/NetworkPerceptionSync.cs` |
| NetworkTriggerAuthority | `Runtime/Components/NetworkTriggerAuthority.cs` |
| NetworkActionAuthority | `Runtime/Components/NetworkActionAuthority.cs` |
| NetworkNPCCharacter | `Runtime/Components/NetworkNPCCharacter.cs` |
| NetworkNPCBehavior | `Runtime/Components/NetworkNPCBehavior.cs` |
| NetworkPickup | `Runtime/Objects/NetworkPickup.cs` |
| Player Prefab | `Runtime/Player/Player_Network.prefab` |
| NPC Prefab | `Runtime/NPC/NPC.prefab` |

## Critical Architecture Note

**DO NOT USE NetworkTransform!**
Per `.serena/memories/CRITICAL/002_network_architecture_never_forget.md`:
- NetworkCharacterAdapter handles character sync via state-based approach
- NetworkTransform conflicts with CharacterController and causes jitter
- If you see NetworkTransform on a character, REMOVE IT
