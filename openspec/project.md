# Project Context

## Purpose

**MLCreator** is a Unity-based multiplayer game development framework designed to achieve **10x improvement** in performance and development efficiency through AI-powered assistance and advanced optimization systems.

### Core Goals
- **AI-Driven Development**: Integration with Antigravity Editor (Gemini 3 Pro) and Serena MCP Server for intelligent code assistance
- **Performance Optimization**: Custom C# framework providing memory management, caching, object pooling, and asset optimization
- **Multiplayer Excellence**: Built on Unity Netcode for GameObjects with Game Creator integration
- **Knowledge Management**: Foam-based neural map with 10,095+ interconnected documentation files
- **Batch Processing**: Overnight task system for asset analysis, validation, and optimization

## Tech Stack

### Core Engine & Runtime
- **Unity 6 (6000.2.13f1)** - Game engine (LTS version for stability)
- **C# 9.0+** - Primary programming language (UTF-8 encoding required)
- **Unity DOTS** (Data-Oriented Technology Stack) - High-performance systems
  - Unity.Entities (ECS framework)
  - Unity.Collections (Memory-efficient collections)
  - Unity.Mathematics (SIMD math operations)
  - Unity.Transforms (Transform system)
  - Unity.Physics (DOTS physics)
  - Unity.Burst (JIT compilation for performance)
- **Unity Netcode for GameObjects 2.7.0** - Multiplayer networking
  - NetworkBehaviour/NetworkObject patterns
  - ServerRpc/ClientRpc communication
  - NetworkVariable synchronization
  - Distributed Authority support

### Game Creator Framework
- **Game Creator 2.0+** - Visual scripting and modular game systems
  - **18 installed modules**: Addressables, Behavior, Core, Dialogue, EditorPro, Factions, Finder, GameCreator_Multiplayer, Inventory, Localization, Mailbox, NanoSave, Perception, Platforming, Quests, Shooter, Stats, Tactile
  - **Custom multiplayer integration**: Invasive modification of core GameCreator files for Unity Netcode compatibility
  - **10,095+ documented components**: Auto-generated Foam knowledge base with comprehensive API documentation
  - **Visual Scripting**: Actions, Conditions, Triggers, and Events with multiplayer synchronization

### AI & Developer Tools
- **Antigravity Editor** - AI-powered code editor using Gemini 3 Pro (200K context window)
- **Serena MCP Server** - Hierarchical memory management system with predictive preloading
  - Memory tiers: CRITICAL (must-read), CORE (foundation), INTEGRATION (framework-specific), TOOLS (development utilities)
  - Proactive intelligence with code health monitoring and dependency tracking
  - OpenSpec integration for unified workflow management
- **Unity MCP Server** - Local stdio-based Unity integration (safe mode enabled)
- **Foam** - Knowledge graph and neural mapping system
  - 10,095+ markdown documentation files (auto-generated from C# source)
  - Automated tagging and wikilink generation
  - Consolidated into docs/_knowledge-base/ for unified access
- **OpenSpec** - Specification-driven development workflow
  - Change proposals, task tracking, and implementation management
  - Standards enforcement for Assemblies, folder structure, APIs, and MCP servers

### Additional Libraries
- **Unity TextMeshPro** - Advanced text rendering
- **Unity Input System** - Modern input handling
- **Unity Cinemachine** - Camera system
- **Supabase** - Backend services integration (planned/in-progress)

### Development Environment
- **Python 3.x** - Automation scripts and tooling
- **PowerShell** - Windows automation and task scheduling
- **Git** - Version control
- **Windows Task Scheduler** - Overnight batch processing

## Project Conventions

### Code Style

#### C# Conventions
- **Namespace Structure**: `MLCreator.{Module}.{Feature}` (UTF-8 encoding required)
  - Examples: `MLCreator.Framework`, `MLCreator.Core`, `MLCreator.Multiplayer`, `MLCreator.AI`
  - **Assembly Boundaries**: Strict namespace-to-assembly mapping enforced
- **Naming Conventions**:
  - Classes: PascalCase (e.g., `MLCreatorInitializer`, `NetworkCharacterAdapter`)
  - Methods: PascalCase (e.g., `InitializeOptimizationSystems`, `RequestMovementServerRpc`)
  - Private fields: camelCase with `m_` prefix (e.g., `m_character`, `m_weapon`, `m_networkVariable`)
  - Properties: PascalCase (e.g., `Title`, `DefaultResult`, `IsNetworkSpawned`)
  - Network RPCs: `ServerRpc`/`ClientRpc` suffix (e.g., `SendDataServerRpc`)
- **Performance Focus**: All code must consider performance implications
  - Memory pooling mandatory for frequently instantiated objects
  - DOTS/ECS patterns for performance-critical systems
  - Network traffic optimization required
- **Singleton Pattern**: Core systems use singleton pattern for global access
- **Data-Oriented Design**: Prefer DOTS and ECS patterns where applicable
- **Network Architecture**: Server-authoritative with client prediction where appropriate

#### Game Creator Integration
- **Actions**: Prefixed with `Instruction` (e.g., `InstructionShooterFirePull`)
- **Conditions**: Prefixed with `Condition` (e.g., `ConditionShooterIsReloading`)
- **Triggers**: Prefixed with `Event` (e.g., `EventShooterWeaponFired`)
- **Visual Scripting Nodes**: No CancellationToken in Task.Run() methods (compilation error)
- **Attributes**: Use C# attributes for metadata and visual scripting integration
  ```csharp
  [Version(1, 0, 0)]
  [Title("Pull Fire Trigger")]
  [Description("Pulls the fire trigger on a shooter weapon")]
  [Category("Shooter/Shooting/Pull Fire Trigger")]
  [Image(typeof(IconShooter), ColorTheme.Type.Red)]
  [Parameter("Character", "The Character reference with a Weapon equipped")]
  [Keywords("Shooter", "Combat", "Shoot", "Execute", "Trigger", "Press", "Blast")]
  ```
- **Property Binding**: Use `PropertyGet*` types for variable binding (e.g., `PropertyGetGameObject`)
- **Event System**: `EventChange` for reactive updates
- **Invasive Integration**: Direct modification of GameCreator core files for Netcode compatibility

### Architecture Patterns

#### Core Architecture
1. **Modular Systems**: Framework divided into specialized assemblies
   - `MLCreator.Core` - Core systems and managers
   - `MLCreator.Gameplay` - Game mechanics and features
   - `MLCreator.Multiplayer` - Network synchronization
   - `MLCreator.AI` - Artificial intelligence systems
   - `MLCreator.UI` - User interface components

2. **Singleton Pattern**: Used for core initializer and system managers
   ```csharp
   public class MLCreatorInitializer : MonoBehaviour
   {
       private static MLCreatorInitializer instance;
       public static MLCreatorInitializer Instance => instance;

       // Core system initialization order
       private void Awake()
       {
           instance = this;
           InitializeMemoryManagement();
           InitializeNetworkSystems();
           InitializeGameCreatorIntegration();
       }
   }
   ```

3. **Data-Oriented Design**: Utilize Unity DOTS for high-performance systems
   - ECS (Entity Component System) for game logic
   - Job System for parallel processing
   - Burst Compiler for optimized code
   - Native collections for memory efficiency

4. **Multiplayer Architecture**:
   - **NetworkBehaviour** components for synchronized objects
   - **Server-authoritative** design with client prediction
   - **NetworkCharacterAdapter** for GameCreator character sync
   - **Invasive GameCreator integration** for seamless multiplayer
   - **RPC patterns**: ServerRpc/ClientRpc with proper ownership checks

5. **Batch Processing**:
   - JSON-based task queue configuration (`claudedocs/overnight-tasks/queue.json`)
   - Overnight processing at 2 AM via Windows Task Scheduler
   - Results stored in `claudedocs/overnight-results/`
   - Asset analysis, prefab validation, and optimization tasks

### Testing Strategy

#### Manual Testing
- Unity Editor Play Mode testing
- **MLCreator Menu Items**:
  - `MLCreator → Unity MCP → Check Server Health`
  - `MLCreator → Batch Processing → Process Overnight Tasks`

#### Automated Testing
- Overnight batch validation (asset analysis, prefab validation)
- Network object audits
- Script reference mapping

#### Performance Testing
- Frame rate monitoring
- Memory profiling
- Asset optimization metrics

### Git Workflow

#### Branch Strategy
- **main** - Production-ready code
- **develop** - Integration branch
- **feature/** - Feature development branches
- **hotfix/** - Emergency fixes

#### Commit Conventions
- Use descriptive commit messages
- Reference related documentation updates
- Include performance impact notes when relevant
- Reference OpenSpec change IDs when applicable

### OpenSpec Workflow

#### Change Categories
- **Assembly Creation**: Assembly definitions, references, compilation settings
- **Folder Reorganization**: Project structure optimization and organization
- **API Integration**: External service integration following best practices
- **MCP Server Setup**: MCP server configuration and tooling integration

#### Proposal Process
1. **Create Proposal**: Use `openspec` CLI or templates
2. **Standards Validation**: Automatic compliance checking
3. **Implementation**: Follow specification-driven development
4. **Archive**: Move completed changes to archive with proper versioning

#### Standards Enforcement
- **Assembly**: Namespace patterns, compilation settings, reference validation
- **Folders**: Functional grouping, scalability patterns, dependency flow
- **APIs**: Error handling, security validation, performance monitoring
- **MCP**: Authentication standards, tool validation, integration testing

## Domain Context

### Unity Game Development
- **Multiplayer Game**: Built with Unity Netcode for GameObjects
- **Visual Scripting**: Extensive use of Game Creator 2 framework
- **Performance Critical**: Focus on optimization and efficient resource usage

### AI-Assisted Development
- **Antigravity Integration**: AI-powered editor using Gemini 3 Pro
- **Serena MCP**: Contextual AI assistant with project knowledge
- **Foam Knowledge Graph**: 10,095+ interconnected documentation files
  - Auto-generated from C# source code
  - Comprehensive Game Creator module documentation
  - Wikilink-based navigation

### Specialized Systems
- **Memory Management**: Custom pooling and caching
- **Asset Optimization**: Automated analysis and recommendations
- **Network Synchronization**: Multiplayer state management
- **Batch Processing**: Overnight task execution for long-running operations

## Important Constraints

### Technical Constraints
1. **Unity Version**: Must maintain compatibility with Unity 6 (6000.0+)
2. **Performance Target**: Maintain 60+ FPS in multiplayer scenarios
3. **Platform**: Primary target is Windows (x64)
4. **Netcode Compatibility**: All multiplayer code must be compatible with Unity Netcode for GameObjects
5. **DOTS Integration**: Performance-critical systems should use DOTS when possible
6. **Memory Budget**: Optimized for efficient memory usage through pooling and caching

### Development Constraints
1. **AI Integration**: All major systems must be documented for AI assistant understanding
2. **Foam Documentation**: Code changes require corresponding Foam documentation updates
3. **Batch Processing**: Long-running tasks (>5 minutes) should use overnight processing
4. **MCP Server**: Local Unity MCP Server must be accessible via stdio

### Business Constraints
1. **Performance Focus**: 10x improvement target across all optimization metrics
2. **AI-First Development**: Prioritize AI-assisted workflows
3. **Modular Design**: Systems must be independently testable and swappable

## External Dependencies

### AI Services
- **Gemini 3 Pro API** - Powers Antigravity Editor
- **Serena MCP Server** - Project context and memory management
  - Local Python server (`serena-env`)
  - Stdio communication protocol

### Unity MCP Integration
- **Unity MCP Server** - Local executable
  - Location: `Library/mcp-server/win-x64/unity-mcp-server.exe`
  - Communication: stdio
  - Configuration: `.antigravity/mcp_config.json`

### Backend Services
- **Supabase** (planned/in-progress)
  - Authentication
  - Real-time multiplayer data
  - Database services

### Unity Asset Store Packages
- **Game Creator 2** - Core framework + 18 modules
  - Addressables, Behavior, Core, Dialogue, EditorPro
  - Factions, Finder, GameCreator_Multiplayer, Inventory
  - Localization, Mailbox, NanoSave, Perception
  - Platforming, Quests, Shooter, Stats, Tactile

### Development Tools
- **Python Environment** (`serena-env`) - Automation scripting
- **PowerShell** - Windows automation
- **Windows Task Scheduler** - Overnight batch processing

### Documentation System
- **Foam** - Knowledge graph system
  - VSCode extension
  - Markdown-based
  - Wikilink navigation

## Key Directory Structure

```
MLCreator/
├── .antigravity/              # Antigravity Editor configuration
│   └── mcp_config.json        # MCP server configuration
├── .foam/                     # Foam workspace configuration
│   └── foam.config.json       # Foam settings (consolidated ~284 files)
├── .serena/                   # Serena AI memory system
│   ├── config.yaml           # Serena configuration
│   ├── project.yml           # Project-specific settings
│   ├── ai/                   # AI processing files
│   ├── cache/                # Multi-tier caching system
│   ├── logs/                 # Health monitoring logs
│   └── memories/             # Hierarchical memory tiers
├── Assets/                    # Unity project assets
│   ├── MLCreator.asmdef      # Assembly definition (MLCreator.*)
│   ├── MLCreatorInitializer.cs # Core initializer (singleton pattern)
│   ├── Plugins/
│   │   └── GameCreator/      # Game Creator 2.0+ (18 modules)
│   └── Resources/
│       └── Unity-MCP-ConnectionConfig.json
├── claudedocs/                # Claude AI documentation
│   ├── overnight-tasks/       # Batch task configurations
│   │   └── queue.json         # Task queue (JSON-based)
│   └── overnight-results/     # Batch processing results
├── docs/                      # Unified documentation system
│   ├── _knowledge-base/       # Single source of truth (~40 files)
│   │   ├── 01-critical/       # Must-read essentials
│   │   ├── 02-guides/         # How-to documentation (13+ files)
│   │   ├── 03-api-reference/  # Technical references (17+ files)
│   │   ├── 04-architecture/   # System design
│   │   └── 05-workflows/      # Process documentation
│   └── _archive/              # Historical documentation (6,000+ files)
├── foam/                      # Foam knowledge base (10,095+ files)
│   ├── gamecreator/           # Game Creator documentation
│   │   ├── modules/           # Module overviews
│   │   ├── actions/           # Action documentation
│   │   ├── conditions/        # Condition documentation
│   │   ├── triggers/          # Trigger documentation
│   │   └── classes/           # Class documentation
│   ├── templates/             # Foam templates
│   └── scripts/               # Auto-generated content (5,754 files)
├── Library/                   # Unity Library (generated)
│   └── mcp-server/            # Unity MCP Server
│       └── win-x64/
│           └── unity-mcp-server.exe
├── openspec/                  # OpenSpec specification system
│   ├── AGENTS.md             # AI assistant instructions
│   ├── project.md            # This file - project conventions
│   ├── changes/              # Change proposals and tracking
│   ├── specs/                # Technical specifications
│   └── templates/            # Proposal templates (4 categories)
├── scripts/                   # Automation scripts
│   ├── activate-environment.ps1    # Environment activation
│   ├── generate-gamecreator-docs.py # Foam documentation generation
│   ├── foam-tag-generator.py       # Automated tagging
│   ├── run-overnight-tasks.ps1     # Batch processing
│   ├── setup-task-scheduler.ps1    # Windows Task Scheduler setup
│   └── validate-foam-tags.ps1      # Tag validation
├── serena-env/                # Python virtual environment (AI tooling)
├── GEMINI.md                  # AI assistant context
├── QUICK_START.md             # Quick start guide
└── README.md                  # Project overview
```

## Quick Reference

### Essential Commands

```powershell
# Activate environment
.\activate-environment.ps1

# Generate/update Foam documentation
python scripts/generate-gamecreator-docs.py

# Validate Foam tags
powershell -ExecutionPolicy Bypass -File scripts/validate-foam-tags.ps1

# Run overnight tasks manually
.\scripts\run-overnight-tasks.ps1

# Setup automated scheduling
.\scripts\setup-task-scheduler.ps1
```

### Key Documentation
- **Unified Knowledge Base**: `docs/_knowledge-base/` - Single source of truth
- **Project Overview**: `GEMINI.md`
- **Quick Start**: `QUICK_START.md`
- **AI Mandatory Checklist**: `docs/_knowledge-base/01-critical/AI_MANDATORY_CHECKLIST.md`
- **MCP Token Limits**: `docs/_knowledge-base/01-critical/MCP_TOKEN_LIMITS_GUIDE.md`
- **OpenSpec Instructions**: `openspec/AGENTS.md`

### AI Assistant Context
- **Serena MCP**: Hierarchical memory management with predictive preloading
- **Antigravity**: Gemini 3 Pro-powered editor (200K context window)
- **Foam Graph**: 10,095+ interconnected documentation files (consolidated access)
- **OpenSpec**: Specification-driven development with standards enforcement

### Recent Major Updates (2025-11-20)
- **Documentation Consolidation**: 6,140+ files → 40 essential files (99.2% reduction)
- **Unified Knowledge Base**: Single source of truth at `docs/_knowledge-base/`
- **OpenSpec Integration**: Complete workflow management for Assemblies/Folders/APIs/MCP
- **Standards Enforcement**: Automated compliance checking for best practices
- **Memory Optimization**: Hierarchical Serena system with proactive intelligence

---

**Last Updated**: 2025-11-20
**Documentation Consolidation**: ✅ Complete (99.2% reduction in active files)
**OpenSpec Integration**: ✅ Complete (standards enforcement active)
**Knowledge Base**: ✅ Unified (docs/_knowledge-base/)
**Status**: Active Development, AI-Optimized, Specification-Driven
**Version**: MLCreator 10x Framework v2.0 (Post-Consolidation)
