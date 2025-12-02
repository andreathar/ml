# Tasks: GameCreator Player Component Targeting Research

**Status: RESEARCH COMPLETE** ✅
**Report:** `claudedocs/reports/GAMECREATOR_PLAYER_TARGETING_RESEARCH.md`

## 1. Component Inventory

### 1.1 Core GameCreator Components
- [x] 1.1.1 Audit `Assets/Plugins/GameCreator/Packages/Core/Runtime/` for player references
- [x] 1.1.2 Document all `PropertyGetGameObject` implementations
- [x] 1.1.3 List all Trigger types and their target resolution
- [x] 1.1.4 List all Hotspot configurations and target fields
- [x] 1.1.5 Map Camera system components (CameraShot, ShotType, etc.)

### 1.2 Multiplayer Extension Components
- [x] 1.2.1 Audit `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/`
- [x] 1.2.2 Document existing network-aware PropertyGet implementations
- [x] 1.2.3 Identify gaps between GC core and multiplayer variants
- [x] 1.2.4 Review LocalPlayerResolver coverage

### 1.3 Visual Scripting Audit
- [x] 1.3.1 List all Instructions that reference players/characters
- [x] 1.3.2 List all Conditions that check player state
- [x] 1.3.3 List all Events triggered by player actions
- [x] 1.3.4 Document PropertyGet usage in each

### 1.4 Module-Specific Components
- [x] 1.4.1 Inventory module player references
- [x] 1.4.2 Stats module player references
- [x] 1.4.3 Perception module player references
- [x] 1.4.4 Dialogue module player references
- [x] 1.4.5 Quest module player references

**FINDING: 357 components use `GetGameObjectPlayer.Create()` across all modules**

## 2. Positioning Investigation

### 2.1 Spawn Flow Analysis
- [x] 2.1.1 Trace NetworkPlayerManager.SpawnPlayer() complete flow
- [x] 2.1.2 Document timing of Character.InitializeNetwork()
- [x] 2.1.3 Document timing of CharacterController initialization
- [x] 2.1.4 Identify where transform.position is set/modified
- [x] 2.1.5 Check for physics race conditions (Physics.SyncTransforms)

**FINDING: CharacterController.center race condition in UnitDriverController:257-276**

### 2.2 CharacterController Analysis
- [x] 2.2.1 Review CharacterController.center initialization
- [x] 2.2.2 Check for competing systems modifying center
- [x] 2.2.3 Document expected center value for network characters
- [x] 2.2.4 Test spawn with debug logging at each step

**FINDING: center set to (0,0,0) when IsNetworkSpawned=false**

### 2.3 Ground Detection
- [x] 2.3.1 Review UnitDriverController ground checking
- [x] 2.3.2 Check if network state affects IsGrounded
- [x] 2.3.3 Document expected vs actual position after spawn

## 3. Trigger/Interaction Analysis

### 3.1 Duplicate Trigger Investigation
- [x] 3.1.1 Set up test scene with single XP trigger
- [x] 3.1.2 Add comprehensive logging to trigger
- [x] 3.1.3 Test with 1 player - count triggers
- [x] 3.1.4 Test with 2 players - count triggers per player
- [x] 3.1.5 Identify where duplication occurs

**FINDING: LocalPlayerOnly fires independently on ALL clients (N players = N triggers)**

### 3.2 Trigger Target Resolution
- [x] 3.2.1 Document how Trigger resolves "Player" target
- [x] 3.2.2 Check if NetworkTriggerAdapter is in use
- [x] 3.2.3 Compare behavior with/without NetworkHotspotExtension
- [x] 3.2.4 Test "Local Network Player" targeting

**FINDING: NetworkTriggerAdapter:172-191 HandleLocalPlayerOnly() broken**

### 3.3 Authority Analysis
- [x] 3.3.1 Document which triggers should be client-authoritative
- [x] 3.3.2 Document which triggers should be server-authoritative
- [x] 3.3.3 Check current authority implementation
- [x] 3.3.4 Identify authority conflicts

## 4. Strategy Development

### 4.1 Targeting Matrix Creation
- [x] 4.1.1 Create spreadsheet with all components
- [x] 4.1.2 Categorize each: LocalPlayer/AnyPlayer/OwnerOnly/ServerAuth/AllClients
- [x] 4.1.3 Document current vs desired behavior
- [x] 4.1.4 Prioritize by impact

**OUTPUT: Targeting Strategy Matrix in research report**

### 4.2 PropertyGetGameObject Strategy
- [x] 4.2.1 Document existing GetGameObjectLocalNetworkPlayer
- [x] 4.2.2 Identify need for additional variants
- [x] 4.2.3 Design interface for network-aware targeting
- [x] 4.2.4 Plan deployment strategy

**FINDING: Need GetGameObjectAnyNetworkPlayer, GetGameObjectNetworkPlayerById**

### 4.3 Scene Configuration Strategy
- [x] 4.3.1 Audit existing scenes for trigger/hotspot configurations
- [x] 4.3.2 Document patterns that work correctly
- [x] 4.3.3 Document patterns that fail
- [x] 4.3.4 Create configuration guidelines

## 5. Documentation & Reporting

### 5.1 Research Report
- [x] 5.1.1 Compile component inventory findings
- [x] 5.1.2 Document positioning root cause
- [x] 5.1.3 Document trigger duplication root cause
- [x] 5.1.4 Write executive summary

**OUTPUT: `claudedocs/reports/GAMECREATOR_PLAYER_TARGETING_RESEARCH.md`**

### 5.2 Implementation Recommendations
- [x] 5.2.1 Prioritize fixes by impact and effort
- [x] 5.2.2 Document breaking changes required
- [x] 5.2.3 Propose migration strategy
- [x] 5.2.4 Estimate implementation effort

**ESTIMATE: 25-35 developer days for full remediation**

### 5.3 Testing Plan
- [x] 5.3.1 Define test scenarios for positioning
- [x] 5.3.2 Define test scenarios for targeting
- [x] 5.3.3 Create validation checklist
- [x] 5.3.4 Document expected outcomes

---

## Research Summary

| Phase | Status | Key Finding |
|-------|--------|-------------|
| Component Inventory | ✅ COMPLETE | 357 components affected |
| Positioning | ✅ COMPLETE | CharacterController.center race condition |
| Trigger Duplication | ✅ COMPLETE | LocalPlayerOnly fires on all clients |
| Targeting Matrix | ✅ COMPLETE | 5 targeting categories defined |
| Report | ✅ COMPLETE | Full recommendations documented |
