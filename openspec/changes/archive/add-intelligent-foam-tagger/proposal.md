# Change: Intelligent Foam Tagger with Relationship Intelligence

## Why

**Critical Bug**: FoamBatchTagger fails to tag any files due to path separator bug causing `Assets\Assets\` duplication on Windows, preventing the tool from functioning at all.

**10x Enhancement Opportunity**: Transform Foam tagging from simple keyword-based tagging into an intelligent relationship tracking system that:
- Understands file dependencies and cross-references
- Detects GameCreator integration patterns and compliance
- Predicts change impact across the codebase
- Provides architectural health monitoring
- Integrates with Serena memories for AI assistant context
- Generates visual dependency diagrams
- Creates bidirectional Foam wiki links automatically

**Value Proposition**: AI assistants and developers currently have no automated way to understand relationships between files, GameCreator integration points, or change impact. This enhancement provides 10x value by adding intelligent analysis that transforms documentation from static tags to dynamic relationship tracking.

## What Changes

### Immediate (Phase 1 - Critical Bug Fix)
- **BREAKING**: None
- Fix path separator normalization in `ApplyTagsToFile()` method (line 560)
- Add cross-platform path handling utilities
- Test on Windows/Mac/Linux file systems

### Phase 2: Roslyn Symbol Analysis (Week 1)
- Add Microsoft.CodeAnalysis.CSharp NuGet package (editor-only)
- Create `SymbolAnalyzer` class for parsing C# files
- Extract: classes, methods, properties, using statements, attributes, inheritance
- Build symbol graph data structure

### Phase 3: Dependency Intelligence (Week 2)
- Implement `DependencyGraphBuilder` for file relationship tracking
- Track 3+ levels of dependencies across assemblies
- Generate JSON export of relationship data
- Create Mermaid diagram generator for visualization
- Auto-generate bidirectional Foam wiki links ([[FileName]])

### Phase 4: GameCreator Pattern Detection (Week 3)
- Create `GameCreatorPatternDetector` for integration analysis
- Detect 15+ patterns: Character API usage, NetworkBehaviour patterns, Visual Scripting
- Implement compliance scoring algorithm (0-100)
- Generate integration health reports per file

### Phase 5: Advanced Intelligence (Week 4)
- Build `ImpactAnalyzer` for change prediction (<1 sec response)
- Implement orphan detection for unused files/symbols
- Create architecture boundary validator (cross-assembly rules)
- Integrate with Serena memories (`.serena/memories/TOOLS/`, `.serena/memories/ARCHITECTURE/`)
- Generate weekly health monitoring reports

### Phase 6: UI/UX Enhancement (Week 5)
- Add multi-tab EditorWindow: Tagging, Dependency Graph, Health Monitor, Integration Map
- Create interactive dependency graph viewer
- Add one-click report generation
- Support export formats: JSON, Mermaid, Markdown

## Impact

### Affected Specs
- **NEW**: `tooling/foam-integration` - Core capability for intelligent tagging

### Affected Code
- `Assets/Editor/FoamBatchTagger.cs` - Complete enhancement (560 lines → ~1500 lines)
- **NEW**: `Assets/Editor/Foam/SymbolAnalyzer.cs` (~300 lines)
- **NEW**: `Assets/Editor/Foam/DependencyGraphBuilder.cs` (~250 lines)
- **NEW**: `Assets/Editor/Foam/GameCreatorPatternDetector.cs` (~200 lines)
- **NEW**: `Assets/Editor/Foam/ImpactAnalyzer.cs` (~150 lines)
- `.serena/memories/TOOLS/foam_dependency_graph.md` - Auto-generated
- `.serena/memories/ARCHITECTURE/gamecreator_integration_map.md` - Auto-generated
- `foam-schemas/` - Schema format extension for relationship data

### Affected Systems
- Unity Editor tools
- Foam knowledge management integration
- Serena MCP memory system
- Project Brain state generation
- OpenSpec workflow (uses Foam for documentation)

### Dependencies
- Microsoft.CodeAnalysis.CSharp (NuGet, editor-only, ~15MB)
- No runtime dependencies (editor tooling only)

### Migration Required
- None (backward compatible - existing tags preserved)
- Optional: Run migration tool to upgrade old tags to new format

### Timeline
- Phase 1 (Critical): 30 minutes - Immediate deployment
- Phases 2-6: 5 weeks, ~34 hours total effort
- Incremental value delivery after each phase

### Success Metrics (10x Validation)
1. ✅ Relationship Depth: 0 → 3+ levels of dependency tracking
2. ✅ Symbol Accuracy: Manual → 95%+ automated extraction
3. ✅ Integration Intelligence: 0 → 15+ GameCreator patterns detected
4. ✅ Change Impact: Unknown → <1 sec prediction with affected file list
5. ✅ Health Visibility: 0% → 100% file coverage with compliance scores
6. ✅ Cross-File Links: 0 → N automatic bidirectional Foam links
7. ✅ Orphan Detection: Manual → Automated unused file/symbol identification
8. ✅ Architecture Validation: None → Real-time assembly boundary checks
9. ✅ Documentation: Manual → Automated relationship diagram generation
10. ✅ AI Context: 0 → Full Serena memory integration for instant context

### Risk Assessment
- **Performance**: Medium risk - Mitigate with async processing, incremental updates
- **Complexity**: Medium risk - Mitigate with phased rollout, user feedback
- **Maintenance**: High risk - Mitigate with good documentation, unit tests, modular design
- **False Positives**: Medium risk - Mitigate with manual review mode, confidence scores

### Breaking Changes
- **NONE** - All enhancements are additive and backward compatible
