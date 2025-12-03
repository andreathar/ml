# Tasks: Fix Hotspot and Interaction Conflicts

## 1. Analysis & Audit

- [ ] 1.1 Scan all scenes for objects with NetworkTransform on static triggers/hotspots
- [ ] 1.2 Identify all Trigger objects using "Local Player" target
- [ ] 1.3 Document current NetworkHotspotExtension usage in scenes
- [ ] 1.4 List all interaction patterns currently in use

## 2. Fix LocalPlayerResolver Hotspot Patching

- [ ] 2.1 Create `GetGameObjectLocalNetworkPlayer` PropertyGetGameObject class
- [ ] 2.2 Register PropertyGetGameObject type with GameCreator
- [ ] 2.3 Update `UpdateHotspotTarget()` to use proper PropertyGetGameObject injection
- [ ] 2.4 Add fallback for tag-based lookup during initialization

## 3. Create NetworkTriggerAdapter Component

- [ ] 3.1 Create `NetworkTriggerAdapter.cs` in `Runtime/Interactions/`
- [ ] 3.2 Implement OnTriggerEnter interception for network validation
- [ ] 3.3 Add TargetMode enum (LocalPlayerOnly, AnyPlayer, FirstOnly, ServerValidated)
- [ ] 3.4 Implement server-side validation for FirstOnly mode
- [ ] 3.5 Add "Consume On Trigger" option for one-time pickups
- [ ] 3.6 Handle Destroy synchronization across clients

## 4. Fix NetworkHotspotExtension Integration

- [ ] 4.1 Detect attached GC Trigger component in Awake
- [ ] 4.2 Disable direct Trigger execution when NetworkHotspotExtension present
- [ ] 4.3 Route Trigger execution through NetworkHotspotExtension validation
- [ ] 4.4 Fix `TriggerGameCreatorHotspotClientRpc` to actually execute Trigger
- [ ] 4.5 Add support for Trigger's Conditions child object

## 5. Editor Validation & Warnings

- [ ] 5.1 Create `NetworkInteractionValidator.cs` editor script
- [ ] 5.2 Add warning for NetworkTransform on objects without movement
- [ ] 5.3 Add warning for Trigger + NetworkHotspotExtension without proper config
- [ ] 5.4 Add warning for NetworkObject on scene-static pickup objects
- [ ] 5.5 Add "Fix" button to auto-remove problematic components
- [ ] 5.6 Add menu item: "GameCreator/Multiplayer/Validate Interactions"

## 6. Scene Cleanup

- [ ] 6.1 Remove NetworkTransform from all Trigger_XP objects in 7_Skill_Check
- [ ] 6.2 Remove NetworkObject from static pickup triggers (if any)
- [ ] 6.3 Configure NetworkHotspotExtension correctly or remove if not needed
- [ ] 6.4 Verify Trigger "Collider: Specific → Local Player" works correctly
- [ ] 6.5 Test XP pickup in multiplayer (host and client)
- [ ] 6.6 Test door trigger in multiplayer

## 7. Documentation

- [ ] 7.1 Create `claudedocs/guides/MULTIPLAYER_INTERACTIONS.md`
- [ ] 7.2 Document when to use Trigger vs NetworkHotspotExtension
- [ ] 7.3 Document when NetworkObject is required vs not
- [ ] 7.4 Add examples for common scenarios (pickup, door, chest, NPC dialogue)
- [ ] 7.5 Update `.serena/memories/` with critical interaction patterns

## 8. Testing

- [ ] 8.1 Create test scene with all interaction patterns
- [ ] 8.2 Test: Local-only XP pickup (each player collects their own)
- [ ] 8.3 Test: Synced one-time pickup (first player gets, destroyed for all)
- [ ] 8.4 Test: Door that any player can open
- [ ] 8.5 Test: Interactive object requiring hold interaction
- [ ] 8.6 Test: Conditional trigger (skill check like Trigger_Door)
