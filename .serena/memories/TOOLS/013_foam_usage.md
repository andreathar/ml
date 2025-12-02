# Foam Usage Guide

## Purpose
Foam is a personal knowledge management and sharing system built on VSCode and Markdown. It enables interconnected note-taking with wikilinks and graph visualization.

## Configuration
**File**: `.foam/foam.config.json`
- Includes: `foam/`, `docs/`, `*.md`, `Assets/**/*.cs`
- Excludes: Unity binaries, Library/, generated files

## Directory Structure
```
foam/
├── daily/          # Daily notes
├── projects/       # Project documentation
├── systems/        # System architecture docs
├── assets/         # Asset documentation
├── research/       # Research notes
└── archive/        # Archived content
```

## When to Use
- Documenting new features or systems
- Creating daily development notes
- Building knowledge connections
- Researching and note-taking

## Core Concepts

### Wikilinks
Connect notes using `[[double brackets]]`:
```markdown
This feature uses [[NetworkPlayerController]] for multiplayer.
See also: [[GameCreator Integration]]
```

### Tags
Categorize notes with `#hashtags`:
```markdown
#multiplayer #gamecreator #networking
```

### Foam Tags (Code References)
Special tags in code comments:
```csharp
// #foam [[NetworkPlayerController]] [[Multiplayer.Components]]
```

## Output Patterns

### Graph Visualization
- Nodes: Individual notes
- Edges: Wikilink connections
- Clusters: Related topic groups

### Backlinks Panel
Shows all notes linking to current note:
```
Backlinks to [[NetworkPlayerController]]:
- [[Multiplayer Architecture]] (2 references)
- [[Player Spawning]] (1 reference)
```

## AI Interpretation Guide

### Navigating Knowledge Graph
1. **Start with main topic note**
2. **Follow wikilinks** to related concepts
3. **Check backlinks** for usage context
4. **Use tags** to find related topics

### Understanding Note Relationships
| Link Type | Meaning |
|-----------|---------|
| `[[Note]]` | Direct reference |
| `[[Note#section]]` | Section reference |
| `[[Note\|alias]]` | Aliased reference |

### Finding Information
```markdown
# To find all multiplayer docs:
- Search: #multiplayer
- Browse: foam/systems/multiplayer/

# To understand a component:
- Find: [[ComponentName]]
- Check backlinks for usage
- Follow related links
```

### Foam in MLCreator
The project has 10,095+ auto-generated documentation files:
- C# API documentation
- GameCreator component docs
- System architecture notes

## VSCode Integration
- Extension: `Foam`
- Graph view: Ctrl+Shift+G
- Create note from selection: Ctrl+Shift+N
- Show backlinks: Sidebar panel

## Daily Notes
Template at `.foam/templates/daily-note.md`:
```markdown
# {{date}}

## Tasks
- [ ] Task 1

## Notes

## Links
```

## Best Practices
1. **Use descriptive titles**: `[[Network Player Spawning]]` not `[[Spawning]]`
2. **Link liberally**: More connections = better discovery
3. **Tag consistently**: Use established tags
4. **Update regularly**: Keep notes current

## Troubleshooting
| Issue | Solution |
|-------|----------|
| Links not resolving | Check file exists, spelling |
| Graph not updating | Refresh Foam extension |
| Too many nodes | Filter by tag or folder |
| Slow performance | Reduce include patterns |
