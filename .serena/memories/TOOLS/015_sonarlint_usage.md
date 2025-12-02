# SonarLint/SonarQube Usage Guide

## Purpose
SonarLint is a static code analysis tool that detects bugs, vulnerabilities, and code smells in real-time. SonarCloud provides centralized quality tracking.

## Configuration
**VSCode Settings** (`.vscode/settings.json`):
```json
{
  "sonarlint.connectedMode.project": {
    "connectionId": "andreathar",
    "projectKey": "andreathar_MLcreator"
  }
}
```

**Local Config**: `.sonarlint/`

## When to Use
- During development (real-time feedback)
- Before committing code
- During code reviews
- For quality gate checks

## Issue Categories

### Bugs
Actual code defects that will cause incorrect behavior:
```csharp
// Bug: Possible null dereference
string name = GetName();
int length = name.Length; // name could be null
```

### Vulnerabilities
Security issues that could be exploited:
```csharp
// Vulnerability: SQL Injection
string query = "SELECT * FROM users WHERE id = " + userId;
```

### Code Smells
Maintainability issues that make code harder to understand:
```csharp
// Code Smell: Method too complex
public void DoEverything() { /* 500 lines */ }
```

### Security Hotspots
Code that needs security review:
```csharp
// Hotspot: Sensitive data handling
var password = Request.Form["password"];
```

## Output Patterns

### Issue Report
```
Bug: S1854 - Unused assignment
  File: PlayerController.cs
  Line: 42
  Severity: Major

  Remove this useless assignment to local variable 'temp'.
```

### Quality Gate
```
Quality Gate: PASSED
  Bugs: 0 (0 new)
  Vulnerabilities: 0 (0 new)
  Code Smells: 12 (2 new)
  Coverage: 75.3%
  Duplications: 2.1%
```

## AI Interpretation Guide

### Severity Levels
| Severity | Meaning | Action |
|----------|---------|--------|
| Blocker | Will break application | Fix immediately |
| Critical | Likely to cause issues | Fix before merge |
| Major | Significant problem | Fix in sprint |
| Minor | Quality issue | Fix when convenient |
| Info | Informational | Optional |

### Issue Types and Priority
| Type | Examples | Priority |
|------|----------|----------|
| Bug | Null deref, infinite loop | High |
| Vulnerability | SQL injection, XSS | Critical |
| Code Smell | Long method, complexity | Medium |
| Security Hotspot | Auth code, crypto | Review |

### Understanding Rules
Each rule has an ID (e.g., S1854):
- **S-rules**: SonarSource rules
- **CS-rules**: C#-specific rules
- **Documentation**: Hover for explanation

### Unity-Specific Considerations
Some rules may false-positive on Unity patterns:
| Rule | Unity Pattern | Action |
|------|---------------|--------|
| S1144 | Unused Unity callbacks | Suppress |
| S1172 | Unused parameters in messages | Suppress |
| S3881 | IDisposable on MonoBehaviour | Review case |

**Suppress with**:
```csharp
[SuppressMessage("SonarLint", "S1144")]
private void OnTriggerEnter(Collider other) { }
```

## Quality Gate Metrics
| Metric | Target | Description |
|--------|--------|-------------|
| Bugs | 0 new | No new bugs |
| Vulnerabilities | 0 new | No new security issues |
| Code Smells | < 10 new | Limited new debt |
| Coverage | > 80% | Test coverage |
| Duplications | < 3% | Code duplication |

## VSCode Integration
- Extension: `SonarLint`
- Real-time analysis as you type
- Inline issue display
- Quick fix suggestions
- Connected mode for team rules

## Connected Mode Benefits
- **Shared rules**: Team-wide consistency
- **Issue sync**: See SonarCloud issues locally
- **Quality gates**: Enforce standards
- **History**: Track quality over time

## CLI Commands (SonarScanner)
```bash
# Run analysis
dotnet sonarscanner begin /k:"project-key"
dotnet build
dotnet sonarscanner end

# View results
# Open: https://sonarcloud.io/project/overview?id=andreathar_MLcreator
```

## Best Practices
1. **Fix blockers immediately**: Never commit blockers
2. **Review hotspots**: Security code needs human review
3. **Track trends**: Watch quality gate over time
4. **Configure rules**: Adjust for Unity patterns
5. **Use connected mode**: Sync with team standards

## Troubleshooting
| Issue | Solution |
|-------|----------|
| Connected mode not working | Verify credentials |
| False positives | Add rule exclusions |
| Slow analysis | Increase memory limits |
| Missing rules | Update extension |
