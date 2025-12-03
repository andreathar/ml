# Spec: KB Agent Toolkit - Full System Integration

## ADDED Requirements

### Requirement: Serena Memory Tier Browser

The system SHALL provide a tree view displaying all Serena memory tiers (CRITICAL, TOOLS, INTEGRATION, CONTEXT, AUTO_GENERATED).

The system SHALL display file count and estimated token count per tier.

The system SHALL allow loading individual memory files into context.

The system SHALL provide search functionality across all memory tiers.

#### Scenario: Browse memory tiers
- **WHEN** user opens the Memory Browser view
- **THEN** all 5 tiers are displayed as collapsible nodes
- **AND** each tier shows file count (e.g., "CRITICAL (6 files)")
- **AND** expanding a tier reveals individual memory files

#### Scenario: Load memory file
- **WHEN** user clicks "Load" on a memory file
- **THEN** memory content is added to current context
- **AND** token budget display updates
- **AND** file shows "loaded" status indicator

#### Scenario: Search across memories
- **WHEN** user enters search term in memory search box
- **THEN** results show matching files across all tiers
- **AND** results include snippet preview of matches

### Requirement: Smart Memory Loading

The system SHALL detect user intent from current file context and symbols.

The system SHALL automatically load relevant CRITICAL memories based on detected intent.

The system SHALL track and display token budget across all loaded context.

The system SHALL achieve 40-50% token savings compared to loading all memories.

#### Scenario: Intent-based loading for character work
- **WHEN** user opens a file in `GameCreator.Runtime.Characters` namespace
- **THEN** system detects "character-movement" intent
- **AND** automatically loads CRITICAL/001 and CRITICAL/002
- **AND** displays "Loaded 2 memories (~3000 tokens)"

#### Scenario: Intent-based loading for RPC work
- **WHEN** user opens a file with `[ServerRpc]` or `[ClientRpc]` attributes
- **THEN** system detects "rpc-implementation" intent
- **AND** automatically loads CRITICAL/003 and CRITICAL/005

#### Scenario: Token budget tracking
- **WHEN** memories are loaded
- **THEN** status bar shows current token usage
- **AND** warning appears if approaching 8000 token limit
- **AND** user can view detailed breakdown via command

### Requirement: Persona Schema Auto-Detection

The system SHALL automatically detect the relevant persona schema from current file's namespace.

The system SHALL fall back to assembly-based detection if namespace mapping fails.

The system SHALL display detected persona with symbol count in the UI.

The system SHALL provide manual persona override via dropdown.

#### Scenario: Namespace-based persona detection
- **WHEN** user opens file with `namespace GameCreator.Runtime.Inventory`
- **THEN** system detects "inventory" persona
- **AND** displays "Persona: Inventory (175 symbols)"
- **AND** loads `inventory.llm.txt` schema

#### Scenario: Assembly fallback detection
- **WHEN** file namespace doesn't match known mapping
- **AND** file is in `MLCreator_Multiplayer.Runtime` assembly
- **THEN** system uses assembly mapping to detect persona
- **AND** loads appropriate persona schema

#### Scenario: Manual persona override
- **WHEN** user selects different persona from dropdown
- **THEN** detected persona is overridden
- **AND** KB searches use new persona for filtering
- **AND** code generation uses new persona's rules

### Requirement: Critical Rules Panel

The system SHALL display P0 critical rules from `critical.llm.txt` in a sidebar panel.

The system SHALL provide quick reference sections for file organization, RPC naming, and Task signatures.

The system SHALL detect rule violations in active editor and show warnings.

#### Scenario: View critical rules
- **WHEN** user opens Critical Rules panel
- **THEN** panel displays parsed content of `critical.llm.txt`
- **AND** content is organized into collapsible sections
- **AND** quick reference cards show key constraints

#### Scenario: Rule violation detection
- **WHEN** user edits file with RPC method not ending in `ServerRpc` or `ClientRpc`
- **THEN** warning decoration appears on method line
- **AND** hover shows violation message with rule reference
- **AND** quick fix suggests correct naming

#### Scenario: File organization violation
- **WHEN** user attempts to create file at project root
- **THEN** warning appears with auto-redirect suggestion
- **AND** clicking suggestion moves file to proper location

### Requirement: Symbol Schema Navigation

The system SHALL load and display assemblies from `assemblies.json` (18 assemblies).

The system SHALL load and display GameCreator modules from `gamecreator_modules.json` (10 modules).

The system SHALL show module dependencies and sync components.

The system SHALL validate file placement against `folder_structure.json`.

#### Scenario: Browse assemblies
- **WHEN** user opens Assembly Navigator view
- **THEN** all 18 assemblies are displayed in tree
- **AND** expanding assembly shows namespaces
- **AND** clicking namespace filters KB search to that namespace

#### Scenario: View module dependencies
- **WHEN** user clicks "Show Dependencies" on Inventory module
- **THEN** dependency graph shows: Variables (required), Stats (optional)
- **AND** graph shows sync component: NetworkInventorySync

#### Scenario: File placement validation
- **WHEN** user creates new file
- **THEN** system suggests placement based on folder_structure.json
- **AND** warns if file is in wrong directory for its namespace

### Requirement: OpenSpec Proposal Browser

The system SHALL discover and display all active proposals from `.openspec/changes/`.

The system SHALL parse and display proposal.md summary (Why/What/Impact).

The system SHALL parse and display tasks.md with completion status.

The system SHALL show task completion percentage.

#### Scenario: View active proposals
- **WHEN** user opens OpenSpec Proposals view
- **THEN** all active proposals are listed with titles
- **AND** each proposal shows file count and task completion
- **AND** clicking proposal opens proposal.md in editor

#### Scenario: View proposal details
- **WHEN** user expands proposal in tree view
- **THEN** Why/What/Impact sections are displayed
- **AND** tasks.md checklist items are shown
- **AND** completion percentage is displayed (e.g., "5/10 tasks complete")

### Requirement: OpenSpec Conflict Detection

The system SHALL parse proposal impact sections for affected files.

The system SHALL warn when user edits a file affected by active proposal.

The system SHALL link warnings to relevant proposal for review.

#### Scenario: Conflict detection on file open
- **WHEN** user opens file affected by active proposal
- **THEN** warning banner appears at top of editor
- **AND** banner shows proposal name and link
- **AND** status bar shows conflict indicator

#### Scenario: Conflict details
- **WHEN** user clicks conflict warning
- **THEN** popup shows proposal summary
- **AND** link to proposal.md is provided
- **AND** option to dismiss warning for session

### Requirement: OpenSpec Archive Mining

The system SHALL index archived changes from `.openspec/changes/archive/`.

The system SHALL support semantic search through archived proposals.

The system SHALL extract patterns from successful implementations.

#### Scenario: Find similar changes
- **WHEN** user invokes "Find Similar Changes" with description
- **THEN** system searches archived proposals
- **AND** returns top 5 similar changes with relevance score
- **AND** each result shows proposal summary

#### Scenario: Extract pattern from archive
- **WHEN** user selects archived change
- **THEN** system can extract code patterns
- **AND** patterns can be used for code generation template

### Requirement: Project Brain State Dashboard

The system SHALL display state.json content in a webview dashboard.

The system SHALL show project health status with color coding.

The system SHALL display active issues with severity badges.

The system SHALL show system status (KB, MCP, compilation).

The system SHALL update live via file watcher.

#### Scenario: View state dashboard
- **WHEN** user opens State Dashboard
- **THEN** webview displays project info card
- **AND** health status shows with green/yellow/red indicator
- **AND** system status cards show KB (77,914 symbols), MCP (connected), compilation (0 errors)

#### Scenario: View active issues
- **WHEN** project has warnings or errors
- **THEN** issues list displays with severity colors
- **AND** clicking issue links to relevant memory or file
- **AND** issues can be dismissed

#### Scenario: Live updates
- **WHEN** state.json changes on disk
- **THEN** dashboard updates within 1 second
- **AND** new issues trigger notification

#### Scenario: Staleness warning
- **WHEN** state.json is more than 1 hour old
- **THEN** warning banner appears in dashboard
- **AND** suggests running Project Brain refresh

### Requirement: Context-Aware Memory Loading

The system SHALL parse task context maps from state.json.

The system SHALL apply context maps to optimize memory loading.

The system SHALL display recommended memory loads based on current task.

The system SHALL achieve 40-50% token savings through smart loading.

#### Scenario: Apply task context map
- **WHEN** state.json indicates current task is "character-movement"
- **THEN** system shows recommended memories to load
- **AND** clicking "Apply" loads all recommended memories
- **AND** token usage is optimized (4500 tokens vs 15000+ all)

### Requirement: MCP Tool Exposure

The system SHALL expose KB toolkit capabilities as MCP tools.

The system SHALL support 7 core MCP tools for agent integration.

The system SHALL use stdio transport for MCP communication.

#### Scenario: Agent searches with context
- **WHEN** Claude Code invokes `mcp__kb-toolkit__search_with_context`
- **THEN** tool returns KB results with persona-aware boosting
- **AND** results include symbol details and file locations

#### Scenario: Agent loads memory tier
- **WHEN** Claude Code invokes `mcp__kb-toolkit__load_memory_tier`
- **THEN** specified tier or file is loaded
- **AND** loaded content is available for context

#### Scenario: Agent gets persona
- **WHEN** Claude Code invokes `mcp__kb-toolkit__get_persona`
- **THEN** tool returns persona schema content
- **AND** includes symbol count and dependency info

#### Scenario: Agent checks proposal conflicts
- **WHEN** Claude Code invokes `mcp__kb-toolkit__check_proposal_conflicts`
- **THEN** tool returns list of proposals affecting file
- **AND** includes proposal summary for each conflict

#### Scenario: Agent gets project state
- **WHEN** Claude Code invokes `mcp__kb-toolkit__get_project_state`
- **THEN** tool returns parsed state.json content
- **AND** includes health status and active issues

#### Scenario: Agent browses OpenSpec
- **WHEN** Claude Code invokes `mcp__kb-toolkit__browse_openspec`
- **THEN** tool returns list of proposals, specs, or archives
- **AND** results include summary information

### Requirement: OpenSpec-Driven Workflows

The system SHALL support "Create Proposal" workflow.

The system SHALL support "Implement Change" workflow.

The system SHALL support "Archive Change" workflow.

#### Scenario: Create proposal workflow
- **WHEN** user invokes Create Proposal workflow
- **THEN** system queries KB for similar patterns
- **AND** analyzes patterns to suggest structure
- **AND** generates proposal.md and tasks.md scaffolding
- **AND** opens generated files in editor

#### Scenario: Implement change workflow
- **WHEN** user invokes Implement Change workflow for proposal
- **THEN** system loads proposal.md
- **AND** loads relevant memories and persona
- **AND** queries KB for implementation patterns
- **AND** generates code following project patterns
- **AND** validates against baseline specs

#### Scenario: Archive change workflow
- **WHEN** user invokes Archive Change workflow
- **THEN** system validates all tasks are complete
- **AND** updates baseline specs if needed
- **AND** moves proposal to archive directory
- **AND** reports archival success

### Requirement: Memory-Assisted Code Generation

The system SHALL use persona schema for code context.

The system SHALL apply CRITICAL rules during code generation.

The system SHALL validate generated code against baseline specs.

The system SHALL enforce RPC naming conventions.

The system SHALL validate Task signature format.

#### Scenario: Generate code with persona
- **WHEN** user invokes code generation in Character context
- **THEN** generated code follows character.llm.txt patterns
- **AND** uses correct namespace and assembly references
- **AND** follows module-specific conventions

#### Scenario: RPC naming enforcement
- **WHEN** code generation creates RPC method
- **THEN** method name ends with `ServerRpc` or `ClientRpc`
- **AND** warning appears if naming convention violated

#### Scenario: Task signature validation
- **WHEN** code generation creates visual scripting Task
- **THEN** signature is `Task Run(Args args)` without CancellationToken
- **AND** warning appears if signature is incorrect

#### Scenario: Preview before insertion
- **WHEN** code generation completes
- **THEN** preview diff is shown before insertion
- **AND** user can accept, modify, or cancel
- **AND** applying inserts code at cursor

### Requirement: Hybrid Vector Search

The system SHALL implement RRF (Reciprocal Rank Fusion) combining dense and sparse vectors.

The system SHALL use dense vectors for semantic similarity.

The system SHALL use sparse vectors (BM42) for keyword matching.

The system SHALL gracefully fallback if Qdrant version lacks hybrid support.

#### Scenario: Hybrid search improves relevance
- **WHEN** user searches "character movement methods"
- **THEN** results combine semantic matches (locomotion APIs) with keyword matches
- **AND** relevance improves 40%+ over scroll-based search

#### Scenario: Exact match priority
- **WHEN** user searches exact class name "NetworkCharacterAdapter"
- **THEN** exact match appears first
- **AND** related classes appear below

#### Scenario: Graceful fallback
- **WHEN** Qdrant version doesn't support hybrid search
- **THEN** system falls back to scroll-based search
- **AND** user is notified of reduced functionality

### Requirement: Context-Boosted Search

The system SHALL boost results from current file's assembly.

The system SHALL use persona schema for filtering.

The system SHALL provide toggle for context boosting.

#### Scenario: Assembly affinity boosting
- **WHEN** user searches from file in `MLCreator_Multiplayer.Runtime`
- **THEN** results from that assembly are boosted in ranking
- **AND** results from related assemblies appear higher

#### Scenario: Persona-aware filtering
- **WHEN** Inventory persona is active
- **THEN** search filters prefer Inventory-related symbols
- **AND** results show sync components and dependencies

### Requirement: Search Caching

The system SHALL cache search results with LRU eviction.

The system SHALL use TTL-based expiration (5 minutes).

The system SHALL invalidate cache on KB reindex.

#### Scenario: Cache hit performance
- **WHEN** user repeats same search within 5 minutes
- **THEN** cached results return in <5ms
- **AND** debug mode shows "Cache hit"

#### Scenario: Cache invalidation
- **WHEN** KB is reindexed (detected via point count change)
- **THEN** all cached results are invalidated
- **AND** next search fetches fresh results

## ADDED Non-Functional Requirements

### Requirement: Performance Targets

The system SHALL complete memory loading in <100ms.

The system SHALL complete KB queries in <50ms (excluding network).

The system SHALL initialize in <200ms on extension activation.

#### Scenario: Memory loading performance
- **WHEN** loading 3 CRITICAL memory files
- **THEN** total load time is under 100ms
- **AND** no UI blocking occurs

### Requirement: Graceful Degradation

The system SHALL function when any integrated system is unavailable.

The system SHALL clearly indicate which features are degraded.

#### Scenario: Serena unavailable
- **WHEN** `.serena/` directory doesn't exist
- **THEN** memory browser shows "Serena not configured"
- **AND** other features continue to work

#### Scenario: OpenSpec unavailable
- **WHEN** `.openspec/` directory doesn't exist
- **THEN** proposal browser shows "OpenSpec not configured"
- **AND** KB search and other features continue

#### Scenario: Project Brain unavailable
- **WHEN** `.project-brain/core/state.json` doesn't exist
- **THEN** state dashboard shows "Project Brain not running"
- **AND** context-aware loading falls back to manual

### Requirement: Backward Compatibility

The system SHALL preserve all existing commands.

The system SHALL not break existing configurations.

The system SHALL provide migration for deprecated features.

#### Scenario: Existing commands work
- **WHEN** user has keybinding for `unity-kb.nlQuery`
- **THEN** command continues to work as before
- **AND** new features are additive only

#### Scenario: Configuration migration
- **WHEN** user upgrades from version 0.1.0
- **THEN** existing settings are preserved
- **AND** new settings use sensible defaults
