# Change: Automatic Local Player Parameter Injection

## Why

In multiplayer GameCreator projects, hundreds of triggers, visual scripting components, and conditions require manual configuration of "From Character" fields to "Local Player". This creates:

1. **Repetitive Manual Work**: Every trigger (chest interactions, reload actions, faction checks, etc.) needs manual dropdown selection
2. **Error-Prone Configuration**: Easy to forget setting "Local Player", causing multiplayer bugs
3. **Scene Consolidation Blocker**: Moving from multiple scenes to one main scene risks losing all these manual configurations
4. **Prefab Conversion Risk**: Converting scene GameObjects to prefabs may lose trigger/event configurations

The project already has `GetLocalPlayer` property getter and `LocalPlayerResolver` infrastructure, but no automatic injection system.

## What Changes

Create an **Automatic Local Player Injection System** that:

1. **Auto-detects** character property fields in GameCreator components (triggers, conditions, instructions)
2. **Auto-injects** `GetLocalPlayer` property when appropriate (multiplayer context)
3. **Provides migration tool** to convert existing manual "Local Player" selections to use `GetLocalPlayer`
4. **Supports prefab extraction** by preserving configurations during scene-to-prefab conversion
5. **Editor helpers** for bulk operations and validation

### Key Components

- **`LocalPlayerAutoInjector`** - Editor utility that scans components and injects `GetLocalPlayer` references
- **`LocalPlayerInjectionRule`** - Defines which components/fields should auto-inject
- **Scene Migration Tool** - Converts existing manual configurations before scene consolidation
- **Prefab Extraction Helper** - Preserves trigger/event configurations during prefabification
- **Validation Tool** - Scans project for missing local player references

## Impact

**Affected Specs**:
- `visual-scripting-integration` (new capability) - Auto-injection for triggers/conditions/instructions
- `player-management` (modify) - Extend LocalPlayerResolver with injection registry
- `editor-tooling` (modify) - Add bulk migration and validation tools

**Affected Code**:
- `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Editor/Tools/` - New injection utilities
- `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/Targeting/LocalPlayerResolver.cs` - Registry support
- Scene assets with triggers/events - Migration to auto-injection system

**Breaking Changes**: None - Additive feature, backward compatible

**Migration Path**:
1. Run migration tool on existing scenes to convert manual configs
2. Extract scene GameObjects to prefabs with preserved configurations
3. Delete old scenes, keeping only main multiplayer scene
4. Future triggers/events automatically use `GetLocalPlayer`

## User Workflow After Implementation

**Before** (Manual):
1. Create trigger in scene
2. Open GameCreator Finder
3. Find trigger in list
4. Expand "From Character" dropdown
5. Select "Local Player"
6. Repeat for EVERY trigger, condition, instruction

**After** (Automatic):
1. Create trigger in scene
2. ✅ Done - "Local Player" automatically injected if multiplayer context detected

**Scene Consolidation Workflow**:
1. Run `Tools > MLCreator > Migrate Local Player References` on all scenes
2. Select GameObjects with triggers/events
3. Run `Tools > MLCreator > Extract to Prefabs (Preserve Configs)`
4. Delete old scenes
5. Instantiate prefabs in main multiplayer scene
6. All configurations preserved automatically
