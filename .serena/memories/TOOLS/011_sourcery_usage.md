# Sourcery Usage Guide

## Purpose
Sourcery is an AI-powered code review tool that suggests refactoring improvements, identifies code smells, and provides quality metrics.

## Configuration
Sourcery works via VSCode extension. Optional config file: `.sourcery.yaml`

## When to Use
- During code reviews
- After writing new features
- When refactoring legacy code
- To learn better coding patterns

## Output Patterns

### Refactoring Suggestion
```
Sourcery refactoring suggestion:
  Replace this with a list comprehension

  Before:
    result = []
    for item in items:
        if item.IsValid:
            result.append(item)

  After:
    result = [item for item in items if item.IsValid]
```

### Code Quality Metrics
```
Quality Score: 85/100
- Complexity: Low
- Readability: High
- Maintainability: Medium
```

## AI Interpretation Guide

### Suggestion Categories
| Category | Severity | Action |
|----------|----------|--------|
| `refactoring` | Low | Consider applying |
| `simplify` | Medium | Recommended |
| `performance` | High | Should apply |
| `bug-risk` | Critical | Must fix |

### Interpreting Quality Scores
- **90-100**: Excellent, no changes needed
- **70-89**: Good, minor improvements possible
- **50-69**: Fair, refactoring recommended
- **Below 50**: Poor, significant refactoring needed

### Unity-Specific Considerations
Sourcery may flag these patterns incorrectly:
- `Start()`, `Update()`, `Awake()` lifecycle methods
- `[SerializeField]` private fields
- Coroutine `yield return` patterns
- Unity event callbacks

**Suppress false positives** with:
```csharp
// sourcery skip: no-unused-private-method
private void OnTriggerEnter(Collider other) { }
```

## VSCode Integration
- Extension: `Sourcery`
- Shows inline suggestions
- Hover for explanations
- Lightbulb quick fixes

## Metrics Tracked
- **Cyclomatic Complexity**: Number of decision paths
- **Cognitive Complexity**: How hard to understand
- **Lines of Code**: Method/class size
- **Duplication**: Repeated code patterns

## Best Practices
1. Review suggestions, don't blindly apply
2. Consider Unity context before accepting
3. Use metrics for code review discussions
4. Track quality trends over time

## Troubleshooting
| Issue | Solution |
|-------|----------|
| No suggestions showing | Check extension is enabled |
| False positives on Unity code | Add skip comments |
| Slow analysis | Exclude large generated files |
