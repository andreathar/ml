# Change: Optimize LLMUnity Strategic Structure

## Why

The LLMUnity integration is currently located outside the strategic Game Creator Multiplayer architecture, creating **parallel structures** that break assembly namespace integrity:

**Current Problems:**
1. **Location**: LLM integration at `Assets/Scripts/MLCreator/Runtime/LLM/` is outside the strategic structure
2. **Missing Assembly**: No assembly definition exists, causing compilation/reference issues
3. **Parallel Structure**: Creates separate code path instead of unified Game Creator integration
4. **Namespace Fragmentation**: Not following `GameCreator.Multiplayer.Runtime.*` pattern
5. **Maintenance Risk**: Isolated structure makes updates and integration harder

**Strategic Architecture Violation:**
The project follows an **invasive integration pattern** with Game Creator (documented in `.serena/memories/CRITICAL/001_gamecreator_invasive_integration.md`). All multiplayer modules should be unified under `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/` to maintain:
- Assembly namespace consistency (`GameCreator.Multiplayer.Runtime.*`)
- Unified dependency management
- Cohesive module architecture
- Easier cross-module integration

## What Changes

### 1. Relocate LLM Integration
Move from `Assets/Scripts/MLCreator/Runtime/LLM/` to proper location:
```
Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/
├── GameCreator.Multiplayer.Runtime.LLM.asmdef (NEW)
├── LLMNetworkCharacter.cs
├── LLMService.cs
├── Instructions/
│   ├── InstructionLLMChat.cs
│   └── InstructionLLMStop.cs
└── Events/
    └── EventLLMReply.cs
```

### 2. Create Proper Assembly Definition
**Assembly Name**: `GameCreator.Multiplayer.Runtime.LLM`
**References**:
- `GameCreator.Runtime.Core` (Game Creator Core)
- `GameCreator.Multiplayer.Runtime` (Multiplayer base)
- `undream.llmunity.Runtime` (LLMUnity plugin)
- `Unity.Netcode.Runtime` (Unity Netcode)

### 3. Update Namespace
Change from: `MLCreator.Runtime.LLM`
Change to: `GameCreator.Multiplayer.Runtime.LLM`

### 4. Clean Up Old Structure
Remove `Assets/Scripts/MLCreator/Runtime/LLM/` directory after migration

## Impact

### Affected Files
- **Move**: All files from `Assets/Scripts/MLCreator/Runtime/LLM/` → `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/`
- **Create**: `GameCreator.Multiplayer.Runtime.LLM.asmdef`
- **Update**: All moved files - change namespace declarations
- **Delete**: `Assets/Scripts/MLCreator/Runtime/LLM/` (empty directory)
- **Update**: Any references to old namespace in project

### Breaking Changes
**Namespace Change**: Code referencing `MLCreator.Runtime.LLM` must update to `GameCreator.Multiplayer.Runtime.LLM`
- Visual scripting references (should auto-update via Unity)
- Any custom scripts referencing LLM classes
- Documentation and comments

**Mitigation**: Unity's refactoring should handle most references automatically when files are moved.

### Dependencies
- **LLMUnity Plugin**: `Assets/LLMUnity/` (unchanged)
- **Game Creator Core**: `Assets/Plugins/GameCreator/Packages/Core/`
- **Game Creator Multiplayer**: `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/`
- **Unity Netcode**: Package dependency

## Success Criteria

- [ ] LLM integration located at `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/`
- [ ] Assembly definition `GameCreator.Multiplayer.Runtime.LLM.asmdef` created with proper references
- [ ] All files use namespace `GameCreator.Multiplayer.Runtime.LLM`
- [ ] Project compiles without errors
- [ ] Visual scripting components (Instructions/Events) appear in Game Creator menus
- [ ] No orphaned files remain at `Assets/Scripts/MLCreator/Runtime/LLM/`
- [ ] Assembly reference validation passes
- [ ] Follows established Game Creator Multiplayer module pattern

## Technical Context

### Strategic Architecture Pattern
Game Creator Multiplayer uses a **modular assembly structure**:
- Base: `GameCreator.Multiplayer.Runtime`
- Sub-modules: `GameCreator.Multiplayer.Runtime.{Module}`
- Examples: `Behavior`, `CityVehicles`, `InputSystemExamples`

LLM integration should follow this pattern as `GameCreator.Multiplayer.Runtime.LLM`.

### Invasive Integration Principle
All multiplayer features integrate **within** Game Creator structure, not parallel to it. This ensures:
- Unified API surface
- Consistent namespace organization
- Easier cross-module dependencies
- Simplified maintenance and updates

### Assembly Reference Pattern
Looking at existing modules (e.g., `Behavior`):
```json
{
  "name": "GameCreator.Multiplayer.Runtime.Behavior",
  "references": [
    "GameCreator.Runtime.Core",
    "GameCreator.Runtime.Behavior",
    "GameCreator.Multiplayer.Runtime",
    "Unity.Netcode.Runtime"
  ]
}
```

LLM module follows same pattern, replacing `GameCreator.Runtime.Behavior` with `undream.llmunity.Runtime`.

## References

- **Invasive Integration**: `.serena/memories/CRITICAL/001_gamecreator_invasive_integration.md`
- **Network Architecture**: `.serena/memories/CRITICAL/002_network_architecture_never_forget.md`
- **LLM Integration Architecture**: `docs/_knowledge-base/04-architecture/LLMUnity_Integration_Architecture.md`
- **Existing Changes**:
  - `openspec/changes/fix-llmunity-gamecreator-integration/` (compilation fixes)
  - `openspec/changes/integrate-llmunity-workflow/` (initial integration)
