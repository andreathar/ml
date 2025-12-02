# Implementation Tasks: Automatic Local Player Injection

## 1. Core Injection System

- [ ] 1.1 Create `LocalPlayerAutoInjector.cs` editor script
  - [ ] Implement component detection via reflection
  - [ ] Implement field pattern matching (e.g., "*Character")
  - [ ] Implement `GetLocalPlayer` property injection logic
  - [ ] Add multiplayer context detection (NetworkManager/LocalPlayerResolver)

- [ ] 1.2 Create `LocalPlayerInjectionRule.cs` configuration class
  - [ ] Define rule structure (component type, field name patterns, priority)
  - [ ] Implement rule matching logic
  - [ ] Add serialization support for ScriptableObject storage

- [ ] 1.3 Create `LocalPlayerInjectionRuleSet.cs` ScriptableObject
  - [ ] Default rules for GameCreator triggers
  - [ ] Default rules for GameCreator conditions
  - [ ] Default rules for GameCreator instructions
  - [ ] Rule enable/disable toggles

- [ ] 1.4 Extend `LocalPlayerResolver.cs` with injection registry
  - [ ] Add `Dictionary<int, InjectionRecord>` for tracking injected components
  - [ ] Implement `RegisterInjection(Component, timestamp)` method
  - [ ] Implement `IsAutoInjected(Component)` query method
  - [ ] Implement `GetInjectionTimestamp(Component)` query method
  - [ ] Add manual override detection and persistence

## 2. Editor Tools - Migration

- [ ] 2.1 Create `LocalPlayerMigrationTool.cs` editor window
  - [ ] Scene scanning for trigger/condition/instruction components
  - [ ] Detection of manual "Local Player" configurations
  - [ ] Preview UI showing components to be migrated
  - [ ] Selective migration with component checkboxes
  - [ ] Batch migration execution with progress bar
  - [ ] Undo/redo support via `Undo.RecordObject()`

- [ ] 2.2 Implement migration logic
  - [ ] Detect PropertyGetCharacter fields with "Local Player" value
  - [ ] Replace with `GetLocalPlayer.Create()` property instance
  - [ ] Mark components as migrated in registry
  - [ ] Generate migration report (count, paths, timestamps)

## 3. Editor Tools - Prefab Extraction

- [ ] 3.1 Create `PrefabExtractionTool.cs` editor utility
  - [ ] Single GameObject to prefab conversion
  - [ ] Batch selection to multiple prefabs
  - [ ] Configuration preservation verification
  - [ ] Prefab variant support (if parent prefab exists)
  - [ ] Auto-replace scene instance with prefab instance

- [ ] 3.2 Implement extraction workflow
  - [ ] Validate GameObject has saveable components
  - [ ] Create prefab at specified path (default: `Assets/Prefabs/Extracted/`)
  - [ ] Preserve all GameCreator trigger/event configurations
  - [ ] Replace original GameObject with prefab instance
  - [ ] Generate extraction report with prefab paths

## 4. Editor Tools - Validation

- [ ] 4.1 Create `LocalPlayerReferenceValidator.cs` editor tool
  - [ ] Scene-level validation scanning
  - [ ] Project-wide validation (all scenes)
  - [ ] Detection of null character references
  - [ ] Detection of incorrect reference types
  - [ ] Missing `GetLocalPlayer` usage detection in multiplayer scenes

- [ ] 4.2 Implement validation reporting
  - [ ] Issue categorization (critical, warning, info)
  - [ ] Component path display with hierarchy
  - [ ] "Auto-Fix" button for each issue
  - [ ] Batch auto-fix for all issues
  - [ ] Export validation report to CSV/JSON

## 5. Editor Tools - Rule Editor

- [ ] 5.1 Create `InjectionRuleEditorWindow.cs` custom editor
  - [ ] Visual rule list with add/remove/reorder
  - [ ] Rule property inspector (component type, field pattern, priority)
  - [ ] Enable/disable toggles per rule
  - [ ] "Test Rule" functionality showing matches in current scene

- [ ] 5.2 Implement rule testing
  - [ ] Apply rule to current scene components
  - [ ] Highlight matched components in hierarchy
  - [ ] Display matched field names in inspector
  - [ ] Show component count that would be affected

## 6. Editor Tools - Unified UI

- [ ] 6.1 Create `LocalPlayerAutomationWindow.cs` editor window
  - [ ] Tab-based UI (Migrate, Extract, Validate, Rules)
  - [ ] Integrate migration tool in "Migrate" tab
  - [ ] Integrate extraction tool in "Extract" tab
  - [ ] Integrate validator in "Validate" tab
  - [ ] Integrate rule editor in "Rules" tab

- [ ] 6.2 Add menu items
  - [ ] `Tools > MLCreator > Local Player Automation` - Opens unified window
  - [ ] `Tools > MLCreator > Migrate Local Player References` - Direct migration
  - [ ] `Tools > MLCreator > Extract to Prefabs (Preserve Configs)` - Direct extraction
  - [ ] `Tools > MLCreator > Validate Local Player References` - Direct validation

## 7. Runtime Hooks

- [ ] 7.1 Implement `LocalPlayerInjectionProcessor.cs` runtime component
  - [ ] `OnComponentAdded` callback for newly created triggers/conditions
  - [ ] Auto-injection on component initialization
  - [ ] Multiplayer context check before injection
  - [ ] Skip injection if component already configured

- [ ] 7.2 Add Editor callbacks
  - [ ] `[InitializeOnLoadMethod]` for editor startup injection
  - [ ] `EditorApplication.hierarchyChanged` callback for new GameObjects
  - [ ] Component addition detection via `ObjectFactory.componentWasAdded`
  - [ ] Scene loaded callback for initial injection pass

## 8. Testing

- [ ] 8.1 Create unit tests for injection logic
  - [ ] Test rule matching with various patterns
  - [ ] Test multiplayer context detection
  - [ ] Test `GetLocalPlayer` property creation
  - [ ] Test injection registry operations

- [ ] 8.2 Create integration tests
  - [ ] Test migration tool on sample scene
  - [ ] Test prefab extraction with trigger preservation
  - [ ] Test validation tool accuracy
  - [ ] Test auto-injection on component creation

- [ ] 8.3 Create scene-based tests
  - [ ] Sample scene with 50 triggers (pre-migration)
  - [ ] Sample scene with mixed configurations
  - [ ] Sample scene for extraction testing
  - [ ] Validation test scene with intentional issues

## 9. Documentation

- [ ] 9.1 Create user guide
  - [ ] Migration workflow documentation
  - [ ] Prefab extraction guide
  - [ ] Validation tool usage
  - [ ] Rule configuration examples
  - [ ] Troubleshooting common issues

- [ ] 9.2 Create API documentation
  - [ ] `LocalPlayerAutoInjector` public methods
  - [ ] `LocalPlayerResolver` injection registry API
  - [ ] `InjectionRule` configuration schema
  - [ ] Editor tool extension points

- [ ] 9.3 Update existing documentation
  - [ ] Add section to multiplayer setup guide
  - [ ] Update scene consolidation workflow
  - [ ] Add best practices for trigger configuration

## 10. Configuration & Defaults

- [ ] 10.1 Create default injection rule asset
  - [ ] Place in `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Editor/Rules/`
  - [ ] Include rules for common trigger types
  - [ ] Include rules for condition types
  - [ ] Include rules for instruction types

- [ ] 10.2 Create settings ScriptableObject
  - [ ] Enable/disable auto-injection globally
  - [ ] Set default prefab extraction path
  - [ ] Configure validation severity levels
  - [ ] Set injection logging verbosity

## 11. Polish & Quality

- [ ] 11.1 Add logging and debugging
  - [ ] Injection operation logs (when enabled)
  - [ ] Migration operation logs
  - [ ] Validation result logs
  - [ ] Error handling and user-friendly messages

- [ ] 11.2 Performance optimization
  - [ ] Cache component scans during migration
  - [ ] Lazy loading of injection rules
  - [ ] Async validation for large projects
  - [ ] Progress reporting for long operations

- [ ] 11.3 Edge case handling
  - [ ] Handle components with multiple character fields
  - [ ] Handle nested prefab instances
  - [ ] Handle prefab variants correctly
  - [ ] Handle scene unloading during operations
  - [ ] Handle null or destroyed component references

## 12. Deployment

- [ ] 12.1 Final testing
  - [ ] Test on sample project from scratch
  - [ ] Test migration on user's actual scenes
  - [ ] Test extraction tool with 100+ GameObjects
  - [ ] Verify undo/redo works correctly

- [ ] 12.2 Integration validation
  - [ ] Verify compatibility with existing GameCreator workflows
  - [ ] Test with Unity 2022.3, 2023.1, 2023.2
  - [ ] Test with GameCreator 2.x versions
  - [ ] Performance profiling on large scenes

- [ ] 12.3 Release preparation
  - [ ] Version bump in package.json
  - [ ] Update CHANGELOG.md
  - [ ] Create release notes
  - [ ] Tag commit for release
