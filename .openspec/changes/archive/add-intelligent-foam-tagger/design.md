# Design Document: Intelligent Foam Tagger

## Context

The FoamBatchTagger is currently a simple keyword-based tagging tool that fails on Windows due to a path separator bug and provides limited value beyond basic categorization. This enhancement transforms it into an intelligent relationship tracking system that provides 10x value through deep code analysis, dependency mapping, GameCreator integration detection, and architectural health monitoring.

### Background
- **Current State**: Simple pattern matching for tags (#unity, #netcode, etc.)
- **Problem**: No relationship understanding, no integration intelligence, no change impact analysis
- **Opportunity**: Transform into project intelligence engine for AI assistants and developers

### Constraints
- Must remain editor-only tool (no runtime impact)
- Must maintain backward compatibility with existing tags
- Must handle large codebases (1000+ files) without freezing Unity Editor
- Must integrate with existing Serena memory system
- Must use standard Markdown format for AI readability

### Stakeholders
- **Primary**: AI assistants (Claude Code, GitHub Copilot) needing project context
- **Secondary**: Developers needing change impact analysis and architecture validation
- **Tertiary**: Project managers tracking technical debt and compliance metrics

## Goals / Non-Goals

### Goals
1. ✅ **Fix Critical Bug**: Resolve path separator issue immediately (Phase 1)
2. ✅ **Deep Symbol Analysis**: Extract 95%+ symbols accurately using Roslyn
3. ✅ **Relationship Tracking**: Build 3+ level dependency graphs
4. ✅ **GameCreator Intelligence**: Detect 15+ integration patterns with compliance scoring
5. ✅ **Change Impact**: Predict affected files in <1 second
6. ✅ **Health Monitoring**: Provide real-time architecture validation
7. ✅ **AI Integration**: Feed Serena memories with relationship data
8. ✅ **Visualization**: Generate Mermaid diagrams and interactive graphs
9. ✅ **Incremental Value**: Each phase delivers usable functionality
10. ✅ **Maintainability**: Modular architecture, well-documented, testable

### Non-Goals
- ❌ Runtime code analysis (editor-only)
- ❌ Real-time background analysis (on-demand only)
- ❌ Integration with external tools (Foam, Obsidian, etc.) - export only
- ❌ AI-powered code generation or refactoring suggestions
- ❌ Version control integration (git blame, history analysis)
- ❌ Performance profiling or runtime metrics
- ❌ Code quality scoring beyond GameCreator compliance

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    FoamBatchTagger UI                       │
│  ┌─────────┬──────────────┬──────────────┬────────────┐    │
│  │ Tagging │ Dependencies │ Health       │ Integration│    │
│  │ Tab     │ Graph Tab    │ Monitor Tab  │ Map Tab    │    │
│  └─────────┴──────────────┴──────────────┴────────────┘    │
└──────────────────────┬──────────────────────────────────────┘
                       │
       ┌───────────────┴───────────────┐
       │     Analysis Coordinator      │
       │  (Orchestrates all analyzers) │
       └───────────────┬───────────────┘
                       │
       ┌───────────────┴────────────────────────────┐
       │              │              │               │
┌──────▼──────┐ ┌────▼────┐ ┌──────▼──────┐ ┌─────▼─────┐
│   Symbol    │ │Dependency│ │  GameCreator│ │  Impact   │
│  Analyzer   │ │  Graph   │ │   Pattern   │ │ Analyzer  │
│  (Roslyn)   │ │ Builder  │ │  Detector   │ │           │
└──────┬──────┘ └────┬─────┘ └──────┬──────┘ └─────┬─────┘
       │             │               │               │
       │             └───────┬───────┘               │
       │                     │                       │
       └─────────────────────┼───────────────────────┘
                             │
                ┌────────────▼────────────┐
                │   Data Exporters        │
                │  ┌─────────────────┐   │
                │  │ JSON            │   │
                │  │ Mermaid         │   │
                │  │ Markdown        │   │
                │  │ Serena Memory   │   │
                │  └─────────────────┘   │
                └─────────────────────────┘
```

## Key Design Decisions

### Decision 1: Use Roslyn for C# Analysis
**Rationale**: Roslyn provides accurate, battle-tested C# parsing with full semantic understanding
**Alternatives Considered**:
- Regex-based parsing: Fast but brittle, high error rate
- Custom parser: High maintenance burden, reinventing the wheel
- Roslyn (CHOSEN): Accurate, maintained by Microsoft, industry standard

**Trade-offs**:
- ✅ PRO: 95%+ accuracy, handles all C# features
- ✅ PRO: Future-proof as C# evolves
- ❌ CON: ~15MB package size (acceptable for editor-only)
- ❌ CON: Learning curve for team

**Implementation**: Add as editor-only NuGet package, async processing to avoid UI freeze

### Decision 2: Build Dependency Graph In-Memory
**Rationale**: In-memory graph provides fast querying for impact analysis (<1 sec requirement)
**Alternatives Considered**:
- Database storage (SQLite): Overkill, slower for graph queries
- File-based cache: I/O bottleneck, serialization overhead
- In-memory graph (CHOSEN): Fast, simple, sufficient for <10k files

**Trade-offs**:
- ✅ PRO: Fast queries, simple implementation
- ✅ PRO: No external dependencies
- ❌ CON: Memory usage (~100MB for 1000 files)
- ❌ CON: Rebuild required on project changes

**Implementation**: Use Dictionary<string, FileNode> for O(1) lookups, lazy loading for large projects

### Decision 3: Pattern-Based GameCreator Detection
**Rationale**: Codify known patterns as rules, easier to maintain and extend
**Alternatives Considered**:
- Machine learning: Overkill, training data burden
- Heuristics: Inconsistent, hard to explain
- Pattern rules (CHOSEN): Explicit, testable, maintainable

**Trade-offs**:
- ✅ PRO: Deterministic, explainable results
- ✅ PRO: Easy to add new patterns
- ❌ CON: Requires pattern discovery upfront
- ❌ CON: May miss novel integration approaches

**Implementation**: JSON configuration file with pattern definitions, scoring weights

### Decision 4: Markdown Format for Serena Memories
**Rationale**: AI assistants consume Markdown natively, human-readable
**Alternatives Considered**:
- JSON: Machine-readable but less human-friendly
- Custom format: High maintenance
- Markdown (CHOSEN): AI-native, human-readable, extensible

**Trade-offs**:
- ✅ PRO: AI assistants read without parsing
- ✅ PRO: Human-readable in version control
- ✅ PRO: Supports tables, lists, code blocks
- ❌ CON: Slightly larger file size than JSON

**Implementation**: Template-based generation with sections: Summary, Dependencies, Patterns, Health

### Decision 5: Phased Rollout (6 Phases)
**Rationale**: Incremental value delivery, user feedback between phases, risk mitigation
**Alternatives Considered**:
- Big bang: High risk, long wait for value
- Two phases (fix + enhancement): Less flexibility
- Six phases (CHOSEN): Balance between granularity and overhead

**Trade-offs**:
- ✅ PRO: Immediate bug fix (Phase 1)
- ✅ PRO: Feedback-driven iteration
- ✅ PRO: Can stop if ROI insufficient
- ❌ CON: More coordination overhead
- ❌ CON: Longer total timeline

**Implementation**: Each phase deployable independently, feature flags for beta testing

### Decision 6: Multi-Tab UI Instead of Separate Windows
**Rationale**: Unified tool experience, less window management
**Alternatives Considered**:
- Separate windows per feature: Cluttered, hard to coordinate
- Single scrolling view: Too dense, cognitive overload
- Multi-tab window (CHOSEN): Progressive disclosure, clean UX

**Trade-offs**:
- ✅ PRO: Clean interface, progressive disclosure
- ✅ PRO: Single window management
- ❌ CON: Can't view multiple tabs simultaneously

**Implementation**: Use Unity UI Toolkit TabView, lazy-load tab content

## Data Models

### FileNode
```csharp
public class FileNode
{
    public string RelativePath;              // Assets/Scripts/Foo.cs
    public string FullPath;                  // D:/Project/Assets/Scripts/Foo.cs
    public List<SymbolNode> Symbols;         // Classes, methods, properties
    public List<string> Dependencies;        // Files this depends on
    public List<string> Dependents;          // Files that depend on this
    public List<string> Tags;                // Foam tags: #unity, #netcode
    public Dictionary<string, string> FoamLinks;  // [[FileName]] -> path
    public int HealthScore;                  // 0-100 compliance score
    public List<PatternMatch> Patterns;      // Detected GameCreator patterns
}
```

### SymbolNode
```csharp
public class SymbolNode
{
    public string Name;                      // ClassName or MethodName
    public SymbolType Type;                  // Class, Method, Property, Field
    public string Namespace;                 // GameCreator.Runtime.Characters
    public string Assembly;                  // GameCreator.Runtime
    public Location Location;                // File path + line number
    public List<string> References;          // Other symbols this references
    public List<string> ReferencedBy;        // Other symbols that reference this
    public List<string> Attributes;          // [ServerRpc], [ClientRpc]
    public string Signature;                 // Full method signature
}
```

### PatternMatch
```csharp
public class PatternMatch
{
    public string PatternId;                 // "character-api-usage"
    public string PatternName;               // "Character API Usage"
    public PatternCategory Category;         // GameCreator, Network, Performance
    public int Weight;                       // 1-10 importance
    public Location Location;                // Where pattern was found
    public string Evidence;                  // Code snippet showing pattern
    public float Confidence;                 // 0.0-1.0 detection confidence
}
```

### DependencyGraph
```csharp
public class DependencyGraph
{
    public Dictionary<string, FileNode> Nodes;    // File path -> FileNode
    public List<DependencyEdge> Edges;            // File dependencies
    public DateTime GeneratedAt;                  // Timestamp
    public ProjectMetadata Metadata;              // Project info

    // Query methods
    public List<FileNode> GetDependencies(string path, int depth = 3);
    public List<FileNode> GetDependents(string path, int depth = 3);
    public List<FileNode> DetectCircularDependencies();
    public List<FileNode> DetectOrphans();
}
```

## Component Descriptions

### 1. SymbolAnalyzer (Roslyn-based)
**Purpose**: Parse C# files to extract symbols with semantic accuracy

**Key Responsibilities**:
- Parse C# source using Roslyn CSharpSyntaxTree
- Extract classes, methods, properties, fields
- Extract using statements for dependency analysis
- Extract attributes for pattern detection
- Build SymbolNode data structure

**Performance Requirements**:
- Parse 100 files/second on typical hardware
- Async processing to avoid UI freeze
- Progress reporting every 10 files

**Error Handling**:
- Skip malformed files, log warning
- Continue processing remaining files
- Report parsing errors in summary

### 2. DependencyGraphBuilder
**Purpose**: Build and maintain file dependency graph

**Key Responsibilities**:
- Traverse all project files using SymbolAnalyzer
- Resolve using statement references to file paths
- Build bidirectional dependency edges
- Detect circular dependencies
- Identify orphaned files

**Performance Requirements**:
- Build graph for 1000 files in <30 seconds
- Support incremental updates (changed files only)
- Memory efficient: <100MB for 1000 files

**Query Optimization**:
- Cache frequently accessed paths
- Use Dictionary for O(1) lookups
- Lazy-load full symbol data

### 3. GameCreatorPatternDetector
**Purpose**: Detect and score GameCreator integration patterns

**Key Responsibilities**:
- Load pattern definitions from configuration
- Match patterns against symbol data
- Calculate compliance scores (0-100)
- Generate pattern usage reports
- Flag anti-patterns and violations

**Pattern Categories**:
- Character API patterns (5 patterns)
- Network synchronization patterns (4 patterns)
- Visual scripting patterns (3 patterns)
- Performance patterns (3 patterns)

**Scoring Algorithm**:
```
FileScore = Sum(PatternWeight * MatchConfidence) / Sum(PossibleWeight)
ProjectScore = Average(FileScores weighted by LOC)
```

### 4. ImpactAnalyzer
**Purpose**: Predict change impact across codebase

**Key Responsibilities**:
- Given file path, traverse dependency graph
- Identify all dependents up to 3 levels deep
- Calculate impact score based on coupling
- Generate impact report with affected files
- Sort by risk/impact severity

**Performance Requirements**:
- Analysis in <1 second for typical file
- Support batch analysis for multiple files
- Real-time updates as graph changes

**Impact Scoring**:
```
ImpactScore = Sum(1 / (depth ^ 2)) for each dependent
Risk = ImpactScore * FileCoupling * FileImportance
```

### 5. Data Exporters
**Purpose**: Export analysis results to various formats

**Key Responsibilities**:
- JSON: Full data export for tooling
- Mermaid: Graph visualization diagrams
- Markdown: Serena memory integration
- CSV: Spreadsheet analysis

**Serena Memory Format**:
```markdown
# Foam Dependency Graph

**Generated**: 2025-01-20 16:30:00
**Files Analyzed**: 247
**Dependencies Found**: 1,432

## Top 10 Most Depended-On Files
| File | Dependents | Assembly |
|------|------------|----------|
| Character.cs | 42 | GameCreator.Runtime |
...

## Cross-Assembly Dependencies
...

## Orphaned Files
...
```

## Risks and Mitigations

### Risk 1: Performance Degradation on Large Codebases
**Impact**: High - Tool unusable if slow
**Likelihood**: Medium - Depends on optimization

**Mitigations**:
1. Async processing with cancellation support
2. Progress bars and status updates
3. Incremental updates (changed files only)
4. Memory profiling and optimization
5. Batch processing with configurable chunk size
6. Background thread execution

**Acceptance Criteria**: 1000 files analyzed in <30 seconds

### Risk 2: False Positives in Pattern Detection
**Impact**: Medium - Misleading compliance scores
**Likelihood**: Medium - Pattern matching is imperfect

**Mitigations**:
1. Confidence scores (0.0-1.0) per match
2. Manual review mode for low-confidence matches
3. Pattern definition validation and testing
4. User feedback mechanism for false positives
5. Continuous pattern refinement based on data

**Acceptance Criteria**: <5% false positive rate

### Risk 3: Roslyn Package Size Impact
**Impact**: Low - Acceptable for editor-only
**Likelihood**: High - Package is ~15MB

**Mitigations**:
1. Editor-only dependency (no runtime impact)
2. Document size impact in release notes
3. Provide option to disable advanced features
4. Consider on-demand package download

**Acceptance Criteria**: <20MB total size impact

### Risk 4: Complexity Overwhelms Users
**Impact**: Medium - Tool not adopted
**Likelihood**: Medium - Many features added

**Mitigations**:
1. Progressive disclosure (multi-tab UI)
2. Simple default mode (just tagging)
3. Advanced features behind "Show Advanced" toggle
4. Good documentation with screenshots
5. Video tutorial for complex features
6. Tooltips and help text throughout

**Acceptance Criteria**: 80%+ user satisfaction in beta

### Risk 5: Maintenance Burden
**Impact**: High - Tool becomes unmaintainable
**Likelihood**: Medium - Complex codebase

**Mitigations**:
1. Modular architecture (each analyzer independent)
2. Comprehensive unit tests (80%+ coverage)
3. Clear code documentation and comments
4. Separation of concerns (UI, analysis, export)
5. Design patterns: Strategy, Factory, Builder
6. Code review requirements

**Acceptance Criteria**: <4 hours/week maintenance after launch

## Migration Plan

### Phase 1: Immediate Bug Fix (No Migration)
- Deploy fix directly
- No user action required
- Existing tags preserved

### Phase 2-6: Backward Compatible
- Tool detects old tag format
- Automatically upgrades to new format on save
- Original tags preserved with `<!-- legacy -->` marker
- User can opt-out of auto-upgrade

### Rollback Plan
If critical issues arise:
1. Revert to previous version via git
2. Disable new features via feature flags
3. Notify users of temporary rollback
4. Fix issues in separate branch
5. Re-deploy after validation

### Data Migration
**Old Format**:
```csharp
/*
 * Foam Tags: #unity #csharp #networking
 */
```

**New Format** (backward compatible):
```csharp
/*
 * Foam Tags: #unity #csharp #networking
 * Dependencies: [[NetworkCharacterAdapter]] [[Character]]
 * Compliance: 85/100
 * Patterns: character-api-usage, networkvariable-pattern
 */
```

## Open Questions

1. **Q**: Should we auto-run analysis on project load or require manual trigger?
   **A**: Manual trigger by default, option to enable auto-run in settings

2. **Q**: How often should weekly health monitoring run?
   **A**: Configurable, default: Sunday 2am local time

3. **Q**: Should we detect patterns in third-party assets (GameCreator, Netcode)?
   **A**: No, focus on user code only to reduce noise

4. **Q**: How to handle generated code (Unity's auto-generated files)?
   **A**: Skip files with `// Auto-generated` header, configurable exclusion list

5. **Q**: Should compliance scores be public (git committed) or private (local only)?
   **A**: Export to `claudedocs/reports/` (gitignored), opt-in for commit

6. **Q**: Integration with Unity Asset Database for real-time updates?
   **A**: Phase 7 (future enhancement), use AssetPostprocessor hook

## Success Criteria

### Phase 1 Success
- ✅ Bug fixed, all files tagged successfully
- ✅ Zero path-related errors in logs
- ✅ Backward compatible with existing tags

### Phase 2 Success
- ✅ 95%+ symbol extraction accuracy
- ✅ Parses 100 files/second
- ✅ Zero Unity Editor freezes

### Phase 3 Success
- ✅ Dependency graph built for 1000 files in <30s
- ✅ Mermaid diagrams generated correctly
- ✅ Foam links bidirectional and accurate

### Phase 4 Success
- ✅ 15+ patterns detected correctly
- ✅ <5% false positive rate
- ✅ Compliance scores match manual review within 10%

### Phase 5 Success
- ✅ Impact analysis in <1 second
- ✅ Orphan detection 100% accurate
- ✅ Serena memories generated in correct format

### Phase 6 Success
- ✅ Multi-tab UI intuitive and responsive
- ✅ All export formats functional
- ✅ 80%+ user satisfaction

### Overall Success (10x Validation)
- ✅ All 10 success metrics from proposal achieved
- ✅ AI assistants report improved context understanding
- ✅ Developers use tool weekly for impact analysis
- ✅ Technical debt tracked and trending downward

## Future Enhancements (Phase 7+)

1. **Real-Time Analysis**: AssetPostprocessor integration for automatic updates
2. **AI-Powered Suggestions**: Claude-generated refactoring recommendations
3. **Historical Trending**: Track compliance scores over time with git integration
4. **Team Dashboard**: Web-based dashboard for project-wide metrics
5. **IDE Integration**: VSCode extension for in-editor relationship viewing
6. **Automated Testing**: Generate unit tests based on dependency analysis
7. **Performance Profiling**: Integrate with Unity Profiler for hotspot detection
8. **Documentation Generation**: Auto-generate API docs from symbol analysis
