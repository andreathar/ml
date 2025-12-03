# Capability: LLM Structure Optimization

## Overview
This capability defines the structural organization of LLM integration within the Game Creator Multiplayer strategic architecture.

## ADDED Requirements

### Requirement: LLM Integration Location
**ID**: `llm-structure-001`
**Priority**: P0 - Critical
**Category**: Architecture / Organization

The LLM integration MUST be located within the Game Creator Multiplayer package structure, following the established module pattern.

**Location**: `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/`

**Rationale**:
- Maintains unified strategic architecture
- Prevents parallel structure fragmentation
- Follows established module conventions
- Enables proper assembly organization

#### Scenario: LLM Module Discovery
**Given** a developer wants to find LLM integration code
**When** they navigate the project structure
**Then** they find LLM under `GameCreator_Multiplayer/Runtime/LLM/`
**And** the location matches other modules like `Behavior` or `CityVehicles`

#### Scenario: Assembly Reference Resolution
**Given** a script needs to reference LLM types
**When** the assembly references `GameCreator.Multiplayer.Runtime.LLM`
**Then** all LLM types are accessible
**And** the assembly compiles without errors

#### Scenario: Visual Scripting Component Registration
**Given** Unity Editor is open with the LLM module
**When** Game Creator scans for visual scripting components
**Then** Instructions and Events are registered
**And** they appear in the Game Creator menus

---

### Requirement: Assembly Definition Existence
**ID**: `llm-structure-002`
**Priority**: P0 - Critical
**Category**: Assembly / Compilation

The LLM module MUST have a dedicated assembly definition that explicitly declares dependencies.

**Assembly Name**: `GameCreator.Multiplayer.Runtime.LLM`
**Root Namespace**: `GameCreator.Multiplayer.Runtime.LLM`

**Required References**:
- `GameCreator.Runtime.Core` (Game Creator Core)
- `GameCreator.Multiplayer.Runtime` (Multiplayer Base)
- `undream.llmunity.Runtime` (LLMUnity Plugin)
- `Unity.Netcode.Runtime` (Unity Netcode)

#### Scenario: Assembly Compilation
**Given** the LLM assembly definition exists with proper references
**When** Unity recompiles the project
**Then** the `GameCreator.Multiplayer.Runtime.LLM` assembly compiles successfully
**And** no "assembly not found" errors occur

#### Scenario: Dependency Isolation
**Given** a change is made to LLM code
**When** Unity recompiles
**Then** only the `GameCreator.Multiplayer.Runtime.LLM` assembly recompiles
**And** other assemblies are not affected

#### Scenario: Reference Validation
**Given** the assembly definition references required assemblies
**When** Unity validates assembly references
**Then** all referenced assemblies are found
**And** no circular dependencies exist

---

### Requirement: Namespace Consistency
**ID**: `llm-structure-003`
**Priority**: P1 - Important
**Category**: Code Organization

All LLM integration code MUST use the namespace `GameCreator.Multiplayer.Runtime.LLM` to match the assembly name and module pattern.

**Namespace**: `GameCreator.Multiplayer.Runtime.LLM`
**Applies To**: All `.cs` files in the LLM module

#### Scenario: File Namespace Declaration
**Given** a C# file in the LLM module
**When** the file is opened
**Then** the namespace declaration is `GameCreator.Multiplayer.Runtime.LLM`
**And** no files use the old `MLCreator.Runtime.LLM` namespace

#### Scenario: Type Resolution in Visual Scripting
**Given** a visual scripting Instruction uses LLM types
**When** Unity compiles the visual script
**Then** all types resolve correctly
**And** no "type not found in namespace" errors occur

#### Scenario: IDE IntelliSense Navigation
**Given** a developer types "GameCreator.Multiplayer.Runtime.LLM" in an IDE
**When** IntelliSense activates
**Then** all LLM types appear in the suggestion list
**And** they are organized under the correct namespace

---

### Requirement: Directory Structure Organization
**ID**: `llm-structure-004`
**Priority**: P1 - Important
**Category**: Code Organization

The LLM module MUST organize code into logical subdirectories based on functionality.

**Required Subdirectories**:
- `Core/` - Core networking and service classes
- `Instructions/` - Visual scripting Actions
- `Events/` - Visual scripting Triggers

**Prohibited Locations**:
- `Assets/Scripts/MLCreator/` (parallel structure)
- `Assets/Scripts/` (root scripts directory)

#### Scenario: Core Class Location
**Given** core LLM classes like `LLMNetworkCharacter` and `LLMService`
**When** developers look for networking logic
**Then** they find these classes under `LLM/Core/`
**And** the organization is clear and logical

#### Scenario: Visual Scripting Component Discovery
**Given** visual scripting components (Instructions and Events)
**When** developers want to add new visual scripting nodes
**Then** they find Instructions in `LLM/Instructions/`
**And** Events in `LLM/Events/`
**And** the separation of concerns is clear

#### Scenario: Parallel Structure Elimination
**Given** the migration is complete
**When** searching for `Assets/Scripts/MLCreator/Runtime/LLM/`
**Then** the directory does not exist
**And** no LLM files remain at the old location
**And** no orphaned `.meta` files exist

---

### Requirement: Assembly Reference Graph Integrity
**ID**: `llm-structure-005`
**Priority**: P0 - Critical
**Category**: Assembly / Dependencies

The LLM assembly MUST maintain a clean, acyclic dependency graph with no circular references.

**Dependency Flow**:
```
GameCreator.Multiplayer.Runtime.LLM
  ├── GameCreator.Runtime.Core (framework)
  ├── GameCreator.Multiplayer.Runtime (multiplayer base)
  ├── undream.llmunity.Runtime (plugin)
  └── Unity.Netcode.Runtime (networking)
```

**Constraints**:
- No circular dependencies
- Only runtime assemblies (no Editor dependencies in Runtime)
- No dependencies on unrelated modules

#### Scenario: Dependency Graph Validation
**Given** the LLM assembly definition
**When** Unity builds the assembly dependency graph
**Then** no circular dependencies are detected
**And** the dependency graph is acyclic

#### Scenario: Runtime-Only Dependencies
**Given** the LLM Runtime assembly
**When** checking assembly references
**Then** all references are to Runtime assemblies
**And** no Editor assemblies are referenced
**And** the assembly can be used in builds

#### Scenario: Compilation Order
**Given** Unity needs to compile assemblies
**When** dependencies are resolved
**Then** all LLM dependencies compile before the LLM assembly
**And** compilation succeeds without ordering issues

---

### Requirement: Visual Scripting Component Registration
**ID**: `llm-structure-006`
**Priority**: P0 - Critical
**Category**: Integration / Visual Scripting

LLM visual scripting components (Instructions and Events) MUST be registered and accessible through the Game Creator visual scripting system.

**Required Components**:
- `InstructionLLMChat` - Action for sending prompts
- `InstructionLLMStop` - Action for canceling generation
- `EventLLMReply` - Trigger for LLM replies

**Registration Requirements**:
- Must appear in Game Creator Action/Event menus
- Must support property binding (e.g., `GetGameObjectPlayer`)
- Must follow Game Creator visual scripting patterns

#### Scenario: Action Menu Registration
**Given** the Unity Editor is open
**When** adding an Action to a Game Creator Trigger
**Then** "LLM Chat" appears under the Game Creator menu
**And** "LLM Stop" appears under the Game Creator menu
**And** both actions can be added to visual scripts

#### Scenario: Event Trigger Registration
**Given** the Unity Editor is open
**When** adding an Event to a Game Creator Trigger
**Then** "On LLM Reply" appears under the Game Creator menu
**And** the event can be added and configured

#### Scenario: Property Getter Support
**Given** an LLM Instruction in a visual script
**When** configuring the target GameObject
**Then** property getters like `GetGameObjectPlayer` are available
**And** they can be selected and configured
**And** they resolve correctly at runtime

---

### Requirement: Migration Safety and Reversibility
**ID**: `llm-structure-007`
**Priority**: P1 - Important
**Category**: Process / Safety

The migration process MUST preserve file GUIDs, maintain reference integrity, and provide rollback capability.

**Safety Requirements**:
- File moves preserve `.meta` files and GUIDs
- Git provides rollback to pre-migration state
- Incremental checkpoints allow early problem detection
- Unity's refactoring handles most reference updates

#### Scenario: GUID Preservation
**Given** files are moved from old to new location
**When** using Unity Editor's move operation
**Then** `.meta` files are moved with their files
**And** GUIDs are preserved
**And** existing references remain valid

#### Scenario: Git Rollback
**Given** issues are discovered during migration
**When** `git checkout -- Assets/` is executed
**Then** all files revert to pre-migration state
**And** the project is functional again
**And** no data loss occurs

#### Scenario: Incremental Validation
**Given** the migration process has checkpoints
**When** each phase completes
**Then** validation is performed
**And** issues are caught early
**And** the migration can be stopped before reaching a bad state

---

## REMOVED Requirements

### Requirement: Parallel Structure Support (Deprecated)
**ID**: `llm-structure-deprecated-001`
**Previously**: LLM integration allowed at `Assets/Scripts/MLCreator/`

**Removal Reason**: Creates fragmentation and violates strategic architecture principles. All multiplayer integrations must be unified under `GameCreator_Multiplayer/`.

#### Scenario: Old Location No Longer Valid
**Given** the migration is complete
**When** checking `Assets/Scripts/MLCreator/Runtime/LLM/`
**Then** the directory does not exist
**And** no LLM code remains at the old location

---

## Success Criteria

### Compilation
- ✅ Project compiles without errors
- ✅ `GameCreator.Multiplayer.Runtime.LLM` assembly builds successfully
- ✅ No "namespace not found" errors
- ✅ No "type not found" errors

### Organization
- ✅ All LLM code at `GameCreator_Multiplayer/Runtime/LLM/`
- ✅ No files at old location `Assets/Scripts/MLCreator/Runtime/LLM/`
- ✅ Directory structure follows Core/Instructions/Events pattern
- ✅ Assembly definition exists with correct references

### Functionality
- ✅ Visual scripting Instructions appear in Game Creator menus
- ✅ Visual scripting Events appear in Game Creator menus
- ✅ Property getters work (e.g., `GetGameObjectPlayer`)
- ✅ Runtime LLM functionality unchanged

### Documentation
- ✅ Architecture documentation updated
- ✅ Namespace references updated
- ✅ Migration report created

---

## Testing Strategy

### Unit Testing
- Verify assembly references resolve correctly
- Verify namespace consistency across all files
- Verify no circular dependencies

### Integration Testing
- Test visual scripting component registration
- Test property getter resolution
- Test LLM network synchronization

### Manual Testing
- Add LLM Instructions to visual scripts
- Add LLM Events to triggers
- Configure and test end-to-end LLM workflow

---

## Implementation Notes

### GUID Reference Format
Assembly references use GUIDs, not assembly names. Use Unity's assembly definition inspector to find correct GUIDs for:
- GameCreator.Runtime.Core
- GameCreator.Multiplayer.Runtime
- undream.llmunity.Runtime
- Unity.Netcode.Runtime

### File Move Best Practice
Always use Unity Editor for file operations (drag & drop in Project window) to preserve `.meta` files and GUIDs. Do NOT use file system operations.

### Namespace Refactoring
Unity's refactoring should handle most reference updates automatically via GUIDs. Manual verification still required for:
- Comments and documentation
- String-based references (rare)
- External tools or scripts

---

## Related Specifications

- **Character System**: `.serena/memories/CRITICAL/001_gamecreator_invasive_integration.md`
- **Network Architecture**: `.serena/memories/CRITICAL/002_network_architecture_never_forget.md`
- **LLM Integration Architecture**: `docs/_knowledge-base/04-architecture/LLMUnity_Integration_Architecture.md`

---

## Version History

- **v1.0.0** (2025-11-24): Initial specification for structural optimization
