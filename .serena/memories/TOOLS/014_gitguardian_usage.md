# GitGuardian Usage Guide

## Purpose
GitGuardian (ggshield) detects and prevents secrets from being committed to git repositories. It scans for API keys, passwords, tokens, and other sensitive data.

## Configuration
**File**: `.ggshield`
- Version 2 configuration
- Custom rules for Unity/Supabase keys
- Pre-commit hook enabled
- Comprehensive file exclusions

## When to Use
- Before every commit (automatic via pre-commit)
- When reviewing code for security
- After discovering potential leaks
- During security audits

## Secret Types Detected
| Type | Pattern | Severity |
|------|---------|----------|
| Unity API Key | `unity.*api.*key` | High |
| Supabase Anon Key | `supabase.*anon.*key` | Critical |
| Supabase Service Key | `supabase.*service.*key` | Critical |
| Generic API Key | `api[_-]?key` | High |
| Database Password | `db.*password` | High |
| AWS Keys | `AKIA...` | Critical |
| Private Keys | `-----BEGIN.*PRIVATE KEY-----` | Critical |

## Output Patterns

### Secret Detected
```
ggshield: 1 incident detected

>>> Incident 1: Generic High Entropy Secret
   Filename: config.json
   Line: 15
   Secret: ****************************

   Remediation: Remove the secret and rotate credentials
```

### Clean Scan
```
ggshield: No secrets detected
```

### Policy Violation
```
ggshield: Policy violation detected
   File: .env.production
   Rule: Production env files should not be committed
```

## AI Interpretation Guide

### Alert Severity
| Severity | Meaning | Action |
|----------|---------|--------|
| Critical | Active credential exposed | Rotate immediately |
| High | Potential credential | Investigate, likely rotate |
| Medium | Possible sensitive data | Review context |
| Low | Pattern match, may be false positive | Verify |

### Interpreting Incidents
1. **Check file and line**: Locate the secret
2. **Assess severity**: Is it real or false positive?
3. **Check git history**: Has it been pushed?
4. **Remediate**: Remove and rotate if needed

### False Positive Handling
Add to `.ggshield`:
```yaml
# Ignore specific patterns
ignore_paths:
  - "test_data/"
  - "**/mock_secrets.json"

# Ignore specific secrets (hash-based)
ignore_secrets:
  - "abc123def456..."  # Test API key
```

### Common False Positives in MLCreator
| Pattern | Why False Positive |
|---------|-------------------|
| Unity GUID | Looks like token but isn't |
| Base64 test data | High entropy but safe |
| Example configs | Documentation, not real |
| Hash values | Not credentials |

## Pre-Commit Integration
```yaml
# .pre-commit-config.yaml
repos:
  - repo: https://github.com/gitguardian/ggshield
    rev: v1.x.x
    hooks:
      - id: ggshield
        language_version: python3
```

## CLI Commands
```bash
# Scan current directory
ggshield secret scan path .

# Scan specific commit
ggshield secret scan commit HEAD

# Scan git history
ggshield secret scan repo .

# Check pre-commit setup
ggshield secret scan pre-commit
```

## Remediation Steps
1. **Remove secret from code**: Delete or move to env var
2. **Rotate credential**: Generate new key/password
3. **Clear git history**: If pushed, use BFG or filter-branch
4. **Update .ggshield**: Add to ignore if false positive
5. **Verify**: Re-scan to confirm clean

## Best Practices
1. **Never commit secrets**: Use environment variables
2. **Use .env files**: Add to .gitignore
3. **Rotate exposed secrets**: Even if caught before push
4. **Review alerts promptly**: Don't ignore warnings

## Troubleshooting
| Issue | Solution |
|-------|----------|
| Pre-commit not running | Reinstall pre-commit hooks |
| False positives | Add to ignore list |
| Slow scanning | Reduce scan scope |
| Missing detections | Update ggshield version |
