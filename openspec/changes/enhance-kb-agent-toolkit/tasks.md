# Tasks: Enhanced KB Agent Toolkit - Full System Integration

## Phase 1: Serena Memory Integration

### 1.1 Memory Tier Browser
- [ ] 1.1.1 Create `src/integrations/serenaMemories.ts` base module
- [ ] 1.1.2 Implement memory tier discovery from `.serena/memories/`
- [ ] 1.1.3 Parse tier structure (CRITICAL, TOOLS, INTEGRATION, CONTEXT, AUTO_GENERATED)
- [ ] 1.1.4 Create tree view provider for memory browser
- [ ] 1.1.5 Add file count and token estimate per tier
- [ ] 1.1.6 Implement memory file preview on hover
- [ ] 1.1.7 Add "Load Memory" command for individual files
- [ ] 1.1.8 Create memory search across all tiers

### 1.2 Smart Memory Loading
- [ ] 1.2.1 Define intent-to-memory mapping configuration
- [ ] 1.2.2 Implement intent detection from file path and symbols
- [ ] 1.2.3 Create P0 loader for `critical.llm.txt` and `state.json`
- [ ] 1.2.4 Implement task-specific memory loading
- [ ] 1.2.5 Add token budget tracking and display
- [ ] 1.2.6 Create memory cache with file watcher invalidation
- [ ] 1.2.7 Add "Clear Memory Cache" command
- [ ] 1.2.8 Write unit tests for intent detection

### 1.3 Persona Schema Integration
- [ ] 1.3.1 Create `src/integrations/serenaPersonas.ts` module
- [ ] 1.3.2 Parse `_index.llm.txt` and `_summary.json` for persona list
- [ ] 1.3.3 Implement namespace-to-persona mapping
- [ ] 1.3.4 Implement assembly-to-persona fallback mapping
- [ ] 1.3.5 Create persona auto-detection from current file
- [ ] 1.3.6 Add persona selector dropdown to advanced query
- [ ] 1.3.7 Display persona symbol count in selector
- [ ] 1.3.8 Create persona dependency graph viewer (webview)

### 1.4 Critical Rules Panel
- [ ] 1.4.1 Create sidebar panel for P0 rules display
- [ ] 1.4.2 Parse and render `critical.llm.txt` content
- [ ] 1.4.3 Add quick reference for file organization rules
- [ ] 1.4.4 Add quick reference for RPC naming conventions
- [ ] 1.4.5 Add quick reference for Task signatures
- [ ] 1.4.6 Implement rule violation detection in active editor
- [ ] 1.4.7 Show warning decorations for rule violations

### 1.5 Symbol Schema Navigation
- [ ] 1.5.1 Create `src/integrations/serenaSymbols.ts` module
- [ ] 1.5.2 Load and parse `assemblies.json` (18 assemblies)
- [ ] 1.5.3 Load and parse `gamecreator_modules.json` (10 modules)
- [ ] 1.5.4 Load and parse `folder_structure.json`
- [ ] 1.5.5 Create assembly tree view with namespace browsing
- [ ] 1.5.6 Create module cards view with dependencies
- [ ] 1.5.7 Add "Navigate to Assembly" command
- [ ] 1.5.8 Add "Show Module Dependencies" command
- [ ] 1.5.9 Implement file placement validation against folder_structure

## Phase 2: OpenSpec Integration

### 2.1 Proposal Browser
- [ ] 2.1.1 Create `src/integrations/openspecBrowser.ts` module
- [ ] 2.1.2 Discover active proposals from `.openspec/changes/`
- [ ] 2.1.3 Parse proposal.md files for summary extraction
- [ ] 2.1.4 Create tree view for active proposals
- [ ] 2.1.5 Add proposal quick view with Why/What/Impact
- [ ] 2.1.6 Parse and display tasks.md checklist
- [ ] 2.1.7 Show task completion percentage
- [ ] 2.1.8 Add "Open Proposal" command to open in editor

### 2.2 Spec Query and Navigation
- [ ] 2.2.1 Index baseline specs from `.openspec/specs/`
- [ ] 2.2.2 Parse spec.md files for requirements
- [ ] 2.2.3 Create spec search by requirement or scenario
- [ ] 2.2.4 Add spec tree view with capabilities
- [ ] 2.2.5 Implement requirement detail view
- [ ] 2.2.6 Add "Find Spec for File" command

### 2.3 Conflict Detection
- [ ] 2.3.1 Create `src/integrations/openspecValidator.ts` module
- [ ] 2.3.2 Parse proposal impact sections for affected files
- [ ] 2.3.3 Implement conflict check for current file
- [ ] 2.3.4 Add conflict warning badge in status bar
- [ ] 2.3.5 Create conflict details popup on click
- [ ] 2.3.6 Add diagnostic warnings for affected files
- [ ] 2.3.7 Link warnings to proposal for review

### 2.4 Archive Mining
- [ ] 2.4.1 Index archived changes from `.openspec/changes/archive/`
- [ ] 2.4.2 Extract patterns from archived proposals
- [ ] 2.4.3 Create "Find Similar Changes" command
- [ ] 2.4.4 Implement semantic search through archives
- [ ] 2.4.5 Add archive browser tree view
- [ ] 2.4.6 Create pattern extraction from archived code

## Phase 3: Project Brain Integration

### 3.1 State Dashboard
- [ ] 3.1.1 Create `src/integrations/projectBrainState.ts` module
- [ ] 3.1.2 Load and parse `.project-brain/core/state.json`
- [ ] 3.1.3 Create webview dashboard for project state
- [ ] 3.1.4 Display project info (name, Unity version, Netcode version)
- [ ] 3.1.5 Display health status with color coding
- [ ] 3.1.6 Display active issues with severity badges
- [ ] 3.1.7 Display system status (KB, MCP, compilation)
- [ ] 3.1.8 Add file watcher for live updates
- [ ] 3.1.9 Show staleness warning if state > 1 hour old

### 3.2 Context-Aware Loading
- [ ] 3.2.1 Create `src/integrations/projectBrainContext.ts` module
- [ ] 3.2.2 Parse task context maps from state.json
- [ ] 3.2.3 Implement user intent detection from actions
- [ ] 3.2.4 Apply task context map to memory loading
- [ ] 3.2.5 Track token budget across loaded context
- [ ] 3.2.6 Display recommended memory loads
- [ ] 3.2.7 Add "Apply Context Map" command
- [ ] 3.2.8 Implement predictive preloading based on file patterns

### 3.3 Health Monitoring
- [ ] 3.3.1 Create status bar item for project health
- [ ] 3.3.2 Show quick pick menu with health details
- [ ] 3.3.3 Add notification for new issues
- [ ] 3.3.4 Link to relevant memory for issue resolution

## Phase 4: Enhanced KB Search

### 4.1 Hybrid Vector Search
- [ ] 4.1.1 Upgrade `src/qdrantClient.ts` for hybrid search
- [ ] 4.1.2 Implement dense vector query builder
- [ ] 4.1.3 Implement sparse vector (BM42) query builder
- [ ] 4.1.4 Add RRF fusion algorithm (k=60)
- [ ] 4.1.5 Create parallel query executor
- [ ] 4.1.6 Add Qdrant version detection for feature gating
- [ ] 4.1.7 Implement graceful fallback to scroll search

### 4.2 Context-Boosted Search
- [ ] 4.2.1 Integrate persona schema with search filters
- [ ] 4.2.2 Add assembly affinity boosting from current file
- [ ] 4.2.3 Add namespace proximity scoring
- [ ] 4.2.4 Implement memory-informed query expansion
- [ ] 4.2.5 Create "Search with Context" toggle

### 4.3 Search Caching
- [ ] 4.3.1 Implement LRU cache for search results
- [ ] 4.3.2 Add cache key generator (query + filters + context)
- [ ] 4.3.3 Implement TTL-based expiration (5 min)
- [ ] 4.3.4 Add cache invalidation on KB reindex
- [ ] 4.3.5 Display cache hit/miss in debug mode
- [ ] 4.3.6 Add "Clear Search Cache" command

## Phase 5: Agent Workflows

### 5.1 MCP Tool Exposure
- [ ] 5.1.1 Create `src/mcpBridge.ts` for MCP protocol
- [ ] 5.1.2 Define `mcp__kb-toolkit__search_with_context` tool
- [ ] 5.1.3 Define `mcp__kb-toolkit__load_memory_tier` tool
- [ ] 5.1.4 Define `mcp__kb-toolkit__get_persona` tool
- [ ] 5.1.5 Define `mcp__kb-toolkit__check_proposal_conflicts` tool
- [ ] 5.1.6 Define `mcp__kb-toolkit__generate_code` tool
- [ ] 5.1.7 Define `mcp__kb-toolkit__get_project_state` tool
- [ ] 5.1.8 Define `mcp__kb-toolkit__browse_openspec` tool
- [ ] 5.1.9 Implement stdio transport for MCP
- [ ] 5.1.10 Add MCP tool registration on extension activation

### 5.2 OpenSpec-Driven Workflows
- [ ] 5.2.1 Create `src/workflows/createProposal.ts`
- [ ] 5.2.2 Implement KB query → pattern analysis → scaffolding
- [ ] 5.2.3 Generate proposal.md from template
- [ ] 5.2.4 Generate tasks.md from detected requirements
- [ ] 5.2.5 Create `src/workflows/implementChange.ts`
- [ ] 5.2.6 Load proposal → memories → KB query → code generation
- [ ] 5.2.7 Create `src/workflows/archiveChange.ts`
- [ ] 5.2.8 Validate completion → update specs → move to archive

### 5.3 Memory-Assisted Code Generation
- [ ] 5.3.1 Create `src/workflows/generateCode.ts`
- [ ] 5.3.2 Load relevant persona for code context
- [ ] 5.3.3 Apply CRITICAL rules to generated code
- [ ] 5.3.4 Validate against baseline spec if applicable
- [ ] 5.3.5 Implement RPC naming convention enforcement
- [ ] 5.3.6 Implement Task signature validation
- [ ] 5.3.7 Add file organization rule checking
- [ ] 5.3.8 Create preview before insertion

### 5.4 Full Pipeline Workflow
- [ ] 5.4.1 Create `src/workflows/fullPipeline.ts`
- [ ] 5.4.2 Implement intent detection step
- [ ] 5.4.3 Implement state.json loading step
- [ ] 5.4.4 Implement critical rules loading step
- [ ] 5.4.5 Implement persona detection step
- [ ] 5.4.6 Implement memory loading step
- [ ] 5.4.7 Implement proposal conflict check step
- [ ] 5.4.8 Implement KB search step
- [ ] 5.4.9 Implement code generation step
- [ ] 5.4.10 Implement Unity MCP apply step
- [ ] 5.4.11 Add pipeline progress indicator

## Phase 6: Visual Enhancements

### 6.1 Memory Browser View
- [ ] 6.1.1 Create collapsible tier nodes
- [ ] 6.1.2 Add file icons per memory type
- [ ] 6.1.3 Show load status (loaded/not loaded)
- [ ] 6.1.4 Display token count per file
- [ ] 6.1.5 Add context menu for load/unload

### 6.2 Persona Dependency Graph
- [ ] 6.2.1 Create webview for dependency visualization
- [ ] 6.2.2 Render modules as nodes
- [ ] 6.2.3 Render dependencies as edges
- [ ] 6.2.4 Add click navigation to module details
- [ ] 6.2.5 Highlight current file's module

### 6.3 State Dashboard Webview
- [ ] 6.3.1 Create responsive dashboard layout
- [ ] 6.3.2 Add health status card with icon
- [ ] 6.3.3 Add system status cards
- [ ] 6.3.4 Add issues list with severity colors
- [ ] 6.3.5 Add task context recommendations
- [ ] 6.3.6 Add refresh button
- [ ] 6.3.7 Add deep links to memory files

### 6.4 OpenSpec Proposal Panel
- [ ] 6.4.1 Create side panel for active proposals
- [ ] 6.4.2 Show proposal summary card
- [ ] 6.4.3 Display tasks with checkboxes
- [ ] 6.4.4 Add progress bar for completion
- [ ] 6.4.5 Add links to affected files

## Phase 7: Testing and Documentation

### 7.1 Unit Testing
- [ ] 7.1.1 Set up Jest testing framework
- [ ] 7.1.2 Write tests for memory tier loading
- [ ] 7.1.3 Write tests for persona detection
- [ ] 7.1.4 Write tests for OpenSpec parsing
- [ ] 7.1.5 Write tests for conflict detection
- [ ] 7.1.6 Write tests for state.json parsing
- [ ] 7.1.7 Write tests for intent detection
- [ ] 7.1.8 Write tests for token counting

### 7.2 Integration Testing
- [ ] 7.2.1 Test memory loading with real .serena/ structure
- [ ] 7.2.2 Test OpenSpec parsing with real proposals
- [ ] 7.2.3 Test KB search with hybrid vectors
- [ ] 7.2.4 Test MCP tool invocation
- [ ] 7.2.5 Test full pipeline workflow
- [ ] 7.2.6 Create mock Qdrant for CI testing

### 7.3 Documentation
- [ ] 7.3.1 Update README with new features
- [ ] 7.3.2 Document Serena memory integration
- [ ] 7.3.3 Document OpenSpec workflow support
- [ ] 7.3.4 Document Project Brain integration
- [ ] 7.3.5 Document MCP tools for agents
- [ ] 7.3.6 Create troubleshooting guide
- [ ] 7.3.7 Add configuration reference
- [ ] 7.3.8 Create video walkthrough script

### 7.4 Release
- [ ] 7.4.1 Update package.json version to 0.2.0
- [ ] 7.4.2 Update extension metadata
- [ ] 7.4.3 Create CHANGELOG.md
- [ ] 7.4.4 Build VSIX package
- [ ] 7.4.5 Test on clean VS Code installation
- [ ] 7.4.6 Write release notes

## Completion Criteria

### Phase 1: Serena Integration
- [ ] Memory browser shows all 5 tiers with file counts
- [ ] Smart loading achieves 40-50% token savings vs. loading all
- [ ] Persona auto-detection works for all 13 modules
- [ ] Critical rules panel displays P0 constraints

### Phase 2: OpenSpec Integration
- [ ] Active proposals are discoverable from extension
- [ ] Conflict detection warns when editing affected files
- [ ] Archive mining finds similar past changes

### Phase 3: Project Brain Integration
- [ ] State dashboard shows project health
- [ ] Context-aware loading applies task context maps
- [ ] Health status visible in status bar

### Phase 4: Enhanced KB Search
- [ ] Hybrid search improves relevance by 40%+
- [ ] Context boosting prioritizes current file's assembly
- [ ] Cache reduces repeated query latency to <5ms

### Phase 5: Agent Workflows
- [ ] All 7 MCP tools are callable by Claude Code
- [ ] Create Proposal workflow generates valid scaffolding
- [ ] Full pipeline completes in <30 seconds

### Phase 6: Visual Enhancements
- [ ] Memory browser is visually clear and functional
- [ ] State dashboard renders correctly
- [ ] Persona graph shows dependencies

### Phase 7: Testing & Docs
- [ ] 80%+ code coverage for new modules
- [ ] Documentation covers all new features
- [ ] Extension installs cleanly on new VS Code
