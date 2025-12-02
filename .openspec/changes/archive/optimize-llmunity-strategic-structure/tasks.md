# Tasks: Optimize LLMUnity Strategic Structure

## Overview
Migrate LLM integration from parallel structure to unified Game Creator Multiplayer architecture.

**Risk Level**: Medium (file moves + namespace changes)
**Estimated Time**: 2-3 hours
**Parallelizable**: Phases are sequential, but tasks within Phase 3 can be parallel

---

## Phase 1: Preparation & Setup (Low Risk)

### 1. Create Directory Structure
**Status**: ⏳ Pending
**Location**: Unity Editor
**Commands**:
```bash
mkdir -p "Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM"
mkdir -p "Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/Core"
mkdir -p "Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/Instructions"
mkdir -p "Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/Events"
```
**Validation**: Directories exist in Unity Project window

---

### 2. Create Assembly Definition
**Status**: ⏳ Pending
**Location**: `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/`
**File**: `GameCreator.Multiplayer.Runtime.LLM.asmdef`

**Content**:
```json
{
  "name": "GameCreator.Multiplayer.Runtime.LLM",
  "rootNamespace": "GameCreator.Multiplayer.Runtime.LLM",
  "references": [
    "GUID:9d6c7830d9ab746a7a12f144bdeeee7c",
    "GUID:e6ab36a850314744fbf0610d0ee34639",
    "GUID:5540e30183c82e84b954c033c388e06c",
    "GUID:1491147abf9948f42a7ffca8e7f77123"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

**GUID Mapping** (verify before applying):
- `9d6c7830d9ab746a7a12f144bdeeee7c` → `GameCreator.Runtime.Core`
- `e6ab36a850314744fbf0610d0ee34639` → `GameCreator.Multiplayer.Runtime`
- `5540e30183c82e84b954c033c388e06c` → `undream.llmunity.Runtime`
- `1491147abf9948f42a7ffca8e7f77123` → `Unity.Netcode.Runtime`

**Commands to Find GUIDs**:
```bash
# Find GameCreator.Runtime.Core GUID
grep -r "GameCreator.Runtime.Core" Assets/Plugins/GameCreator --include="*.asmdef" -A 1 | grep "GUID"

# Find GameCreator.Multiplayer.Runtime GUID
grep -r "GameCreator.Multiplayer.Runtime" Assets/Plugins/GameCreator --include="*.asmdef" -A 1 | grep "GUID"

# Find undream.llmunity.Runtime GUID
grep -r "undream.llmunity.Runtime" Assets/LLMUnity --include="*.asmdef" -A 1 | grep "GUID"

# Find Unity.Netcode.Runtime GUID
grep -r "Unity.Netcode.Runtime" Library/PackageCache --include="*.asmdef" -A 1 | grep "GUID"
```

**Validation**: Unity compiles assembly definition without errors

---

### 3. Checkpoint: Assembly Compilation
**Status**: ⏳ Pending
**Action**: Open Unity Editor, let it compile
**Expected**: No errors related to `GameCreator.Multiplayer.Runtime.LLM` assembly
**On Failure**: Fix GUID references in asmdef file

---

## Phase 2: File Migration (Medium Risk)

### 4. Move Core Files
**Status**: ⏳ Pending
**From**: `Assets/Scripts/MLCreator/Runtime/LLM/`
**To**: `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/Core/`

**Files to Move**:
- `LLMNetworkCharacter.cs` → `Core/LLMNetworkCharacter.cs`
- `LLMService.cs` → `Core/LLMService.cs`

**Method**: Use Unity Editor (drag & drop in Project window)
**Validation**: Files appear in new location with `.meta` files intact

---

### 5. Move Instruction Files
**Status**: ⏳ Pending
**From**: `Assets/Scripts/MLCreator/Runtime/LLM/Instructions/`
**To**: `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/Instructions/`

**Files to Move**:
- `InstructionLLMChat.cs`
- `InstructionLLMStop.cs`

**Method**: Use Unity Editor
**Validation**: Files appear in new location with `.meta` files intact

---

### 6. Move Event Files
**Status**: ⏳ Pending
**From**: `Assets/Scripts/MLCreator/Runtime/LLM/Events/`
**To**: `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/Events/`

**Files to Move**:
- `EventLLMReply.cs`

**Method**: Use Unity Editor
**Validation**: Files appear in new location with `.meta` files intact

---

### 7. Checkpoint: File Move Verification
**Status**: ⏳ Pending
**Actions**:
1. Verify all files moved successfully
2. Verify `.meta` files preserved (GUIDs intact)
3. Check Unity Console for missing script warnings
**On Failure**: Restore from Git, retry move operation

---

## Phase 3: Namespace Updates (Medium Risk)

### 8. Update LLMNetworkCharacter.cs Namespace
**Status**: ⏳ Pending
**File**: `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/Core/LLMNetworkCharacter.cs`

**Change**:
```csharp
// OLD
namespace MLCreator.Runtime.LLM

// NEW
namespace GameCreator.Multiplayer.Runtime.LLM
```

**Validation**: File compiles without errors

---

### 9. Update LLMService.cs Namespace
**Status**: ⏳ Pending
**File**: `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/Core/LLMService.cs`

**Change**:
```csharp
// OLD
namespace MLCreator.Runtime.LLM

// NEW
namespace GameCreator.Multiplayer.Runtime.LLM
```

**Validation**: File compiles without errors

---

### 10. Update InstructionLLMChat.cs Namespace
**Status**: ⏳ Pending
**File**: `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/Instructions/InstructionLLMChat.cs`

**Change**:
```csharp
// OLD
namespace MLCreator.Runtime.LLM

// NEW
namespace GameCreator.Multiplayer.Runtime.LLM
```

**Validation**: File compiles without errors

---

### 11. Update InstructionLLMStop.cs Namespace
**Status**: ⏳ Pending
**File**: `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/Instructions/InstructionLLMStop.cs`

**Change**:
```csharp
// OLD
namespace MLCreator.Runtime.LLM

// NEW
namespace GameCreator.Multiplayer.Runtime.LLM
```

**Validation**: File compiles without errors

---

### 12. Update EventLLMReply.cs Namespace
**Status**: ⏳ Pending
**File**: `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/Events/EventLLMReply.cs`

**Change**:
```csharp
// OLD
namespace MLCreator.Runtime.LLM

// NEW
namespace GameCreator.Multiplayer.Runtime.LLM
```

**Validation**: File compiles without errors

---

### 13. Checkpoint: Namespace Consistency
**Status**: ⏳ Pending
**Validation**:
```bash
# All files should show new namespace
grep -r "namespace" "Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM" --include="*.cs"
# Expected: All show "GameCreator.Multiplayer.Runtime.LLM"
```

---

## Phase 4: Reference Updates & Testing (Medium Risk)

### 14. Search for Old Namespace References
**Status**: ⏳ Pending
**Command**:
```bash
grep -r "MLCreator.Runtime.LLM" Assets --include="*.cs"
grep -r "using MLCreator.Runtime.LLM" Assets --include="*.cs"
```

**Expected**: No results (Unity should auto-update via GUIDs)
**If Found**: Manually update each reference to `GameCreator.Multiplayer.Runtime.LLM`

---

### 15. Verify Visual Scripting Components
**Status**: ⏳ Pending
**Actions**:
1. Open Unity Editor
2. Create new Game Creator visual script (Trigger component)
3. Add Action: Check for "LLM Chat" under Game Creator menu
4. Add Action: Check for "LLM Stop" under Game Creator menu
5. Add Event: Check for "On LLM Reply" under Game Creator menu

**Validation**: All components appear and can be added to visual scripts

---

### 16. Test Property Getters
**Status**: ⏳ Pending
**Actions**:
1. Open `InstructionLLMChat` in visual script
2. Verify `GetGameObjectPlayer` property getter works
3. Verify no compilation errors about missing types

**Validation**: Property getters resolve correctly, no "type not found" errors

---

### 17. Full Project Compilation
**Status**: ⏳ Pending
**Action**:
1. Close Unity Editor
2. Delete `Library/` folder (force full recompile)
3. Reopen Unity Editor
4. Wait for full compilation

**Validation**:
- Zero compilation errors
- Zero missing script warnings
- All LLM components functional

---

## Phase 5: Cleanup (Low Risk)

### 18. Remove Old LLM Directory
**Status**: ⏳ Pending
**Directory**: `Assets/Scripts/MLCreator/Runtime/LLM/`
**Action**: Delete directory in Unity Editor (moves to trash)
**Validation**: Directory no longer exists

---

### 19. Clean Empty Parent Directories
**Status**: ⏳ Pending
**Check and Remove if Empty**:
- `Assets/Scripts/MLCreator/Runtime/` (if only had LLM/)
- `Assets/Scripts/MLCreator/` (if now empty)
- `Assets/Scripts/` (if now empty)

**Validation**: No empty directories remain

---

### 20. Update Documentation References
**Status**: ⏳ Pending

**Files to Update**:

**a) LLMUnity_Integration_Architecture.md**
- Location: `docs/_knowledge-base/04-architecture/LLMUnity_Integration_Architecture.md`
- Changes:
  - Update file paths to new location
  - Update namespace from `MLCreator.Runtime.LLM` to `GameCreator.Multiplayer.Runtime.LLM`
  - Add section about assembly definition

**b) AI_MANDATORY_CHECKLIST.md** (if references LLM)
- Location: `docs/_knowledge-base/01-critical/AI_MANDATORY_CHECKLIST.md`
- Changes: Update any LLM location references

**c) Project README** (if applicable)
- Update directory structure diagrams
- Update namespace examples

---

### 21. Update Related OpenSpec Proposals
**Status**: ⏳ Pending

**Proposals to Update**:

**a) fix-llmunity-gamecreator-integration**
- File: `openspec/changes/fix-llmunity-gamecreator-integration/proposal.md`
- Add note: "Files relocated to GameCreator_Multiplayer/Runtime/LLM/ as part of strategic structure optimization"

**b) integrate-llmunity-workflow**
- File: `openspec/changes/integrate-llmunity-workflow/proposal.md`
- Add note about final location decision

---

### 22. Checkpoint: Final Validation
**Status**: ⏳ Pending

**Comprehensive Checks**:
1. ✅ Project compiles without errors
2. ✅ No orphaned files at old location
3. ✅ Visual scripting components functional
4. ✅ Namespace consistency verified
5. ✅ Assembly reference graph correct
6. ✅ Documentation updated
7. ✅ Git status shows clean migration

---

## Phase 6: Archival & Completion

### 23. Create Migration Report
**Status**: ⏳ Pending
**Location**: `claudedocs/reports/LLMUNITY_STRUCTURE_OPTIMIZATION_COMPLETE.md`

**Content Should Include**:
- Summary of migration
- File moves performed
- Namespace changes
- Compilation status
- Testing results
- Documentation updates
- Next steps (if any)

---

### 24. Git Commit
**Status**: ⏳ Pending
**Commit Message**:
```
refactor: Optimize LLMUnity integration to follow Game Creator strategic structure

- Moved LLM integration from Assets/Scripts/MLCreator/ to GameCreator_Multiplayer/Runtime/LLM/
- Created GameCreator.Multiplayer.Runtime.LLM assembly with proper references
- Updated all namespaces from MLCreator.Runtime.LLM to GameCreator.Multiplayer.Runtime.LLM
- Removed parallel structure that fragmented assembly organization
- Updated documentation to reflect new structure

OpenSpec: optimize-llmunity-strategic-structure
Assemblies: GameCreator.Multiplayer.Runtime.LLM (new)
Breaking: Namespace change (Unity auto-migrates references)
```

---

### 25. OpenSpec Validation & Archive
**Status**: ⏳ Pending

**Actions**:
```bash
# Validate proposal
openspec validate optimize-llmunity-strategic-structure --strict

# After successful implementation and testing:
openspec archive optimize-llmunity-strategic-structure
```

**Validation**: Proposal passes strict validation, archives successfully

---

## Rollback Plan

### If Issues in Phase 2-3
**Action**:
```bash
git checkout -- Assets/
# Restores all files to pre-migration state
```

### If Issues in Phase 4
**Action**:
1. Keep new structure
2. Fix specific issues found in testing
3. Do NOT rollback - files are already migrated

### If Critical Blocking Issues
**Action**:
1. Revert entire commit
2. Investigate root cause
3. Plan alternative approach or fixes
4. Retry migration

---

## Success Criteria Checklist

**Technical**:
- [ ] All files at `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/`
- [ ] Assembly definition exists with correct references
- [ ] All files use `GameCreator.Multiplayer.Runtime.LLM` namespace
- [ ] Project compiles without errors
- [ ] No orphaned files at `Assets/Scripts/MLCreator/Runtime/LLM/`

**Functional**:
- [ ] Visual scripting Instructions appear in Game Creator menus
- [ ] Visual scripting Events appear in Game Creator menus
- [ ] Property getters work (e.g., `GetGameObjectPlayer`)
- [ ] No runtime errors when using LLM components

**Documentation**:
- [ ] Architecture documentation updated
- [ ] Related OpenSpec proposals updated
- [ ] Migration report created

**Process**:
- [ ] Changes committed to Git
- [ ] OpenSpec proposal validated
- [ ] Proposal archived after successful deployment

---

## Estimated Timeline

| Phase | Tasks | Time | Risk |
|-------|-------|------|------|
| Phase 1: Preparation | 1-3 | 30 min | Low |
| Phase 2: File Migration | 4-7 | 20 min | Medium |
| Phase 3: Namespace Updates | 8-13 | 30 min | Medium |
| Phase 4: Testing | 14-17 | 40 min | Medium |
| Phase 5: Cleanup | 18-22 | 30 min | Low |
| Phase 6: Archival | 23-25 | 20 min | Low |
| **Total** | **25 tasks** | **~2.5 hours** | **Medium** |

---

## Dependencies

**Blocked By**: None (can start immediately)

**Blocks**:
- Any future LLM module development should use new structure
- Documentation that references old structure needs updates

**Related Changes**:
- `fix-llmunity-gamecreator-integration` - Compilation fixes (completed)
- `integrate-llmunity-workflow` - Initial integration (mostly complete)

---

## Notes

**IMPORTANT**: Use Unity Editor for all file operations (moves/deletes) to preserve GUIDs and references.

**Testing Priority**: Focus on visual scripting functionality - this is the primary user-facing interface.

**Performance**: Expect ~10-30 second improvement in compilation time for LLM-only changes after migration.

**Reversibility**: All changes are reversible via Git. No destructive operations until Phase 5 cleanup.
