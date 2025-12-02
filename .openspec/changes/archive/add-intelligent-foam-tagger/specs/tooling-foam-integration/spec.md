# Specification: Foam Integration Tooling

## ADDED Requirements

### Requirement: Cross-Platform Path Handling
The FoamBatchTagger SHALL correctly handle file paths on Windows, macOS, and Linux operating systems, normalizing path separators to prevent duplication or file-not-found errors.

#### Scenario: Windows path with backslashes
- **WHEN** a file path contains Windows-style backslashes (e.g., `Assets\Core\Foo.cs`)
- **THEN** the path SHALL be normalized to forward slashes
- **AND** the path SHALL not contain duplicate `Assets\Assets\` segments
- **AND** the file SHALL be found and tagged successfully

#### Scenario: Unix path with forward slashes
- **WHEN** a file path contains Unix-style forward slashes (e.g., `Assets/Core/Foo.cs`)
- **THEN** the path SHALL be used as-is without modification
- **AND** the file SHALL be found and tagged successfully

#### Scenario: Mixed path separators
- **WHEN** a file path contains mixed separators (e.g., `Assets\Core/Foo.cs`)
- **THEN** the path SHALL be normalized to consistent forward slashes
- **AND** the file SHALL be found and tagged successfully

### Requirement: Roslyn-Based Symbol Analysis
The system SHALL use Microsoft Roslyn (Microsoft.CodeAnalysis.CSharp) to parse C# source files and extract symbols with semantic accuracy of 95% or higher.

#### Scenario: Extract class definitions
- **WHEN** analyzing a C# file containing class definitions
- **THEN** the system SHALL extract class names, namespaces, and inheritance hierarchy
- **AND** the system SHALL identify implemented interfaces
- **AND** symbol extraction accuracy SHALL be ≥95%

#### Scenario: Extract method signatures
- **WHEN** analyzing a C# file containing methods
- **THEN** the system SHALL extract method names, parameters, return types, and attributes
- **AND** the system SHALL distinguish between public, private, protected, and internal methods
- **AND** symbol extraction accuracy SHALL be ≥95%

#### Scenario: Extract using statements
- **WHEN** analyzing a C# file with using directives
- **THEN** the system SHALL extract all using statements
- **AND** the system SHALL resolve namespaces to assembly references
- **AND** extraction SHALL complete without errors

#### Scenario: Handle malformed files
- **WHEN** analyzing a C# file with syntax errors
- **THEN** the system SHALL log a warning message
- **AND** the system SHALL continue processing remaining files
- **AND** the error SHALL be reported in the analysis summary

#### Scenario: Performance requirement
- **WHEN** analyzing 100 C# files on typical hardware
- **THEN** parsing SHALL complete in ≤1 second
- **AND** the UI SHALL remain responsive during analysis

### Requirement: Dependency Graph Construction
The system SHALL build a directed graph of file dependencies by analyzing using statements, class inheritance, and symbol references, supporting depth of 3 or more levels.

#### Scenario: Build file dependency graph
- **WHEN** analyzing a project with multiple interconnected files
- **THEN** the system SHALL create a graph where nodes represent files
- **AND** edges SHALL represent dependency relationships
- **AND** the graph SHALL support 3+ levels of dependency depth

#### Scenario: Resolve dependencies from using statements
- **WHEN** a file contains `using GameCreator.Runtime.Characters;`
- **THEN** the system SHALL identify all files in that namespace
- **AND** the system SHALL create dependency edges to those files
- **AND** dependencies SHALL be stored in FileNode.Dependencies list

#### Scenario: Track bidirectional relationships
- **WHEN** File A depends on File B
- **THEN** File A's Dependencies list SHALL contain File B
- **AND** File B's Dependents list SHALL contain File A
- **AND** both relationships SHALL be queryable in O(1) time

#### Scenario: Detect circular dependencies
- **WHEN** File A depends on File B and File B depends on File A (directly or transitively)
- **THEN** the system SHALL detect the circular dependency
- **AND** the system SHALL log a warning with the dependency cycle
- **AND** both files SHALL be flagged in the health report

#### Scenario: Performance requirement for graph construction
- **WHEN** building a dependency graph for 1000 files
- **THEN** construction SHALL complete in ≤30 seconds
- **AND** memory usage SHALL be ≤100MB

### Requirement: GameCreator Pattern Detection
The system SHALL detect 15 or more GameCreator integration patterns and calculate compliance scores (0-100) based on adherence to invasive integration best practices.

#### Scenario: Detect Character API usage
- **WHEN** a file uses `Character.Motion`, `Character.Jump`, or other Character API members
- **THEN** the system SHALL detect the "character-api-usage" pattern
- **AND** the pattern SHALL be stored with file location and evidence
- **AND** confidence score SHALL be 0.9 or higher for exact matches

#### Scenario: Detect NetworkBehaviour patterns
- **WHEN** a class inherits from `NetworkBehaviour`
- **THEN** the system SHALL detect the "networkbehaviour-inheritance" pattern
- **AND** the system SHALL validate RPC method naming conventions
- **AND** violations SHALL reduce the compliance score

#### Scenario: Detect RPC method patterns
- **WHEN** a method has `[ServerRpc]` attribute but doesn't end with "ServerRpc"
- **THEN** the system SHALL detect a naming convention violation
- **AND** the violation SHALL be flagged in the pattern match
- **AND** the file's compliance score SHALL be reduced by the pattern weight

#### Scenario: Detect NetworkVariable patterns
- **WHEN** a field is declared as `NetworkVariable<T>`
- **THEN** the system SHALL detect the "networkvariable-usage" pattern
- **AND** the system SHALL validate naming convention: `m_[Name]` for field, `[Name]` for property
- **AND** violations SHALL reduce the compliance score

#### Scenario: Calculate file compliance score
- **WHEN** all patterns have been detected for a file
- **THEN** the compliance score SHALL be calculated as: Sum(PatternWeight * MatchConfidence) / Sum(PossibleWeight)
- **AND** the score SHALL be in the range [0, 100]
- **AND** the score SHALL be stored in FileNode.HealthScore

#### Scenario: Generate pattern detection report
- **WHEN** pattern detection completes
- **THEN** the system SHALL generate a Markdown report listing all detected patterns
- **AND** the report SHALL include per-file pattern matches with locations
- **AND** the report SHALL include project-wide compliance summary
- **AND** the report SHALL be exported to `claudedocs/reports/foam-integration-health.md`

### Requirement: Change Impact Prediction
The system SHALL analyze the dependency graph to predict which files would be affected by changes to a given file, completing analysis in less than 1 second.

#### Scenario: Predict immediate dependents
- **WHEN** a user requests impact analysis for File A
- **THEN** the system SHALL identify all files directly dependent on File A
- **AND** results SHALL be returned in <1 second
- **AND** results SHALL include file paths and dependency depth (1)

#### Scenario: Predict transitive dependents
- **WHEN** a user requests impact analysis with depth=3
- **THEN** the system SHALL identify all dependents up to 3 levels deep
- **AND** results SHALL include dependency path for each affected file
- **AND** results SHALL be sorted by impact score (descending)

#### Scenario: Calculate impact score
- **WHEN** calculating impact for a file change
- **THEN** impact score SHALL be: Sum(1 / (depth ^ 2)) for each dependent
- **AND** risk score SHALL incorporate file coupling and importance metrics
- **AND** scores SHALL be normalized to [0, 100] range

#### Scenario: Performance requirement for impact analysis
- **WHEN** analyzing impact for a file with 50 dependents across 3 levels
- **THEN** analysis SHALL complete in <1 second
- **AND** memory allocation SHALL be minimal (no graph rebuild)

### Requirement: Orphan File Detection
The system SHALL identify files and symbols that have zero references from other parts of the codebase, flagging potential dead code or unused assets.

#### Scenario: Detect orphaned files
- **WHEN** a file has zero entries in its Dependents list
- **THEN** the file SHALL be flagged as potentially orphaned
- **AND** the file SHALL appear in the orphan detection report
- **AND** the report SHALL include file path and last modification date

#### Scenario: Detect orphaned symbols
- **WHEN** a class or method has zero entries in its ReferencedBy list
- **THEN** the symbol SHALL be flagged as potentially orphaned
- **AND** the symbol SHALL appear in the orphan detection report
- **AND** the report SHALL include symbol name, type, and location

#### Scenario: Exclude entry points from orphan detection
- **WHEN** a class is marked as a Unity MonoBehaviour or has `[InitializeOnLoad]` attribute
- **THEN** the class SHALL NOT be flagged as orphaned
- **AND** public methods in such classes SHALL NOT be flagged as orphaned
- **AND** the exclusion SHALL be documented in the report

### Requirement: Architecture Boundary Validation
The system SHALL validate that cross-assembly dependencies comply with architectural rules defined in project configuration, flagging violations in real-time.

#### Scenario: Detect cross-assembly dependencies
- **WHEN** File A in Assembly X references File B in Assembly Y
- **THEN** the system SHALL record the cross-assembly dependency
- **AND** the dependency SHALL be validated against architecture rules
- **AND** the dependency SHALL appear in the cross-assembly dependency report

#### Scenario: Flag architecture violations
- **WHEN** a cross-assembly dependency violates configured rules
- **THEN** the system SHALL flag the violation with severity level (Error, Warning, Info)
- **AND** the violation SHALL include source file, target file, and violated rule
- **AND** the violation SHALL appear in the health monitor report

#### Scenario: Architecture rule configuration
- **WHEN** architecture rules are defined in project configuration
- **THEN** rules SHALL specify allowed/forbidden assembly relationships
- **AND** rules SHALL support glob patterns for flexible matching
- **AND** rules SHALL be validated on system startup

### Requirement: Serena Memory Integration
The system SHALL export analysis results to Serena MCP memory files in `.serena/memories/` directory using standard Markdown format for AI assistant consumption.

#### Scenario: Generate dependency graph memory
- **WHEN** analysis completes successfully
- **THEN** the system SHALL generate `foam_dependency_graph.md` in `.serena/memories/TOOLS/`
- **AND** the file SHALL contain: file list, dependency relationships, top depended-on files, orphaned files
- **AND** the file SHALL use Markdown tables for structured data

#### Scenario: Generate health report memory
- **WHEN** health monitoring completes
- **THEN** the system SHALL generate `foam_health_report.md` in `.serena/memories/TOOLS/`
- **AND** the file SHALL contain: compliance scores, detected patterns, violations, trend data
- **AND** the file SHALL use Markdown format with timestamp and metadata headers

#### Scenario: Generate integration map memory
- **WHEN** GameCreator pattern detection completes
- **THEN** the system SHALL generate `gamecreator_integration_map.md` in `.serena/memories/ARCHITECTURE/`
- **AND** the file SHALL contain: detected integration points, compliance scores, pattern usage statistics
- **AND** the file SHALL be AI-readable for context understanding

#### Scenario: Memory update workflow
- **WHEN** Serena memory files are updated
- **THEN** timestamp metadata SHALL be current
- **AND** version metadata SHALL be incremented
- **AND** existing files SHALL be backed up before overwrite

### Requirement: Mermaid Diagram Generation
The system SHALL generate Mermaid-format diagrams visualizing file dependencies, class hierarchies, and module relationships.

#### Scenario: Generate dependency flowchart
- **WHEN** user requests dependency diagram
- **THEN** the system SHALL generate a Mermaid flowchart showing file relationships
- **AND** the diagram SHALL use color coding by assembly/module
- **AND** the diagram SHALL be exported to `claudedocs/diagrams/foam-dependency-graph.mmd`

#### Scenario: Generate class diagram
- **WHEN** user requests class hierarchy diagram
- **THEN** the system SHALL generate a Mermaid class diagram showing inheritance and composition
- **AND** the diagram SHALL include class names, key methods, and relationships
- **AND** the diagram SHALL be limited to top 20 most important classes to prevent clutter

#### Scenario: Diagram readability
- **WHEN** a diagram contains >50 nodes
- **THEN** the system SHALL provide filtering options (by assembly, by tag)
- **AND** the system SHALL support zooming and panning for large diagrams
- **AND** the system SHALL warn user about diagram complexity

### Requirement: Automatic Foam Link Generation
The system SHALL automatically generate bidirectional Foam wiki links between related files based on dependency relationships.

#### Scenario: Generate dependency links
- **WHEN** File A depends on File B
- **THEN** File A's Foam tags SHALL include `[[File B]]` in Dependencies section
- **AND** File B's Foam tags SHALL include `[[File A]]` in Dependents section
- **AND** links SHALL use Foam wiki link format: `[[FileName]]`

#### Scenario: Generate symbol reference links
- **WHEN** File A references Class C defined in File B
- **THEN** File A's Foam tags SHALL include `[[Class C]]` in References section
- **AND** link SHALL resolve to File B in Foam workspace
- **AND** hover preview SHALL show Class C definition

#### Scenario: Update existing Foam tags
- **WHEN** a file already has Foam tags without dependency links
- **THEN** the system SHALL add a "Related Files" section
- **AND** the system SHALL add a "Dependencies" section with `[[...]]` links
- **AND** the system SHALL add a "Depended By" section with `[[...]]` links
- **AND** existing tags SHALL be preserved

### Requirement: Multi-Tab Editor Window UI
The system SHALL provide a Unity Editor window with multiple tabs for different analysis views: Tagging, Dependency Graph, Health Monitor, and Integration Map.

#### Scenario: Tab navigation
- **WHEN** user opens the Foam Batch Tagger window
- **THEN** the window SHALL display 4 tabs: Tagging, Dependency Graph, Health Monitor, Integration Map
- **AND** clicking a tab SHALL switch the active view
- **AND** tab content SHALL be lazy-loaded on first access

#### Scenario: Tagging tab (existing functionality)
- **WHEN** user switches to the Tagging tab
- **THEN** the tab SHALL display file scanning and tagging controls
- **AND** the tab SHALL show scan results and tag preview
- **AND** all existing functionality SHALL be preserved

#### Scenario: Dependency Graph tab
- **WHEN** user switches to the Dependency Graph tab
- **THEN** the tab SHALL display an interactive graph visualization
- **AND** the tab SHALL provide zoom, pan, and search controls
- **AND** clicking a node SHALL highlight dependencies and dependents

#### Scenario: Health Monitor tab
- **WHEN** user switches to the Health Monitor tab
- **THEN** the tab SHALL display compliance scores for all analyzed files
- **AND** the tab SHALL show detected patterns and violations
- **AND** the tab SHALL provide filtering and sorting options

#### Scenario: Integration Map tab
- **WHEN** user switches to the Integration Map tab
- **THEN** the tab SHALL display GameCreator integration points
- **AND** the tab SHALL show pattern usage statistics
- **AND** the tab SHALL highlight files with low compliance scores

### Requirement: Data Export Functionality
The system SHALL support exporting analysis results to multiple formats: JSON, Mermaid, Markdown, and CSV.

#### Scenario: Export to JSON
- **WHEN** user selects "Export to JSON"
- **THEN** the system SHALL serialize the full dependency graph to JSON format
- **AND** the JSON SHALL include: FileNodes, SymbolNodes, PatternMatches, metadata
- **AND** the file SHALL be saved to a user-specified location

#### Scenario: Export to Mermaid
- **WHEN** user selects "Export to Mermaid"
- **THEN** the system SHALL generate a Mermaid diagram file
- **AND** the diagram SHALL visualize file dependencies or class hierarchies
- **AND** the file SHALL be saved with `.mmd` extension

#### Scenario: Export to Markdown
- **WHEN** user selects "Export to Markdown"
- **THEN** the system SHALL generate a human-readable Markdown report
- **AND** the report SHALL include: summary, dependencies, patterns, health metrics
- **AND** the report SHALL use Markdown tables for structured data

#### Scenario: Export to CSV
- **WHEN** user selects "Export to CSV"
- **THEN** the system SHALL export tabular data to CSV format
- **AND** the CSV SHALL include: file paths, compliance scores, pattern counts, dependency counts
- **AND** the file SHALL be compatible with Excel and Google Sheets

### Requirement: Asynchronous Processing with Progress Reporting
The system SHALL perform all analysis operations asynchronously with UI progress reporting to prevent Unity Editor freezing.

#### Scenario: Async file scanning
- **WHEN** user initiates file scanning
- **THEN** the operation SHALL run on a background thread
- **AND** the UI SHALL remain responsive during scanning
- **AND** progress bar SHALL update every 10 files processed

#### Scenario: Cancellation support
- **WHEN** user clicks "Cancel" during analysis
- **THEN** the operation SHALL be cancelled gracefully
- **AND** partial results SHALL be preserved
- **AND** the system SHALL return to ready state

#### Scenario: Progress reporting
- **WHEN** a long-running operation is in progress
- **THEN** the UI SHALL display: operation name, current file, progress percentage
- **AND** estimated time remaining SHALL be displayed after 10% completion
- **AND** progress SHALL update at least every 500ms

### Requirement: Backward Compatibility with Existing Tags
The system SHALL maintain backward compatibility with existing Foam tags, automatically upgrading old format to new format while preserving original tags.

#### Scenario: Detect old tag format
- **WHEN** a file contains tags in old format: `/* Foam Tags: #unity #csharp */`
- **THEN** the system SHALL recognize the old format
- **AND** the system SHALL parse existing tags correctly
- **AND** the system SHALL mark for upgrade

#### Scenario: Auto-upgrade tags
- **WHEN** saving updated tags to a file with old format
- **THEN** the system SHALL add new sections: Dependencies, Compliance, Patterns
- **AND** the system SHALL preserve original tags
- **AND** the system SHALL add `<!-- upgraded from legacy format -->` marker

#### Scenario: Opt-out of auto-upgrade
- **WHEN** user disables auto-upgrade in settings
- **THEN** the system SHALL not modify files with old format
- **AND** the system SHALL still analyze files and generate reports
- **AND** export functionality SHALL work with old format

### Requirement: Performance and Scalability
The system SHALL handle large codebases with 1000+ files efficiently, completing full analysis in under 2 minutes on typical hardware.

#### Scenario: Large codebase performance
- **WHEN** analyzing a project with 1000 C# files
- **THEN** symbol analysis SHALL complete in ≤30 seconds
- **AND** dependency graph construction SHALL complete in ≤30 seconds
- **AND** pattern detection SHALL complete in ≤40 seconds
- **AND** report generation SHALL complete in ≤20 seconds
- **AND** total time SHALL be ≤2 minutes

#### Scenario: Memory efficiency
- **WHEN** analyzing 1000 files
- **THEN** peak memory usage SHALL be ≤100MB
- **AND** memory SHALL be released after analysis completes
- **AND** Unity Editor SHALL remain stable

#### Scenario: Incremental updates
- **WHEN** only 10 files have changed since last analysis
- **THEN** the system SHALL re-analyze only those 10 files
- **AND** the dependency graph SHALL be updated incrementally
- **AND** incremental analysis SHALL complete in <5 seconds
