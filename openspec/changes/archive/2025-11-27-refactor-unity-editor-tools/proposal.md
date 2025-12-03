# Change: Refactor Unity Editor Tools - Clean, Organize, and Consolidate

## Why

The `Assets/Editor/` directory contains 22 custom editor scripts accumulated over time with varying code quality, inconsistent organization, and potential dead code. A systematic review and cleanup will:

1. **Reduce maintenance burden** - Remove unused/obsolete tools
2. **Improve discoverability** - Consistent menu paths and naming
3. **Ensure code quality** - Apply consistent patterns and standards
4. **Reduce compile time** - Fewer scripts to compile
5. **Improve reliability** - Remove deprecated or broken functionality

## What Changes

### Phase 1: Audit & Classification
- Inventory all 22 editor scripts with usage analysis
- Classify each as: KEEP, CONSOLIDATE, DEPRECATE, or REMOVE
- Document dependencies between scripts

### Phase 2: Consolidation
- Merge related tools into logical groups:
  - **Multiplayer Setup Tools** → Single consolidated editor
  - **Foam/Documentation Tools** → Unified documentation tooling
  - **Build/Optimization Tools** → Combined build pipeline
  - **Project Utilities** → Streamlined utility collection

### Phase 3: Code Quality
- Apply consistent namespace: `MLCreator.Editor.*`
- Standardize menu paths via `EditorMenuPaths.cs`
- Remove dead code and commented sections
- Add proper XML documentation
- Ensure consistent error handling

### Phase 4: Cleanup
- Remove deprecated scripts entirely
- Archive removed code in `_archive/` for reference
- Update any documentation references

## Current Editor Scripts (22 files)

| Script | Lines | Status | Recommendation |
|--------|-------|--------|----------------|
| `EditorMenuPaths.cs` | 199 | Good | KEEP - Central menu path management |
| `SceneSpawnSetup.cs` | 337 | Good | KEEP - Active multiplayer tool |
| `SetupMultiplayerForScene.cs` | ~200 | Duplicate? | CONSOLIDATE with SceneSpawnSetup |
| `FoamBatchTagger.cs` | 1404 | Complex | REVIEW - Large, may need splitting |
| `TriggerAnalyzerAndRenamer.cs` | 416 | Good | KEEP - Useful trigger tool |
| `ExtendL0Floor.cs` | 248 | Scene-specific | DEPRECATE - One-time use tool |
| `PerformanceSettings.cs` | 160 | Good | KEEP - Runtime settings |
| `HotReloadHandler.cs` | 1017 | Complex | KEEP - VS Code integration |
| `BuildOptimizationConfigurator.cs` | ~150 | Good | CONSOLIDATE with BuildSizeOptimizer |
| `BuildSizeOptimizer.cs` | ~200 | Good | CONSOLIDATE into build tools |
| `BatchNetworkSceneProcessor.cs` | ~300 | Good | KEEP - Batch processing |
| `ScriptExecutionOrderSetup.cs` | ~100 | Good | KEEP - Critical setup |
| `SupabaseVariablesSetup.cs` | ~150 | Backend | KEEP - Backend integration |
| `CodeAssistant/NamespaceReferenceManager.cs` | ~200 | Good | KEEP - Code organization |
| `CodeAssistant/SchemaAutoUpdater.cs` | ~150 | Good | KEEP - Schema management |
| `BatchProcessing/OvernightTaskProcessor.cs` | ~300 | Good | KEEP - Overnight tasks |
| `Tools/IconGenerator/*.cs` (4 files) | ~600 | Good | KEEP - Icon generation suite |
| `Tools/AssetOptimizer/AssetOptimizer.cs` | ~200 | Good | KEEP - Asset optimization |
| `Tools/Buildings_Creator/MaterialStructureCreator.cs` | ~150 | Specific | REVIEW - Limited use |

## Impact

### Affected Specs
- New capability: `editor-tooling` (to be created)

### Affected Code
- `Assets/Editor/` - All 22 files reviewed
- `Assets/Editor/Tools/` - Subdirectory organization
- Menu paths throughout project

### Breaking Changes
- **Potential**: Menu path changes for consolidated tools
- **Mitigation**: Maintain backward-compatible menu items during transition

### Risk Assessment
- **Low risk**: Read-only audit phase first
- **Medium risk**: Consolidation may break cross-references
- **Mitigation**: Comprehensive testing after each phase

## Success Criteria

1. **Reduction**: At least 20% fewer editor scripts through consolidation
2. **Quality**: All remaining scripts follow coding standards
3. **Documentation**: Each tool has clear XML documentation
4. **Organization**: Consistent menu structure via `EditorMenuPaths.cs`
5. **No regressions**: All existing functionality preserved or improved
