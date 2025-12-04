# Design: Project Structure Refactoring

## Context

The MLCreator project is built on GameCreator, a well-organized Unity framework with clear patterns. However, custom code has been added without following these patterns, creating organizational debt. This design document establishes the technical approach for restructuring.

## Goals / Non-Goals

### Goals
- Establish consistent folder structure matching GameCreator patterns
- Create namespace hierarchy that maps 1:1 with folder structure
- Enable better discoverability and navigation
- Support AI-assisted development via Claude skills
- Maintain compatibility with Unity 6 and GameCreator

### Non-Goals
- Modify GameCreator plugin code (vendor managed)
- Change existing GameCreator namespace patterns
- Create unnecessary abstractions for minimal code
- Over-engineer for hypothetical future needs

## Decisions

### Decision 1: Namespace Pattern
**Choice**: `MLCreator.{Context}.{Module}.{Feature}`
- `MLCreator.Runtime.Core` - Core runtime systems
- `MLCreator.Runtime.Gameplay.Effects` - Gameplay effects
- `MLCreator.Editor.Tools` - Editor tooling

**Rationale**: Mirrors GameCreator pattern (`GameCreator.Runtime.*`, `GameCreator.Editor.*`) for consistency.

### Decision 2: Assembly Definition Strategy
**Choice**: Two primary assemblies with optional sub-assemblies

```
MLCreator.Runtime.asmdef
├── References: Unity.*, GameCreator.Runtime.*
├── Platforms: Any
└── Defines: MLCREATOR_RUNTIME

MLCreator.Editor.asmdef
├── References: MLCreator.Runtime, Unity.Editor.*, GameCreator.Editor.*
├── Platforms: Editor only
└── Defines: MLCREATOR_EDITOR
```

**Rationale**:
- Simple two-assembly split minimizes complexity for small codebase
- Can expand to per-module assemblies if codebase grows significantly
- Editor-only assembly prevents accidental runtime dependencies

### Decision 3: Folder Hierarchy
**Choice**: Feature-based organization with depth limit of 4 levels

```
Assets/MLCreator/{Runtime|Editor}/{Module}/{Feature}/{File}.cs
```

**Examples**:
- `Assets/MLCreator/Runtime/Gameplay/Effects/ProximityColorChanger.cs`
- `Assets/MLCreator/Editor/Tools/HotReloadHandler.cs`
- `Assets/MLCreator/Runtime/Core/Managers/MLCreatorManager.cs`

**Rationale**:
- 4-level max prevents deep nesting while maintaining organization
- Feature-based grouping keeps related code together
- Matches Unity and GameCreator conventions

### Decision 4: Naming Conventions
**Choice**: Aligned with GameCreator patterns

| Type | Pattern | Example |
|------|---------|---------|
| Manager Classes | `{Feature}Manager` | `NetworkManager` |
| MonoBehaviour | `{Feature}` or `{Feature}Handler` | `ProximityColorChanger` |
| Editor Windows | `{Feature}Window` or `{Feature}Handler` | `HotReloadHandler` |
| Property Drawers | `{Property}Drawer` | `NetworkPropertyDrawer` |
| ScriptableObjects | `{Feature}Asset` or `{Feature}Config` | `MLCreatorConfig` |
| Interfaces | `I{Capability}` | `INetworkSyncable` |
| Abstract Classes | `Base{Feature}` or `{Feature}Base` | `BaseNetworkComponent` |

### Decision 5: Claude Skills Architecture
**Choice**: One skill per major namespace/module

Skills provide:
1. Module context and purpose
2. Key classes and their relationships
3. Common patterns and usage examples
4. Integration points with GameCreator

**Location**: `.claude/commands/{module}.md`

## Alternatives Considered

### Alternative A: Minimal Restructure
Keep current structure, only add namespaces to existing files.

**Rejected because**:
- Doesn't solve folder organization issues
- Editor/Runtime separation not enforced
- Harder to scale as project grows

### Alternative B: Per-Feature Assemblies
Create separate assembly for each feature (Core, Gameplay, Multiplayer, etc.)

**Rejected because**:
- Overkill for current 2-file codebase
- Adds compilation overhead
- Can migrate to this later if needed

### Alternative C: Match Unity Packages Structure
Use `Packages/com.mlcreator.core/` pattern.

**Rejected because**:
- More complex setup for embedded development
- Better suited for distributed packages
- Assets folder is standard for project-specific code

## Risks / Trade-offs

### Risk 1: Scene Reference Breakage
**Risk**: Moving scripts breaks prefab/scene references
**Mitigation**:
- Unity maintains GUID-based references
- Document all moves and verify references post-migration
- Run full build test after migration

### Risk 2: Namespace Import Updates
**Risk**: Any code importing current files needs updates
**Mitigation**:
- Currently only 2 custom files with no cross-references
- Search for any external references before migration

### Risk 3: Learning Curve
**Risk**: Team must learn new organization
**Mitigation**:
- Structure mirrors familiar GameCreator patterns
- Claude skills provide inline documentation
- Clear README in MLCreator root

## Migration Plan

### Phase 1: Directory Setup
1. Create `Assets/MLCreator/Runtime/` hierarchy
2. Create `Assets/MLCreator/Editor/` hierarchy
3. Add `.asmdef` files

### Phase 2: Code Migration
1. Move `ProximityColorChanger.cs` → `Runtime/Gameplay/Effects/`
2. Move `HotReloadHandler.cs` → `Editor/Tools/`
3. Add namespace declarations to both files
4. Update any using statements

### Phase 3: Validation
1. Verify compilation succeeds
2. Check all scene/prefab references
3. Test HotReload functionality
4. Test ProximityColorChanger in play mode

### Phase 4: Skills Setup
1. Create `.claude/commands/` directory
2. Add skill files for each namespace
3. Test skill invocation

### Rollback Plan
If migration fails:
1. Revert file moves via git
2. Remove added assembly definitions
3. Restore original file locations

## Open Questions

1. Should we create stub directories for planned modules (AI, Multiplayer) now?
   - Recommendation: Yes, but only with a README placeholder

2. Should Claude skills include code generation templates?
   - Recommendation: Yes, include common boilerplate patterns

3. Should we add EditorConfig for style enforcement?
   - Recommendation: Yes, align with Unity/GameCreator conventions
