# Change: Enhance KB Agent Toolkit - Full Serena & OpenSpec Integration

## Why

The current `unity-kb-query` VS Code extension is a minimal proof-of-concept disconnected from the powerful systems already in place:

### Current State Problems

1. **Isolated from Serena Memory System** - Cannot access the 5-tier memory hierarchy (CRITICAL, CORE, INTEGRATION, TOOLS, AUTO_GENERATED) that provides context-aware knowledge
2. **No Persona Schema Awareness** - 13 GameCreator personas with 77,914+ indexed symbols are inaccessible programmatically
3. **Missing OpenSpec Integration** - Cannot query active proposals, archived changes, or baseline specs
4. **No Symbol Schema Loading** - The rich `assemblies.json`, `gamecreator_modules.json`, and `folder_structure.json` are unused
5. **Disconnected from Project Brain** - Cannot leverage `state.json` for project health, active warnings, or task context maps
6. **Manual Query Building** - No understanding of project-specific patterns or coding standards

### What We Have (Underutilized)

| System | Location | Capability |
|--------|----------|------------|
| **Serena Memories** | `.serena/memories/` | 5-tier hierarchical knowledge (CRITICAL→AUTO_GENERATED) |
| **Persona Schemas** | `.serena/personas/` | 13 module-specific AI personas with symbol counts |
| **Symbol Schemas** | `.serena/symbols/` | 18 assemblies, 10 GC modules, folder structure |
| **OpenSpec** | `.openspec/` | 4 baseline specs, 20+ archived changes, active proposals |
| **Project Brain** | `.project-brain/` | Auto-generated state, health monitoring, task context |
| **KB Index** | `unity_project_kb` | 77,914 indexed symbols with hybrid vectors |
| **Critical Rules** | `.serena/ai/critical.llm.txt` | P0 rules (~1000 tokens) |

By integrating the VS Code extension with these systems, AI agents can:
- Load context-appropriate memories before coding
- Understand module dependencies via persona schemas
- Query active OpenSpec proposals to avoid conflicts
- Generate code following established patterns from archived changes
- Validate implementations against baseline specs
- Access the full 77,914 symbol knowledge base intelligently

## What Changes

### Phase 1: Serena Memory Integration

#### 1.1 Memory Tier Browser
- **Tree view** showing all memory tiers with file counts and load status
- **Smart preloading** based on current file context (e.g., editing NetworkCharacterAdapter → load CRITICAL/001, CRITICAL/002)
- **Token budget tracking** showing loaded memories vs. estimated tokens
- **Memory search** across all tiers with highlighted snippets

#### 1.2 Persona Schema Loading
- **Persona selector** dropdown in advanced query (Character, Inventory, Stats, etc.)
- **Auto-detect persona** from current file's namespace/assembly
- **Symbol count display** showing persona coverage (e.g., "Character: 190 symbols")
- **Dependency graph** showing persona relationships

#### 1.3 Critical Rules Panel
- **P0 rules sidebar** showing critical.llm.txt content
- **Rule violation warnings** when editing code that conflicts with P0 rules
- **Quick reference** for file organization, RPC naming, Task signatures

### Phase 2: OpenSpec Integration

#### 2.1 Proposal Browser
- **Active proposals panel** showing all `changes/` directories
- **Proposal quick view** with Why/What/Impact summary
- **Task checklist viewer** with completion status
- **Conflict detection** when editing files affected by active proposals

#### 2.2 Spec Query
- **Baseline spec search** across `specs/` capabilities
- **Requirement lookup** by scenario or requirement text
- **Spec validation** against current code
- **Delta preview** for active change proposals

#### 2.3 Archive Mining
- **Archived changes browser** for learning from past work
- **Pattern extraction** from successful implementations
- **Similar change finder** based on current task description

### Phase 3: Symbol Schema Integration

#### 3.1 Assembly Navigator
- **Assembly tree view** from `assemblies.json` (18 assemblies)
- **Namespace browser** with symbol counts
- **Cross-assembly search** with dependency awareness
- **Assembly filter presets** (GameCreator-only, Multiplayer-only, etc.)

#### 3.2 GameCreator Module Browser
- **Module cards** from `gamecreator_modules.json` (10 modules)
- **Sync component lookup** (e.g., Inventory → NetworkInventorySync)
- **Dependency chain** visualization
- **Module-aware code generation**

#### 3.3 Folder Structure Awareness
- **Project map** from `folder_structure.json`
- **File placement suggestions** based on file organization rules
- **Auto-redirect warnings** for root folder violations

### Phase 4: Project Brain Integration

#### 4.1 State Dashboard
- **Live state.json viewer** showing project health
- **Warning badges** for active issues
- **System status** (KB, MCP servers, compilation)
- **Task context** showing recommended memory loads

#### 4.2 Context-Aware Loading
- **Intent detection** from user actions
- **Task context map** execution
- **Token budget optimization** (40-50% savings)
- **Predictive preloading** based on file patterns

### Phase 5: Unified Agent Workflows

#### 5.1 OpenSpec-Driven Workflows
- **"Create Proposal" workflow**: Query KB → analyze patterns → generate proposal scaffolding
- **"Implement Change" workflow**: Load proposal → load memories → query KB → generate code
- **"Archive Change" workflow**: Validate completion → update specs → move to archive

#### 5.2 Memory-Assisted Code Generation
- **Pattern-from-memory**: Use CRITICAL rules when generating code
- **Persona-guided generation**: Apply module-specific patterns
- **Spec-compliant code**: Validate against baseline specs before insertion

#### 5.3 Full Integration Pipeline
```
User Intent Detection
    ↓
Load Project State (.project-brain/state.json)
    ↓
Load Critical Rules (.serena/ai/critical.llm.txt)
    ↓
Detect Relevant Persona (.serena/personas/)
    ↓
Load Task-Specific Memories (.serena/memories/CRITICAL/)
    ↓
Check Active OpenSpec Proposals (.openspec/changes/)
    ↓
Query KB with Context (unity_project_kb)
    ↓
Generate/Validate Code
    ↓
Apply via Unity MCP
```

## Impact

### Affected Specs
- **NEW**: `kb-agent-toolkit` - Full specification for the integrated toolkit
- **MODIFIED**: Aligns with `add-kb-query-window` Unity Editor window proposal

### Affected Systems

| System | Integration Type | Changes |
|--------|------------------|---------|
| **Serena Memories** | Read | Memory browser, preloading, search |
| **Serena Personas** | Read | Persona detection, symbol lookup |
| **Serena Symbols** | Read | Assembly/module navigation |
| **OpenSpec** | Read/Parse | Proposal browser, spec query, archive mining |
| **Project Brain** | Read | State dashboard, context loading |
| **Unity KB** | Query | Enhanced hybrid search with context |
| **Unity MCP** | Execute | Automated edits from KB patterns |

### New Files

```
vscode-extensions/unity-kb-query/src/
├── integrations/
│   ├── serenaMemories.ts      # Memory tier loading & browsing
│   ├── serenaPersonas.ts      # Persona schema detection & loading
│   ├── serenaSymbols.ts       # Assembly/module navigation
│   ├── openspecBrowser.ts     # Proposal/spec/archive browser
│   ├── openspecValidator.ts   # Spec compliance checking
│   ├── projectBrainState.ts   # State.json dashboard
│   └── projectBrainContext.ts # Context-aware loading
├── workflows/
│   ├── createProposal.ts      # OpenSpec proposal generation
│   ├── implementChange.ts     # Memory-assisted implementation
│   └── fullPipeline.ts        # End-to-end agent workflow
└── views/
    ├── memoryBrowser.ts       # Memory tier tree view
    ├── personaSelector.ts     # Persona dropdown
    ├── proposalPanel.ts       # Active proposals view
    └── stateDashboard.ts      # Project Brain dashboard
```

### Non-Breaking Changes
- All existing commands preserved
- New features are additive
- Graceful degradation when systems unavailable

### Performance Targets

| Metric | Current | Target | With Integration |
|--------|---------|--------|------------------|
| Context loading | Manual | Automatic | 40-50% token savings |
| KB query relevance | 60% | 95% | Persona-aware boosting |
| Code generation accuracy | N/A | 90% | Memory + spec validation |
| Proposal conflict detection | N/A | 100% | OpenSpec integration |

## Success Criteria

1. **Memory Integration**: Can browse and load any memory tier from extension
2. **Persona Detection**: Automatically detects and applies relevant persona for current file
3. **OpenSpec Awareness**: Shows active proposals and prevents conflicting edits
4. **Symbol Navigation**: Can navigate assemblies/modules from `symbols/` schemas
5. **State Dashboard**: Displays project health from state.json
6. **Unified Workflow**: End-to-end flow from intent to implementation in <30 seconds
7. **Token Optimization**: Achieves 40-50% token savings via smart memory loading

## KB Analysis Results

Based on analysis of 77,914 indexed symbols:

### Existing Patterns Found
- **Memory system**: 5 tiers, 25+ files, structured hierarchy
- **Persona schemas**: 13 modules, 1,500+ symbols mapped
- **OpenSpec archive**: 20+ completed changes with proven patterns
- **Symbol schemas**: 18 assemblies, 10 modules, complete folder map

### Recommended Approach
- Use established Serena memory loading patterns
- Leverage persona schemas for context detection
- Mine OpenSpec archive for code generation templates
- Integrate with Project Brain for state awareness
