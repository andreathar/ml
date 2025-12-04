# Change: Refactor Project Structure and Namespace Organization

## Why

The project has grown organically without consistent folder naming, namespace organization, or coding patterns. This creates confusion, makes navigation difficult, and hinders development efficiency. Currently:
- Only 2 custom C# files exist outside GameCreator plugins
- No namespace organization for custom code
- No assembly definition files for custom code isolation
- Inconsistent component placement (Editor code mixed, Runtime code without namespace)

## What Changes

### Folder Structure Reorganization
- **BREAKING**: Move custom code to proper directory structure under `Assets/MLCreator/`
- Establish clear Runtime/Editor separation following GameCreator patterns
- Create feature-based subdirectories: Core, Gameplay, Multiplayer, AI, UI
- Add `.asmdef` assembly definitions for each module

### Namespace Standardization
- All custom code uses `MLCreator.{Module}.{Feature}` pattern
- Clear namespace-to-folder mapping for discoverability
- Consistent naming conventions enforced via EditorConfig

### Claude Skills Integration
- Create namespace-specific Claude skills for AI-assisted development
- Skills provide context and patterns for each module
- Enables efficient code generation following project conventions

## Impact

### Affected Code
- `Assets/Scripts/ProximityColorChanger.cs` → `Assets/MLCreator/Runtime/Gameplay/Effects/ProximityColorChanger.cs`
- `Assets/Editor/HotReloadHandler.cs` → `Assets/MLCreator/Editor/Tools/HotReloadHandler.cs`
- New assembly definitions for all modules

### Affected Specs
- `folder-structure` (new capability)

### Breaking Changes
- Script references may need updating in scenes/prefabs
- Assembly references will change
- Namespace imports in any dependent code

## Current State Analysis

### Directory Findings
```
Assets/
├── Editor/                    # 1 file (HotReloadHandler.cs) - No namespace
├── Scripts/                   # 1 file (ProximityColorChanger.cs) - No namespace
├── Plugins/GameCreator/       # 2,906 files - Well-organized vendor code
├── Resources/                 # 2 files - Under-utilized
├── Scenes/                    # 1 scene - No organization
└── Settings/                  # Unity settings
```

### GameCreator Namespace Patterns (Reference)
| Namespace | Purpose | File Count |
|-----------|---------|------------|
| `GameCreator.Runtime.Common` | Core utilities | 1,066 |
| `GameCreator.Runtime.VisualScripting` | Visual scripting | 541 |
| `GameCreator.Runtime.Characters` | Character system | 225 |
| `GameCreator.Runtime.Variables` | Variable system | 177 |
| `GameCreator.Runtime.Perception` | NPC perception | 101 |
| `GameCreator.Editor.*` | Editor tooling | 400+ |

### Naming Conventions Found
- Classes: `T<Type>` prefix for templates, `<Feature>Drawer` for drawers
- Instructions: `Instruction<Command>` for visual scripting
- Events: `Event<Trigger>` for event handlers
- Rigs: `Rig<Feature>` for IK components

## Proposed Structure

```
Assets/MLCreator/
├── Runtime/
│   ├── Core/               # Core systems, managers, initialization
│   ├── Gameplay/           # Game mechanics, effects, interactions
│   │   └── Effects/        # Visual effects like ProximityColorChanger
│   ├── Multiplayer/        # Network synchronization, RPCs
│   ├── AI/                 # AI behaviors, decision systems
│   └── UI/                 # User interface components
├── Editor/
│   ├── Core/               # Core editor utilities
│   ├── Tools/              # Development tools like HotReloadHandler
│   └── Inspectors/         # Custom inspectors and drawers
├── MLCreator.Runtime.asmdef
└── MLCreator.Editor.asmdef
```

## Success Criteria

1. All custom code has proper namespace (`MLCreator.*`)
2. Clear Runtime/Editor separation enforced by assembly definitions
3. Folder structure matches namespace hierarchy
4. Claude skills available for each major namespace
5. Zero compilation errors after migration
6. All scene/prefab references updated correctly
