# Change: Add Unity Asset Corruption Prevention System

## Why

On 2025-11-25, the project experienced prefab corruption affecting GameCreator plugin assets (Behavior.ActionPlan@1.1.3). The specific error was:

```
Failed to load 'Assets/Plugins/GameCreator/Installs/Behavior.ActionPlan@1.1.3/Prefabs/Branch.prefab'.
File may be corrupted or was serialized with a newer version of Unity.
```

### Root Cause Analysis

Investigation revealed multiple contributing factors:

1. **Unity 6 Compatibility**: Project uses Unity 6000.2.13f1 (Unity 6), but GameCreator plugins were created for earlier Unity versions (2021/2022)
2. **Asset Serialization Changes**: Unity 6 introduced breaking changes to asset serialization format
3. **Bulk Reimport Operations**: Git history shows 30+ material files modified simultaneously, indicating Unity performed automatic asset reimport
4. **Line Ending Changes**: Recent commit "Fix line endings and add Git configuration" may have triggered mass reimport
5. **No Version Control Protection**: No safeguards exist to detect incompatible Unity versions or prevent catastrophic asset reimports

### Impact

- **Current**: Unknown number of prefabs and assets may be affected
- **Risk**: Complete project data loss if corruption spreads
- **Productivity**: Development blocked until assets are restored
- **Technical Debt**: No monitoring or prevention system exists

## What Changes

This proposal establishes a comprehensive Unity asset protection system to prevent corruption and enable rapid recovery:

### 1. Asset Health Monitoring System
- Pre-commit hooks to detect corrupted assets before they enter version control
- Automated Unity console log scanning for asset load failures
- Daily asset integrity validation checks
- Immediate alerts when corruption is detected

### 2. Unity Version Control Integration
- Enforce Unity version consistency across team (prevent version mismatches)
- Automatic detection of Unity version changes with validation gates
- Plugin compatibility matrix (Unity 6 compatibility tracking)
- **BREAKING**: Block commits if Unity version doesn't match project standard

### 3. Asset Backup & Recovery System
- Automated prefab snapshots before bulk operations
- .meta file backup and validation
- Rollback procedures for corrupted assets
- Asset dependency graph for impact analysis

### 4. GitAttributes Configuration
- Proper handling of Unity asset line endings (YAML files)
- Binary file detection for .prefab, .asset, .mat files
- Git LFS integration for large assets
- Prevent text transformations on Unity binary formats

### 5. Documentation & Procedures
- Incident response playbook for asset corruption
- Safe Unity upgrade procedures
- Plugin compatibility verification checklist
- Recovery procedures with step-by-step instructions

## Impact

### Affected Specs
- **NEW**: `unity-asset-management` - Asset integrity and version control
- **NEW**: `unity-project-safety` - Project-wide safety measures
- **NEW**: `git-integration` - Unity-specific git configuration

### Affected Code
- **New**: `.git/hooks/pre-commit` - Asset validation hook
- **New**: `scripts/unity-asset-health-check.ps1` - Health monitoring script
- **New**: `.project-brain/monitors/asset-health.py` - Automated monitoring
- **Modified**: `.gitattributes` - Unity asset handling rules
- **Modified**: `ProjectSettings/EditorSettings.asset` - Asset serialization settings
- **New**: `claudedocs/procedures/asset-corruption-recovery.md` - Recovery procedures

### Breaking Changes
- **BREAKING**: Unity version enforcement - blocks commits with version mismatches
- **BREAKING**: Pre-commit hooks add 2-5 second validation overhead
- **BREAKING**: Git LFS may be required for large assets

### Migration Requirements
1. All developers must use Unity 6000.2.13f1 (no exceptions)
2. Install Git LFS if not already configured
3. Run initial asset health check: `python .project-brain/monitors/asset-health.py --full-scan`
4. Update GameCreator plugins to Unity 6 compatible versions (or document workarounds)

### Risk Mitigation
- Asset backups created before system activation
- Rollback procedures documented
- Validation can be temporarily disabled for emergencies (with audit logging)
- Monitoring is non-invasive (read-only operations)

### Success Criteria
- Zero asset corruption incidents after implementation
- 100% detection rate for incompatible Unity versions
- <5 second pre-commit validation time
- Complete recovery capability within 15 minutes of incident detection
