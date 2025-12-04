# Tasks: Refactor Project Structure

## 1. Directory Structure Setup
- [ ] 1.1 Create `Assets/MLCreator/Runtime/Core/` directory
- [ ] 1.2 Create `Assets/MLCreator/Runtime/Gameplay/Effects/` directory
- [ ] 1.3 Create `Assets/MLCreator/Runtime/Multiplayer/` directory (placeholder)
- [ ] 1.4 Create `Assets/MLCreator/Runtime/AI/` directory (placeholder)
- [ ] 1.5 Create `Assets/MLCreator/Runtime/UI/` directory (placeholder)
- [ ] 1.6 Create `Assets/MLCreator/Editor/Core/` directory
- [ ] 1.7 Create `Assets/MLCreator/Editor/Tools/` directory
- [ ] 1.8 Create `Assets/MLCreator/Editor/Inspectors/` directory (placeholder)

## 2. Assembly Definition Files
- [ ] 2.1 Create `MLCreator.Runtime.asmdef` with Unity and GameCreator references
- [ ] 2.2 Create `MLCreator.Editor.asmdef` with Editor-only platform and runtime reference

## 3. Code Migration
- [ ] 3.1 Move `Assets/Scripts/ProximityColorChanger.cs` to `Assets/MLCreator/Runtime/Gameplay/Effects/`
- [ ] 3.2 Add `namespace MLCreator.Runtime.Gameplay.Effects` to ProximityColorChanger
- [ ] 3.3 Move `Assets/Editor/HotReloadHandler.cs` to `Assets/MLCreator/Editor/Tools/`
- [ ] 3.4 Add `namespace MLCreator.Editor.Tools` to HotReloadHandler
- [ ] 3.5 Remove empty `Assets/Scripts/` directory
- [ ] 3.6 Remove empty `Assets/Editor/` directory

## 4. Claude Skills Creation
- [ ] 4.1 Create `.claude/commands/mlcreator-core.md` skill
- [ ] 4.2 Create `.claude/commands/mlcreator-gameplay.md` skill
- [ ] 4.3 Create `.claude/commands/mlcreator-multiplayer.md` skill
- [ ] 4.4 Create `.claude/commands/mlcreator-ai.md` skill
- [ ] 4.5 Create `.claude/commands/mlcreator-ui.md` skill
- [ ] 4.6 Create `.claude/commands/mlcreator-editor.md` skill
- [ ] 4.7 Create `.claude/commands/gamecreator-common.md` skill
- [ ] 4.8 Create `.claude/commands/gamecreator-characters.md` skill
- [ ] 4.9 Create `.claude/commands/gamecreator-perception.md` skill
- [ ] 4.10 Create `.claude/commands/gamecreator-visualscripting.md` skill

## 5. Documentation
- [ ] 5.1 Create `Assets/MLCreator/README.md` with structure overview
- [ ] 5.2 Add placeholder READMEs in empty feature directories
- [ ] 5.3 Update CLAUDE.md with new skill commands

## 6. Validation
- [ ] 6.1 Verify Unity compilation succeeds with no errors
- [ ] 6.2 Verify all scene/prefab references are intact
- [ ] 6.3 Test HotReloadHandler functionality in Editor
- [ ] 6.4 Test ProximityColorChanger in Play mode
- [ ] 6.5 Run `openspec validate refactor-project-structure --strict`

## 7. Cleanup
- [ ] 7.1 Delete any leftover empty directories
- [ ] 7.2 Verify no duplicate .meta files remain
- [ ] 7.3 Commit all changes with descriptive message
