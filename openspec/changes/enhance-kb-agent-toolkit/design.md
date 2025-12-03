# Design: Enhanced KB Agent Toolkit - Full System Integration Architecture

## Context

The Unity KB Query extension must evolve from an isolated search tool to a **unified development assistant** deeply integrated with the project's knowledge management systems:

### Systems to Integrate

| System | Location | Purpose |
|--------|----------|---------|
| **Serena Memories** | `.serena/memories/` | 5-tier hierarchical knowledge base |
| **Serena Personas** | `.serena/personas/` | 13 module-specific AI schemas |
| **Serena Symbols** | `.serena/symbols/` | Assembly/module/folder mappings |
| **OpenSpec** | `.openspec/` | Spec-driven development workflow |
| **Project Brain** | `.project-brain/` | Auto-generated state and context |
| **Unity KB** | `unity_project_kb` | 77,914 indexed code symbols |
| **Unity MCP** | Port 8081 | Editor automation |
| **Critical Rules** | `.serena/ai/critical.llm.txt` | P0 constraints |

### Constraints
- Must work when any system is unavailable (graceful degradation)
- Must not add significant latency (<100ms for interactive operations)
- Must respect file organization rules (ZERO ROOT POLLUTION)
- Must preserve existing extension behavior
- Token budget awareness (optimize memory loading)

### Stakeholders
- **Primary**: AI assistants (Claude Code) for autonomous development
- **Secondary**: Human developers for knowledge exploration
- **Tertiary**: Project maintainers for system health monitoring

## Goals / Non-Goals

### Goals
1. Unify all knowledge sources under single extension interface
2. Enable context-aware memory loading with 40-50% token savings
3. Provide OpenSpec workflow support (create, implement, archive)
4. Leverage persona schemas for intelligent code generation
5. Surface project health and active issues from Project Brain
6. Support end-to-end autonomous development workflows

### Non-Goals
1. Replace Serena MCP server (we integrate, not replace)
2. Modify source files of integrated systems
3. Implement custom embedding models
4. Support multiple projects simultaneously

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                              VS Code Extension Host                                  │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                      │
│  ┌──────────────────────────────────────────────────────────────────────────────┐   │
│  │                           Integration Layer                                   │   │
│  ├──────────────────────────────────────────────────────────────────────────────┤   │
│  │                                                                              │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐            │   │
│  │  │   Serena    │ │  OpenSpec   │ │   Project   │ │   Unity     │            │   │
│  │  │ Integration │ │ Integration │ │    Brain    │ │     KB      │            │   │
│  │  ├─────────────┤ ├─────────────┤ ├─────────────┤ ├─────────────┤            │   │
│  │  │ • Memories  │ │ • Proposals │ │ • state.json│ │ • Hybrid    │            │   │
│  │  │ • Personas  │ │ • Specs     │ │ • Context   │ │   Search    │            │   │
│  │  │ • Symbols   │ │ • Archives  │ │ • Health    │ │ • 77,914    │            │   │
│  │  │ • Critical  │ │ • Validate  │ │ • Tasks     │ │   Symbols   │            │   │
│  │  └──────┬──────┘ └──────┬──────┘ └──────┬──────┘ └──────┬──────┘            │   │
│  │         │               │               │               │                   │   │
│  └─────────┼───────────────┼───────────────┼───────────────┼───────────────────┘   │
│            │               │               │               │                        │
│            └───────────────┴───────────────┴───────────────┘                        │
│                                    │                                                 │
│                                    ▼                                                 │
│  ┌──────────────────────────────────────────────────────────────────────────────┐   │
│  │                          Unified Context Engine                               │   │
│  ├──────────────────────────────────────────────────────────────────────────────┤   │
│  │                                                                              │   │
│  │  • Intent Detection (from file, cursor, selection)                          │   │
│  │  • Memory Selection (which CRITICAL files to load)                          │   │
│  │  • Persona Mapping (current file → persona schema)                          │   │
│  │  • Proposal Conflict Check (active changes affecting file)                  │   │
│  │  • Token Budget Management (track loaded context)                           │   │
│  │                                                                              │   │
│  └─────────────────────────────────────┬────────────────────────────────────────┘   │
│                                        │                                            │
│                                        ▼                                            │
│  ┌──────────────────────────────────────────────────────────────────────────────┐   │
│  │                          Agent Workflow Engine                                │   │
│  ├──────────────────────────────────────────────────────────────────────────────┤   │
│  │                                                                              │   │
│  │  • Create Proposal (KB → patterns → scaffolding)                            │   │
│  │  • Implement Change (proposal → memories → KB → code)                       │   │
│  │  • Archive Change (validate → update specs → move)                          │   │
│  │  • Generate Code (persona + memory + spec → validated code)                 │   │
│  │                                                                              │   │
│  └─────────────────────────────────────┬────────────────────────────────────────┘   │
│                                        │                                            │
│                                        ▼                                            │
│  ┌──────────────────────────────────────────────────────────────────────────────┐   │
│  │                              MCP Bridge                                       │   │
│  ├──────────────────────────────────────────────────────────────────────────────┤   │
│  │                                                                              │   │
│  │  MCP Tools Exposed:                                                          │   │
│  │  • mcp__kb-toolkit__search_with_context                                     │   │
│  │  • mcp__kb-toolkit__load_memory_tier                                        │   │
│  │  • mcp__kb-toolkit__get_persona                                             │   │
│  │  • mcp__kb-toolkit__check_proposal_conflicts                                │   │
│  │  • mcp__kb-toolkit__generate_code                                           │   │
│  │  • mcp__kb-toolkit__get_project_state                                       │   │
│  │                                                                              │   │
│  └──────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                      │
└──────────────────────────────────────────────────────────────────────────────────────┘
                    │                   │                   │
                    ▼                   ▼                   ▼
        ┌───────────────────┐ ┌─────────────────┐ ┌─────────────────┐
        │   File System     │ │   Qdrant KB     │ │   Unity MCP     │
        │ .serena/          │ │ localhost:6333  │ │   Port 8081     │
        │ .openspec/        │ │ unity_project_kb│ │                 │
        │ .project-brain/   │ │                 │ │                 │
        └───────────────────┘ └─────────────────┘ └─────────────────┘
```

## Decisions

### Decision 1: Serena Memory Tier Loading Strategy

**What**: Implement smart memory loading based on file context and intent detection

**Why**:
- Loading all memories wastes 5000+ tokens
- Task-specific loading achieves 40-50% savings
- Follows established `.serena/memories/_INDEX.md` patterns

**Implementation**:
```typescript
interface MemoryLoadStrategy {
  // Tier priority (always load in this order)
  tiers: ['CRITICAL', 'TOOLS', 'INTEGRATION', 'CONTEXT', 'AUTO_GENERATED'];

  // Intent → required memories mapping
  intentMap: {
    'character-movement': ['CRITICAL/001', 'CRITICAL/002'],
    'rpc-implementation': ['CRITICAL/003', 'CRITICAL/005'],
    'visual-scripting': ['CRITICAL/004'],
    'inventory-sync': ['CRITICAL/001'],
    'spawn-workflow': ['CRITICAL/006'],
  };

  // Always load first (P0)
  alwaysLoad: [
    '.serena/ai/critical.llm.txt',      // ~1000 tokens
    '.project-brain/core/state.json',   // ~2000 tokens
  ];
}

// Context-aware loading
async function loadMemoriesForContext(context: EditorContext): Promise<Memory[]> {
  const memories: Memory[] = [];

  // 1. Always load P0
  memories.push(await loadCriticalRules());
  memories.push(await loadProjectState());

  // 2. Detect intent from file
  const intent = detectIntent(context.filePath, context.symbols);

  // 3. Load intent-specific memories
  const requiredFiles = intentMap[intent] || [];
  for (const file of requiredFiles) {
    memories.push(await loadMemory(`.serena/memories/${file}.md`));
  }

  // 4. Track token budget
  const totalTokens = memories.reduce((sum, m) => sum + m.tokenCount, 0);
  console.log(`Loaded ${memories.length} memories (~${totalTokens} tokens)`);

  return memories;
}
```

**Alternatives Considered**:
- Load all memories: Too expensive (15000+ tokens)
- Manual selection: Poor UX, error-prone
- No memory loading: Loses critical context

### Decision 2: Persona Schema Auto-Detection

**What**: Automatically detect and apply relevant persona based on current file's namespace/assembly

**Why**:
- 13 personas cover all GameCreator modules
- Each persona has module-specific rules and 100+ symbols
- Reduces cognitive load on developer

**Implementation**:
```typescript
interface PersonaMapping {
  // Namespace → persona file mapping
  namespaceToPersona: {
    'GameCreator.Runtime.Characters': 'character.llm.txt',
    'GameCreator.Runtime.Inventory': 'inventory.llm.txt',
    'GameCreator.Runtime.Stats': 'stats.llm.txt',
    'GameCreator.Runtime.Perception': 'perception.llm.txt',
    'GameCreator.Runtime.Behavior': 'behavior.llm.txt',
    'GameCreator.Runtime.Quests': 'quests.llm.txt',
    'GameCreator.Runtime.Dialogue': 'dialogue.llm.txt',
    'GameCreator.Runtime.Variables': 'variables.llm.txt',
    'NinjutsuGames.Runtime.Factions': 'factions.llm.txt',
    'GameCreator.Multiplayer.Runtime': 'netcode-core.llm.txt',
    // ... more mappings
  };

  // Assembly → persona file mapping (fallback)
  assemblyToPersona: {
    'GameCreator.Runtime.Inventory': 'inventory.llm.txt',
    'MLCreator_Multiplayer.Runtime': 'character.llm.txt',
  };
}

async function detectPersona(filePath: string): Promise<Persona | null> {
  // Parse file for namespace
  const content = await readFile(filePath);
  const namespace = extractNamespace(content);

  // Try namespace mapping first
  if (namespaceToPersona[namespace]) {
    return await loadPersona(namespaceToPersona[namespace]);
  }

  // Fall back to assembly mapping
  const assembly = getAssemblyForFile(filePath);
  if (assemblyToPersona[assembly]) {
    return await loadPersona(assemblyToPersona[assembly]);
  }

  return null;
}
```

### Decision 3: OpenSpec Proposal Browser

**What**: Provide browsing, conflict detection, and workflow support for OpenSpec changes

**Why**:
- 4 baseline specs and 20+ archived changes provide patterns
- Active proposals may conflict with current edits
- Archive mining enables learning from past work

**Implementation**:
```typescript
interface OpenSpecIntegration {
  // Directory structure
  paths: {
    specs: '.openspec/specs/',
    changes: '.openspec/changes/',
    archive: '.openspec/changes/archive/',
    project: '.openspec/project.md',
    agents: '.openspec/AGENTS.md',
  };

  // Proposal structure
  proposalFiles: ['proposal.md', 'tasks.md', 'design.md', 'specs/'];
}

// Conflict detection
async function checkProposalConflicts(filePath: string): Promise<Conflict[]> {
  const conflicts: Conflict[] = [];

  // Get active proposals
  const proposals = await listActiveProposals();

  for (const proposal of proposals) {
    const affectedFiles = await getAffectedFiles(proposal);

    if (affectedFiles.includes(filePath)) {
      conflicts.push({
        proposal: proposal.id,
        reason: `File is affected by active proposal: ${proposal.title}`,
        proposalPath: proposal.path,
      });
    }
  }

  return conflicts;
}

// Archive mining for patterns
async function findSimilarChanges(description: string): Promise<ArchivedChange[]> {
  const archives = await listArchivedChanges();

  // Semantic search through archived proposals
  const matches = await semanticSearch(description, archives.map(a => a.proposal));

  return matches.slice(0, 5);
}
```

### Decision 4: Symbol Schema Navigation

**What**: Provide navigation through assemblies.json, gamecreator_modules.json, and folder_structure.json

**Why**:
- 18 assemblies mapped with namespaces
- 10 GameCreator modules with sync components and dependencies
- Folder structure enables file placement validation

**Implementation**:
```typescript
// From .serena/symbols/assemblies.json
interface AssemblySchema {
  'gamecreator-core': string[];
  'gamecreator-module': string[];
  'multiplayer': string[];
  'ai-integration': string[];
  'project-core': string[];
  'testing': string[];
}

// From .serena/symbols/gamecreator_modules.json
interface GameCreatorModule {
  name: string;           // "Inventory"
  assembly: string;       // "GameCreator.Runtime.Inventory"
  syncComponent: string;  // "NetworkInventorySync"
  dependencies: string[]; // ["Variables", "Stats"]
}

// Navigation helpers
async function navigateToAssembly(assemblyName: string): Promise<void> {
  const assemblies = await loadAssemblies();
  const namespaces = findNamespaces(assemblies, assemblyName);

  // Open KB query filtered to this assembly
  await executeCommand('unity-kb.advancedQuery', {
    assembly: assemblyName,
    namespaces: namespaces,
  });
}

async function getModuleDependencies(moduleName: string): Promise<string[]> {
  const modules = await loadModules();
  const module = modules.find(m => m.name === moduleName);
  return module?.dependencies || [];
}
```

### Decision 5: Project Brain State Dashboard

**What**: Live dashboard showing state.json content with health indicators

**Why**:
- Single source of truth for project status
- Surfaces warnings and issues proactively
- Provides task context recommendations

**Implementation**:
```typescript
interface ProjectState {
  projectInfo: {
    name: string;
    unity: string;
    netcode: string;
  };
  health: {
    status: 'healthy' | 'warning' | 'error';
    issues: Issue[];
    lastCheck: string;
  };
  systems: {
    kb: { status: string; symbolCount: number };
    mcp: { status: string };
    compilation: { status: string; errors: number };
  };
  taskContext: {
    currentTask: string;
    recommendedMemories: string[];
  };
}

// Dashboard webview
function createStateDashboard(): vscode.WebviewPanel {
  const panel = vscode.window.createWebviewPanel(
    'projectState',
    'Project State',
    vscode.ViewColumn.Two,
    { enableScripts: true }
  );

  // Watch for state changes
  const watcher = vscode.workspace.createFileSystemWatcher(
    new vscode.RelativePattern(
      vscode.workspace.workspaceFolders![0],
      '.project-brain/core/state.json'
    )
  );

  watcher.onDidChange(() => updateDashboard(panel));

  return panel;
}
```

### Decision 6: Unified MCP Tool Exposure

**What**: Expose all integrated capabilities as MCP tools for Claude Code

**Why**:
- Enables autonomous agent workflows
- Consistent with existing Unity MCP pattern
- Allows tool chaining for complex operations

**MCP Tools**:
```typescript
const mcpTools = [
  {
    name: 'mcp__kb-toolkit__search_with_context',
    description: 'Search KB with automatic context from current file, persona, and memories',
    parameters: {
      query: { type: 'string', description: 'Search query' },
      useContext: { type: 'boolean', default: true },
      persona: { type: 'string', optional: true },
    },
  },
  {
    name: 'mcp__kb-toolkit__load_memory_tier',
    description: 'Load specific memory tier or file',
    parameters: {
      tier: { enum: ['CRITICAL', 'TOOLS', 'INTEGRATION', 'CONTEXT', 'AUTO_GENERATED'] },
      file: { type: 'string', optional: true },
    },
  },
  {
    name: 'mcp__kb-toolkit__get_persona',
    description: 'Get persona schema for module or auto-detect from file',
    parameters: {
      module: { type: 'string', optional: true },
      filePath: { type: 'string', optional: true },
    },
  },
  {
    name: 'mcp__kb-toolkit__check_proposal_conflicts',
    description: 'Check if file is affected by active OpenSpec proposals',
    parameters: {
      filePath: { type: 'string' },
    },
  },
  {
    name: 'mcp__kb-toolkit__generate_code',
    description: 'Generate code using persona, memories, and spec validation',
    parameters: {
      template: { type: 'string', description: 'Generation template' },
      context: { type: 'object', description: 'Context for generation' },
      validateAgainstSpec: { type: 'string', optional: true },
    },
  },
  {
    name: 'mcp__kb-toolkit__get_project_state',
    description: 'Get current project state from Project Brain',
    parameters: {},
  },
  {
    name: 'mcp__kb-toolkit__browse_openspec',
    description: 'Browse OpenSpec proposals, specs, or archives',
    parameters: {
      type: { enum: ['proposals', 'specs', 'archives'] },
      filter: { type: 'string', optional: true },
    },
  },
];
```

## Risks / Trade-offs

### Risk 1: File System Performance
- **Risk**: Reading many files for context loading could be slow
- **Mitigation**: Cache file contents, use file watchers for invalidation
- **Detection**: Monitor load times, alert if >100ms

### Risk 2: State.json Staleness
- **Risk**: state.json may be outdated if Project Brain not running
- **Mitigation**: Check timestamp, warn if >1 hour old
- **Detection**: Show age indicator in dashboard

### Risk 3: OpenSpec Parsing Errors
- **Risk**: Malformed proposal.md or spec.md could cause errors
- **Mitigation**: Graceful error handling, skip malformed files
- **Detection**: Log parse errors, show warning in browser

### Risk 4: Memory Token Overflow
- **Risk**: Loading too many memories exceeds context limit
- **Mitigation**: Token budget tracking, warn when approaching limit
- **Detection**: Display token count, hard cap at 8000 tokens

### Risk 5: Persona Mismatch
- **Risk**: Auto-detected persona may be wrong for cross-module files
- **Mitigation**: Allow manual override, show detected persona for confirmation
- **Detection**: User feedback loop

## Migration Plan

### Phase 1: Read-Only Integration
1. Add Serena memory browser (read-only)
2. Add OpenSpec proposal viewer (read-only)
3. Add state.json dashboard (read-only)
4. Add persona detection (display only)

### Phase 2: Context-Aware Features
1. Implement smart memory loading
2. Add persona-boosted KB search
3. Add conflict detection warnings
4. Enable token budget tracking

### Phase 3: Workflow Automation
1. Implement MCP tool exposure
2. Add code generation with validation
3. Enable end-to-end workflows
4. Add archive mining for patterns

### Rollback
- Each phase can be disabled via settings
- Falls back to basic KB search if integrations fail
- No modifications to source systems

## Open Questions

1. **Memory format**: Should we parse .md files or .llm.txt files for token counting?
   - Leaning toward: Parse both, different strategies per format

2. **Persona caching**: How long to cache persona detection results?
   - Leaning toward: Until file is modified

3. **OpenSpec CLI integration**: Should we shell out to `openspec` CLI or parse files directly?
   - Leaning toward: Parse files directly for speed

4. **State refresh**: How often to poll state.json for changes?
   - Leaning toward: File watcher, no polling
