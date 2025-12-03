# Implementation Tasks: Intelligent Foam Tagger

## Phase 1: Critical Bug Fix (Immediate - 30 mins)

### 1.1 Path Normalization Fix
- [ ] 1.1.1 Fix `ApplyTagsToFile()` line 560 - replace path separator handling
- [ ] 1.1.2 Add `NormalizePath()` utility method for cross-platform path handling
- [ ] 1.1.3 Update `GetRelativePath()` to use normalized separators
- [ ] 1.1.4 Test path handling on Windows (backslash)
- [ ] 1.1.5 Test path handling on Mac/Linux (forward slash)
- [ ] 1.1.6 Add unit tests for path normalization edge cases
- [ ] 1.1.7 Deploy fix immediately to unblock existing users

## Phase 2: Roslyn Symbol Analysis (Week 1 - 4 hours)

### 2.1 Setup Roslyn Infrastructure
- [ ] 2.1.1 Add Microsoft.CodeAnalysis.CSharp NuGet package to project
- [ ] 2.1.2 Configure package as editor-only dependency
- [ ] 2.1.3 Verify package size impact (~15MB acceptable for editor)
- [ ] 2.1.4 Test package loading in Unity Editor

### 2.2 Create SymbolAnalyzer Class
- [ ] 2.2.1 Create `Assets/Editor/Foam/SymbolAnalyzer.cs`
- [ ] 2.2.2 Implement `ParseCSharpFile(string path)` using Roslyn
- [ ] 2.2.3 Extract using statements from syntax tree
- [ ] 2.2.4 Extract class definitions with namespace, inheritance, interfaces
- [ ] 2.2.5 Extract method signatures with parameters and return types
- [ ] 2.2.6 Extract properties and fields with types
- [ ] 2.2.7 Extract attributes (NetworkBehaviour, RPC, etc.)
- [ ] 2.2.8 Build SymbolNode data structure
- [ ] 2.2.9 Add unit tests for SymbolAnalyzer (10+ test cases)

### 2.3 Integrate with FoamBatchTagger
- [ ] 2.3.1 Update `AnalyzeCSharpFile()` to use SymbolAnalyzer
- [ ] 2.3.2 Store symbol data in file analysis results
- [ ] 2.3.3 Add symbol count to UI display
- [ ] 2.3.4 Test end-to-end symbol extraction

## Phase 3: Dependency Intelligence (Week 2 - 6 hours)

### 3.1 Create Dependency Graph Builder
- [ ] 3.1.1 Create `Assets/Editor/Foam/DependencyGraphBuilder.cs`
- [ ] 3.1.2 Implement `FileNode` class: {path, symbols[], dependencies[], dependents[], tags[]}
- [ ] 3.1.3 Implement `BuildGraph()` method to traverse all files
- [ ] 3.1.4 Analyze using statements to build dependency edges
- [ ] 3.1.5 Track class inheritance relationships
- [ ] 3.1.6 Track interface implementation relationships
- [ ] 3.1.7 Support 3+ levels of dependency depth
- [ ] 3.1.8 Detect circular dependencies and flag warnings

### 3.2 JSON Export Functionality
- [ ] 3.2.1 Implement `ExportToJson()` method for dependency graph
- [ ] 3.2.2 Create JSON schema for graph data structure
- [ ] 3.2.3 Add file-level metadata: LOC, symbol count, tag count
- [ ] 3.2.4 Export to `foam-schemas/dependency-graph.json`
- [ ] 3.2.5 Add timestamp and version metadata

### 3.3 Mermaid Diagram Generation
- [ ] 3.3.1 Create `MermaidGenerator.cs` for graph visualization
- [ ] 3.3.2 Implement `GenerateClassDiagram()` for file relationships
- [ ] 3.3.3 Implement `GenerateFlowchart()` for dependency flow
- [ ] 3.3.4 Add color coding by assembly/module
- [ ] 3.3.5 Export to `claudedocs/diagrams/foam-dependency-graph.mmd`
- [ ] 3.3.6 Add UI button to generate and view diagram

### 3.4 Foam Wiki Link Generation
- [ ] 3.4.1 Implement `GenerateFoamLinks()` for bidirectional references
- [ ] 3.4.2 Create [[FileName]] format links for direct dependencies
- [ ] 3.4.3 Create [[ClassName]] format links for symbol references
- [ ] 3.4.4 Add "Related Files" section to Foam tags
- [ ] 3.4.5 Add "Depends On" and "Depended By" lists
- [ ] 3.4.6 Update existing Foam tags with cross-references

## Phase 4: GameCreator Pattern Detection (Week 3 - 8 hours)

### 4.1 Create Pattern Detector
- [ ] 4.1.1 Create `Assets/Editor/Foam/GameCreatorPatternDetector.cs`
- [ ] 4.1.2 Define 15+ detection patterns as configuration
- [ ] 4.1.3 Implement pattern matching using Roslyn symbol data

### 4.2 Character API Pattern Detection
- [ ] 4.2.1 Detect `GameCreator.Runtime.Characters.Character` usage
- [ ] 4.2.2 Detect Character property access: Motion, Jump, Driver, IsNetworkOwner
- [ ] 4.2.3 Detect Character method calls: TryGetComponent, Get, Set
- [ ] 4.2.4 Score compliance with invasive integration pattern (0-100)

### 4.3 NetworkBehaviour Pattern Detection
- [ ] 4.3.1 Detect NetworkBehaviour inheritance
- [ ] 4.3.2 Detect RPC methods: [ServerRpc], [ClientRpc]
- [ ] 4.3.3 Validate RPC naming convention: EndsWith("ServerRpc" or "ClientRpc")
- [ ] 4.3.4 Detect NetworkVariable usage: NetworkVariable<T>
- [ ] 4.3.5 Validate NetworkVariable naming: m_[Name] + public property

### 4.4 Visual Scripting Pattern Detection
- [ ] 4.4.1 Detect VisualScripting namespace usage
- [ ] 4.4.2 Detect `Task Run(Args args)` method signature
- [ ] 4.4.3 Detect GameCreator Instruction/Condition classes
- [ ] 4.4.4 Validate visual scripting integration patterns

### 4.5 Additional Pattern Detection
- [ ] 4.5.1 Detect Supabase integration patterns
- [ ] 4.5.2 Detect ML-Agents patterns
- [ ] 4.5.3 Detect performance-critical code patterns
- [ ] 4.5.4 Detect editor-only code patterns

### 4.6 Compliance Scoring
- [ ] 4.6.1 Implement scoring algorithm (0-100 per file)
- [ ] 4.6.2 Weight patterns by importance: Critical, High, Medium, Low
- [ ] 4.6.3 Generate per-file compliance report
- [ ] 4.6.4 Generate project-wide compliance summary
- [ ] 4.6.5 Add compliance score to UI display

### 4.7 Integration Health Reports
- [ ] 4.7.1 Create report template: Markdown format
- [ ] 4.7.2 Generate per-file reports with detected patterns
- [ ] 4.7.3 Generate project summary with statistics
- [ ] 4.7.4 Export to `claudedocs/reports/foam-integration-health.md`
- [ ] 4.7.5 Add UI button to view reports

## Phase 5: Advanced Intelligence (Week 4 - 10 hours)

### 5.1 Change Impact Analyzer
- [ ] 5.1.1 Create `Assets/Editor/Foam/ImpactAnalyzer.cs`
- [ ] 5.1.2 Implement `PredictImpact(string filePath)` method
- [ ] 5.1.3 Traverse dependency graph to find affected files
- [ ] 5.1.4 Include 3+ levels of transitive dependencies
- [ ] 5.1.5 Calculate impact score based on depth and coupling
- [ ] 5.1.6 Generate impact report in <1 second
- [ ] 5.1.7 Add UI panel: "What would changing this file affect?"
- [ ] 5.1.8 Integrate with Unity asset change detection

### 5.2 Orphan Detection
- [ ] 5.2.1 Implement `DetectOrphans()` for unused files
- [ ] 5.2.2 Identify files with zero dependents (unreferenced)
- [ ] 5.2.3 Identify symbols with zero references
- [ ] 5.2.4 Flag potential dead code (unused classes/methods)
- [ ] 5.2.5 Generate orphan report with recommendations
- [ ] 5.2.6 Add UI display for orphaned files

### 5.3 Architecture Boundary Validation
- [ ] 5.3.1 Load assembly boundary rules from project configuration
- [ ] 5.3.2 Detect cross-assembly dependencies
- [ ] 5.3.3 Flag violations of architectural rules
- [ ] 5.3.4 Generate boundary violation report
- [ ] 5.3.5 Add severity levels: Error, Warning, Info
- [ ] 5.3.6 Integrate with Unity console for real-time warnings

### 5.4 Serena Memory Integration
- [ ] 5.4.1 Create memory writer for `.serena/memories/TOOLS/`
- [ ] 5.4.2 Generate `foam_dependency_graph.md` with relationship data
- [ ] 5.4.3 Generate `foam_health_report.md` with compliance data
- [ ] 5.4.4 Create memory writer for `.serena/memories/ARCHITECTURE/`
- [ ] 5.4.5 Generate `gamecreator_integration_map.md` with pattern analysis
- [ ] 5.4.6 Use standard Markdown format for AI readability
- [ ] 5.4.7 Add metadata headers: timestamp, version, file count
- [ ] 5.4.8 Integrate with Serena memory update workflow

### 5.5 Weekly Health Monitoring
- [ ] 5.5.1 Create automated health check scheduler
- [ ] 5.5.2 Run full analysis on project-wide basis
- [ ] 5.5.3 Compare metrics week-over-week
- [ ] 5.5.4 Generate trend reports: compliance improving/degrading
- [ ] 5.5.5 Export to `claudedocs/reports/weekly-health-YYYY-MM-DD.md`
- [ ] 5.5.6 Add notifications for critical issues

## Phase 6: UI/UX Enhancement (Week 5 - 6 hours)

### 6.1 Multi-Tab Editor Window
- [ ] 6.1.1 Refactor FoamBatchTagger to use TabView
- [ ] 6.1.2 Create "Tagging" tab (existing functionality)
- [ ] 6.1.3 Create "Dependency Graph" tab (visualization)
- [ ] 6.1.4 Create "Health Monitor" tab (compliance scores)
- [ ] 6.1.5 Create "Integration Map" tab (GameCreator patterns)
- [ ] 6.1.6 Add tab icons and tooltips

### 6.2 Interactive Dependency Graph Viewer
- [ ] 6.2.1 Implement graph visualization using Unity UI Toolkit
- [ ] 6.2.2 Add node selection: click to highlight file
- [ ] 6.2.3 Add edge highlighting: show dependency paths
- [ ] 6.2.4 Add zoom and pan controls
- [ ] 6.2.5 Add filter controls: by assembly, by tag, by compliance
- [ ] 6.2.6 Add search box: find specific files/classes

### 6.3 One-Click Report Generation
- [ ] 6.3.1 Add "Generate Reports" button to UI
- [ ] 6.3.2 Generate all reports in single click
- [ ] 6.3.3 Show progress bar during generation
- [ ] 6.3.4 Open report folder when complete
- [ ] 6.3.5 Add report preview in UI

### 6.4 Export Functionality
- [ ] 6.4.1 Add export dropdown: JSON, Mermaid, Markdown, CSV
- [ ] 6.4.2 Implement JSON export with full data
- [ ] 6.4.3 Implement Mermaid export for visualization tools
- [ ] 6.4.4 Implement Markdown export for documentation
- [ ] 6.4.5 Implement CSV export for spreadsheet analysis
- [ ] 6.4.6 Add copy-to-clipboard functionality

## Testing and Documentation

### 7.1 Unit Tests
- [ ] 7.1.1 Write tests for SymbolAnalyzer (10+ cases)
- [ ] 7.1.2 Write tests for DependencyGraphBuilder (8+ cases)
- [ ] 7.1.3 Write tests for GameCreatorPatternDetector (15+ cases)
- [ ] 7.1.4 Write tests for ImpactAnalyzer (5+ cases)
- [ ] 7.1.5 Write tests for path normalization (6+ cases)
- [ ] 7.1.6 Achieve 80%+ code coverage

### 7.2 Integration Tests
- [ ] 7.2.1 Test full workflow: Scan → Analyze → Export → Report
- [ ] 7.2.2 Test on sample project with known structure
- [ ] 7.2.3 Test on MLCreator project (52 assemblies)
- [ ] 7.2.4 Test performance with 1000+ files
- [ ] 7.2.5 Test memory usage under load

### 7.3 Documentation
- [ ] 7.3.1 Update tool documentation in `claudedocs/guides/`
- [ ] 7.3.2 Create usage guide with screenshots
- [ ] 7.3.3 Document all 15+ pattern detection rules
- [ ] 7.3.4 Create troubleshooting guide
- [ ] 7.3.5 Add API documentation for extensibility
- [ ] 7.3.6 Create video tutorial (optional)

## Deployment

### 8.1 Phase 1 Immediate Deployment
- [ ] 8.1.1 Deploy bug fix to production immediately
- [ ] 8.1.2 Notify users of fix availability
- [ ] 8.1.3 Monitor for issues in first 24 hours

### 8.2 Phased Rollout
- [ ] 8.2.1 Deploy Phase 2 (Roslyn) to beta testers
- [ ] 8.2.2 Gather feedback, iterate
- [ ] 8.2.3 Deploy Phase 3 (Dependency) to beta testers
- [ ] 8.2.4 Gather feedback, iterate
- [ ] 8.2.5 Deploy Phase 4 (Patterns) to beta testers
- [ ] 8.2.6 Gather feedback, iterate
- [ ] 8.2.7 Deploy Phase 5 (Advanced) to beta testers
- [ ] 8.2.8 Gather feedback, iterate
- [ ] 8.2.9 Deploy Phase 6 (UI) to all users
- [ ] 8.2.10 Announce 10x enhancement completion
