# CSharpier Usage Guide

## Purpose
CSharpier is an opinionated C# code formatter (like Prettier for C#). It enforces consistent code style across the MLCreator project.

## Configuration
**File**: `.csharpierrc.json`
```json
{
  "printWidth": 120,
  "useTabs": false,
  "tabWidth": 4,
  "preprocessorSymbolSets": ["UNITY_EDITOR", "UNITY_STANDALONE", ...]
}
```

## When to Use
- Before committing code changes
- After refactoring or writing new code
- When reviewing PRs with style inconsistencies

## Output Patterns

### Success Output
```
Formatted 5 files in 1.23s
```

### No Changes Needed
```
0 files would be formatted
```

### Format Violations (Check Mode)
```
Error Assets/Scripts/MyClass.cs - Was not formatted
```

## AI Interpretation Guide

### Interpreting CSharpier Output
| Output | Meaning | Action |
|--------|---------|--------|
| `Formatted X files` | Files were reformatted | Review changes, commit |
| `0 files would be formatted` | All files already formatted | No action needed |
| `Error ... Was not formatted` | File needs formatting | Run CSharpier to fix |
| `Syntax error` | Invalid C# syntax | Fix syntax first |

### Common Formatting Changes
- **Line breaks**: Long lines split at 120 characters
- **Indentation**: Normalized to 4 spaces
- **Brace placement**: Allman style (braces on new line)
- **Spacing**: Consistent around operators and keywords

## VSCode Integration
- Extension: `CSharpier`
- Format on save: Enabled via VSCode settings
- Command: `Format Document` (Shift+Alt+F)

## CLI Commands
```bash
# Format all files
dotnet csharpier .

# Check without modifying
dotnet csharpier --check .

# Format specific file
dotnet csharpier Assets/Scripts/MyClass.cs
```

## Unity-Specific Notes
- Preprocessor symbols configured for Unity defines
- Does NOT format `.meta`, `.asset`, `.prefab` files
- Works with asmdef-separated assemblies

## False Positives & Cautions

### Unity-Specific False Positives
CSharpier may reformat code that should stay as-is:

| Pattern | Why It's Intentional | Action |
|---------|---------------------|--------|
| Using directive order | GameCreator requires specific order | Review changes |
| Long attribute lines | `[SerializeField]` chains | May need manual adjustment |
| Preprocessor blocks | `#if UNITY_EDITOR` formatting | Verify logic preserved |
| String literals | Multi-line strings in UI | Check not broken |

### GameCreator-Specific Considerations
```csharp
// CSharpier may reorder these - but order can matter for GameCreator
using GameCreator.Runtime.Common;      // Must be before specific modules
using GameCreator.Runtime.Characters;  // Depends on Common
using GameCreator.Runtime.VisualScripting;
```

### Safe Workflow
1. **Run CSharpier on branch** - Not directly on main
2. **Review diff carefully** - Especially using statements
3. **Test compilation** - After formatting
4. **Test runtime** - GameCreator actions/conditions still work

### Ignoring Specific Files
Add to `.csharpierignore`:
```
# Files that should not be formatted
**/Generated/**
**/ThirdParty/**
```

### Ignoring Code Sections
```csharp
// csharpier-ignore-start
// This code must stay exactly as-is
var specialFormat = new Dictionary<string, int>
{
    { "key1", 1 }, { "key2", 2 }, { "key3", 3 }
};
// csharpier-ignore-end
```

Or single statement:
```csharp
// csharpier-ignore
var matrix = new int[,] { {1,2,3}, {4,5,6}, {7,8,9} };
```

## Troubleshooting
| Issue | Solution |
|-------|----------|
| Extension not formatting | Check `.csharpierrc.json` exists |
| Syntax errors | Fix C# errors before formatting |
| Slow formatting | Exclude Library/, Temp/ folders |
| Using order wrong | Add `// csharpier-ignore` or review manually |
| Breaking GameCreator | Test actions/conditions after format |
