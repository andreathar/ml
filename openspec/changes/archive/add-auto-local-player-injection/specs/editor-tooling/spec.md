# Editor Tooling Spec (Delta)

## ADDED Requirements

### Requirement: Scene Migration Tool

The system SHALL provide an editor tool to migrate existing manual "Local Player" configurations to use `GetLocalPlayer` property.

#### Scenario: Migrate all triggers in scene

- **GIVEN** a scene with 50 triggers manually configured with "Local Player"
- **WHEN** the user runs `Tools > MLCreator > Migrate Local Player References`
- **THEN** the tool scans all trigger components in the scene
- **AND** converts manual "Local Player" selections to `GetLocalPlayer` property
- **AND** displays a summary showing 50 components migrated

#### Scenario: Selective migration with preview

- **GIVEN** a scene with mixed configurations
- **WHEN** the user opens the migration tool
- **THEN** the tool shows a preview list of components to migrate
- **AND** the user can deselect components to skip
- **AND** only selected components are migrated when confirmed

### Requirement: Prefab Extraction Tool

The system SHALL provide a tool to extract scene GameObjects to prefabs while preserving trigger/event configurations.

#### Scenario: Extract GameObject with triggers to prefab

- **GIVEN** a GameObject "Chest_Loot_Items" with trigger components in a scene
- **WHEN** the user selects the GameObject and runs `Tools > MLCreator > Extract to Prefabs (Preserve Configs)`
- **THEN** a new prefab is created in the project
- **AND** all trigger configurations are preserved in the prefab
- **AND** the original scene GameObject is replaced with prefab instance

#### Scenario: Batch extract multiple GameObjects

- **GIVEN** 100 GameObjects with triggers selected in the scene
- **WHEN** the user runs the batch extraction tool
- **THEN** the tool creates 100 prefabs preserving all configurations
- **AND** displays progress bar during extraction
- **AND** shows completion summary with created prefab paths

### Requirement: Validation Tool

The system SHALL provide a validation tool to scan the project for missing or incorrect local player references.

#### Scenario: Scan scene for missing references

- **GIVEN** a multiplayer scene with various trigger components
- **WHEN** the user runs `Tools > MLCreator > Validate Local Player References`
- **THEN** the tool scans all triggers, conditions, and instructions
- **AND** reports components with null or incorrect character references
- **AND** provides "Auto-Fix" button to inject `GetLocalPlayer` where needed

#### Scenario: Project-wide validation

- **GIVEN** a project with 10 scenes
- **WHEN** the user runs validation in project-wide mode
- **THEN** the tool scans all scenes for reference issues
- **AND** generates a report showing issues per scene
- **AND** allows batch fixing across all scenes

### Requirement: Injection Rule Editor

The system SHALL provide a visual editor for configuring auto-injection rules.

#### Scenario: Create new injection rule

- **GIVEN** the injection rule editor is open
- **WHEN** the user creates a new rule
- **THEN** the user can specify component type pattern (e.g., "Trigger*")
- **AND** can specify field name pattern (e.g., "*Character")
- **AND** can enable/disable the rule
- **AND** can set priority order for rule evaluation

#### Scenario: Test rule against scene

- **GIVEN** a configured injection rule
- **WHEN** the user clicks "Test Rule" button
- **THEN** the system shows which components in the current scene match the rule
- **AND** highlights the matched fields in the inspector
- **AND** displays count of components that would be affected

### Requirement: Bulk Operations UI

The system SHALL provide a unified UI for bulk migration, extraction, and validation operations.

#### Scenario: Unified operations window

- **GIVEN** the user opens `Tools > MLCreator > Local Player Automation`
- **WHEN** the window opens
- **THEN** the user sees tabs for "Migrate", "Extract", "Validate", and "Rules"
- **AND** each tab shows relevant options and preview
- **AND** progress is displayed for long-running operations

#### Scenario: Undo support for bulk operations

- **GIVEN** a migration operation has completed
- **WHEN** the user presses Ctrl+Z (Undo)
- **THEN** all changes made by the migration are reverted
- **AND** components return to their previous state
- **AND** undo history shows "Migrate Local Player References" entry
