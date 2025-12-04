# Folder Structure Specification

## ADDED Requirements

### Requirement: MLCreator Directory Hierarchy
The project SHALL organize all custom code under `Assets/MLCreator/` with clear Runtime/Editor separation.

#### Scenario: Creating new runtime code
- **WHEN** a developer creates new runtime code
- **THEN** the file MUST be placed under `Assets/MLCreator/Runtime/{Module}/{Feature}/`
- **AND** the namespace MUST match `MLCreator.Runtime.{Module}.{Feature}`

#### Scenario: Creating new editor code
- **WHEN** a developer creates new editor-only code
- **THEN** the file MUST be placed under `Assets/MLCreator/Editor/{Module}/`
- **AND** the namespace MUST match `MLCreator.Editor.{Module}`

### Requirement: Namespace-Folder Mapping
The project SHALL enforce 1:1 mapping between namespace hierarchy and folder structure.

#### Scenario: Namespace reflects folder path
- **WHEN** a file exists at `Assets/MLCreator/Runtime/Gameplay/Effects/MyEffect.cs`
- **THEN** the namespace MUST be `MLCreator.Runtime.Gameplay.Effects`

#### Scenario: Folder depth limit
- **WHEN** organizing code into folders
- **THEN** the depth SHALL NOT exceed 4 levels below `Assets/MLCreator/`

### Requirement: Assembly Definition Separation
The project SHALL use assembly definitions to enforce Runtime/Editor separation.

#### Scenario: Runtime assembly compilation
- **WHEN** the `MLCreator.Runtime` assembly compiles
- **THEN** it SHALL NOT reference any Editor assemblies
- **AND** it SHALL reference required GameCreator.Runtime assemblies

#### Scenario: Editor assembly compilation
- **WHEN** the `MLCreator.Editor` assembly compiles
- **THEN** it SHALL only compile for Editor platform
- **AND** it SHALL reference `MLCreator.Runtime` assembly

### Requirement: Module Directory Structure
The project SHALL maintain standardized module directories for code organization.

#### Scenario: Core module structure
- **WHEN** the Core module is accessed
- **THEN** it SHALL contain: Managers, Initialization, Utilities subdirectories

#### Scenario: Gameplay module structure
- **WHEN** the Gameplay module is accessed
- **THEN** it SHALL contain feature-based subdirectories (Effects, Interactions, etc.)

#### Scenario: Multiplayer module structure
- **WHEN** the Multiplayer module is accessed
- **THEN** it SHALL contain: Sync, RPC, NetworkBehaviours subdirectories

### Requirement: Naming Convention Enforcement
The project SHALL follow consistent naming patterns for all code artifacts.

#### Scenario: MonoBehaviour class naming
- **WHEN** creating a MonoBehaviour class
- **THEN** the class name SHALL be descriptive of its function
- **AND** SHALL use PascalCase without prefixes (unless template pattern)

#### Scenario: Manager class naming
- **WHEN** creating a manager/singleton class
- **THEN** the class name SHALL end with `Manager`

#### Scenario: Editor window naming
- **WHEN** creating an EditorWindow class
- **THEN** the class name SHALL end with `Window` or `Handler`

### Requirement: Claude Skills Integration
The project SHALL provide Claude skills for each major namespace to enable AI-assisted development.

#### Scenario: Skill discovery
- **WHEN** a developer uses Claude Code in this project
- **THEN** namespace-specific skills SHALL be available via `/mlcreator-*` commands

#### Scenario: Skill content
- **WHEN** a Claude skill is invoked
- **THEN** it SHALL provide: module context, key classes, common patterns, and GameCreator integration points

### Requirement: Placeholder Documentation
Empty or stub directories SHALL contain README documentation explaining intended purpose.

#### Scenario: Empty module directory
- **WHEN** a module directory is created without implementation
- **THEN** it SHALL contain a `README.md` explaining the module's intended purpose
