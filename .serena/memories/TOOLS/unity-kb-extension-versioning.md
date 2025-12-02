# Unity KB Extension Versioning Workflow

## Location
`D:\GithubRepos\MLcreator\vscode-extensions\unity-kb-query\`

## Version Files
- `package.json` - Main version source (npm standard)
- `VERSION` - Simple text file for quick version check
- `CHANGELOG.md` - Human-readable changelog

## Bump Version Script
```powershell
# Patch bump (0.2.0 -> 0.2.1)
.\scripts\bump-version.ps1 patch -message "Fixed bug X"

# Minor bump (0.2.0 -> 0.3.0)
.\scripts\bump-version.ps1 minor -message "Added feature Y"

# Major bump (0.2.0 -> 1.0.0)
.\scripts\bump-version.ps1 major -message "Breaking change Z"
```

## When to Bump Version

| Change Type | Bump | Example |
|-------------|------|---------|
| Bug fixes, typos | patch | 0.2.0 → 0.2.1 |
| New commands, features | minor | 0.2.0 → 0.3.0 |
| Breaking changes, major rewrites | major | 0.2.0 → 1.0.0 |

## Claude Code Workflow

When releasing a new version:
1. Make code changes
2. Compile: `cd vscode-extensions/unity-kb-query && npx tsc -p ./`
3. Run bump script: `.\scripts\bump-version.ps1 <type> -message "<summary>"`
4. Review CHANGELOG.md and add details
5. Commit with version tag

## Current Version
Read from `VERSION` file or `package.json`:
```powershell
Get-Content vscode-extensions/unity-kb-query/VERSION
# or
(Get-Content vscode-extensions/unity-kb-query/package.json | ConvertFrom-Json).version
```

## Semantic Versioning Rules
- MAJOR: Incompatible API changes
- MINOR: Add functionality (backward compatible)
- PATCH: Bug fixes (backward compatible)
