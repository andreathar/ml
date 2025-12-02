# Implementation Tasks: Unity Asset Corruption Prevention

## 1. Investigation & Assessment
- [ ] 1.1 Scan entire project for corrupted assets (full validation)
- [ ] 1.2 Create list of affected prefabs, materials, and assets
- [ ] 1.3 Document GameCreator plugin Unity 6 compatibility status
- [ ] 1.4 Analyze git history for patterns of corruption (identify trigger commits)
- [ ] 1.5 Verify current Unity version consistency across development machines

## 2. Immediate Recovery
- [ ] 2.1 Create backup snapshot of current project state (before any fixes)
- [ ] 2.2 Attempt to recover corrupted prefabs from git history
- [ ] 2.3 Regenerate corrupted assets if recovery fails
- [ ] 2.4 Validate all assets load correctly in Unity Editor
- [ ] 2.5 Document recovery process for future reference

## 3. Git Configuration
- [ ] 3.1 Create/update `.gitattributes` with Unity-specific rules
  - YAML files (.prefab, .asset, .mat) with LF line endings
  - Unity binary file detection and handling
  - Git LFS patterns for large assets
- [ ] 3.2 Configure `.gitignore` for Unity temporary files
- [ ] 3.3 Add pre-commit hook for asset validation
  - Check for Unity version consistency
  - Scan for corrupted asset signatures
  - Validate .meta file integrity
- [ ] 3.4 Test git hooks with sample commits
- [ ] 3.5 Document git configuration for team

## 4. Asset Health Monitoring System
- [ ] 4.1 Create `scripts/unity-asset-health-check.ps1`
  - Scan all prefabs and assets
  - Parse Unity console logs for errors
  - Validate .meta files exist and are valid
  - Check for missing GUID references
- [ ] 4.2 Create `.project-brain/monitors/asset-health.py`
  - Automated daily health checks
  - Integration with cleanup automation
  - Alert system for detected issues
- [ ] 4.3 Integrate with orchestration system
  - Add asset health quality gate
  - Enable continuous monitoring
- [ ] 4.4 Create dashboard for asset health metrics
- [ ] 4.5 Test monitoring system with known good and bad assets

## 5. Unity Version Enforcement
- [ ] 5.1 Document required Unity version (6000.2.13f1)
- [ ] 5.2 Create version detection script
- [ ] 5.3 Add Unity version check to pre-commit hook (BLOCKING)
- [ ] 5.4 Add Unity version validation to orchestration workflow
- [ ] 5.5 Create team notification about version requirements
- [ ] 5.6 Document version upgrade procedures for future updates

## 6. Plugin Compatibility System
- [ ] 6.1 Create plugin compatibility matrix
  - List all GameCreator plugins
  - Document Unity 6 compatibility status
  - Identify plugins needing updates
- [ ] 6.2 Contact GameCreator support about Unity 6 compatibility
- [ ] 6.3 Create workarounds for incompatible plugins
- [ ] 6.4 Document plugin upgrade procedures
- [ ] 6.5 Add plugin version tracking to project state

## 7. Backup & Recovery System
- [ ] 7.1 Implement automated prefab snapshot system
  - Before Unity version upgrades
  - Before bulk reimport operations
  - Before plugin updates
- [ ] 7.2 Create .meta file backup system
- [ ] 7.3 Implement asset dependency graph generator
- [ ] 7.4 Create rollback automation scripts
- [ ] 7.5 Test recovery procedures with test assets

## 8. Unity Editor Configuration
- [ ] 8.1 Configure asset serialization to Force Text mode
- [ ] 8.2 Enable visible meta files
- [ ] 8.3 Set consistent line ending mode (Unix LF)
- [ ] 8.4 Configure cache server if available
- [ ] 8.5 Document editor settings requirements

## 9. Documentation
- [ ] 9.1 Create incident response playbook
  - Step-by-step corruption detection
  - Recovery procedures
  - Escalation procedures
- [ ] 9.2 Create safe Unity upgrade guide
  - Pre-upgrade checklist
  - Upgrade steps
  - Post-upgrade validation
- [ ] 9.3 Create plugin compatibility checklist
- [ ] 9.4 Create developer onboarding guide
  - Unity version requirements
  - Git configuration steps
  - Asset safety best practices
- [ ] 9.5 Add to AI assistant critical context (`.serena/ai/critical.llm.txt`)

## 10. Testing & Validation
- [ ] 10.1 Test pre-commit hooks with various scenarios
  - Valid commits (should pass)
  - Corrupted assets (should block)
  - Version mismatches (should block)
  - Large file commits (should handle correctly)
- [ ] 10.2 Test monitoring system detects known issues
- [ ] 10.3 Test recovery procedures with backed up assets
- [ ] 10.4 Validate performance impact (<5s pre-commit overhead)
- [ ] 10.5 Test emergency bypass procedures

## 11. Team Rollout
- [ ] 11.1 Create rollout announcement
- [ ] 11.2 Schedule team training session
- [ ] 11.3 Provide individual developer setup support
- [ ] 11.4 Monitor first week for issues
- [ ] 11.5 Collect feedback and iterate

## 12. Long-term Maintenance
- [ ] 12.1 Schedule monthly asset health review
- [ ] 12.2 Monitor Unity version compatibility as new versions release
- [ ] 12.3 Update plugin compatibility matrix quarterly
- [ ] 12.4 Review and update procedures based on incidents
- [ ] 12.5 Archive lessons learned in project brain

## Validation Checklist

Before marking this change as complete, verify:

- ✅ All prefabs and assets load without errors in Unity Editor
- ✅ Pre-commit hooks successfully block corrupted assets
- ✅ Unity version enforcement prevents version mismatches
- ✅ Asset health monitoring runs successfully on schedule
- ✅ Recovery procedures have been tested and verified
- ✅ All documentation is complete and accessible
- ✅ Team has been trained on new procedures
- ✅ No performance regressions (<5s git commit overhead)
- ✅ Emergency bypass procedures work correctly
- ✅ Orchestration integration is complete and tested
