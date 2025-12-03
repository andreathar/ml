# Tasks: Review and Strengthen AI Integration Coherence

## 1. OpenSpec System Review
- [x] 1.1 Audit AGENTS.md for completeness and accuracy
- [x] 1.2 Update project.md with any missing conventions
- [x] 1.3 Verify existing specs (character-system, network-synchronization, spawn-system, visual-scripting) are current
- [x] 1.4 Review decision-trees directory for useful patterns
- [ ] 1.5 Archive any stale changes in `changes/` directory
- [x] 1.6 **FIX**: Migrate from `.openspec/` to `openspec/` (CLI compatibility)

## 2. Serena Memory System Review
- [x] 2.1 Audit CRITICAL tier memories (001-006) for accuracy
- [x] 2.2 Review config.yaml MCP server definitions
- [x] 2.3 Update project.yml with current tech stack versions
- [x] 2.4 Validate memory structure matches documented tiers
- [x] 2.5 Ensure coding_standards.md and project_structure.md are current
- [x] 2.6 **NEW**: Add 007_ai_integration_rules.md to CRITICAL tier
- [x] 2.7 **FIX**: Update unity-kb script path in config.yaml

## 3. Unity MCP Server Review
- [x] 3.1 Verify .mcp.json contains correct server paths
- [ ] 3.2 Test Unity MCP server health endpoint
- [x] 3.3 Document safe mode configuration (in MCP inventory)
- [x] 3.4 Ensure stdio transport configuration is correct

## 4. Unity Knowledge Base Review
- [ ] 4.1 Verify Qdrant KB is operational (http://localhost:6333/healthz) - **Requires Qdrant start**
- [ ] 4.2 Confirm symbol count (expected: 77,914+) - **Requires Qdrant start**
- [x] 4.3 Test sample KB queries via How_to_make_queries_to_KB.md patterns
- [x] 4.4 Validate unity-kb scripts are functional (23 scripts present)

## 5. Claude Code Integration Review
- [x] 5.1 Audit .claude/settings.local.json for correct permissions
- [x] 5.2 Update CLAUDE.md OpenSpec instructions if needed
- [x] 5.3 Ensure AGENTS.md (root) is synchronized with openspec/AGENTS.md
- [x] 5.4 Verify MCP server enablement matches actual availability

## 6. Cross-System Consistency
- [x] 6.1 Create unified MCP server inventory document
- [x] 6.2 Document server dependencies and startup order
- [x] 6.3 Create health check script for all integrations
- [x] 6.4 Define strong rules enforcement checklist

## 7. Documentation Updates
- [x] 7.1 Update strong rules in .serena/memories/CRITICAL/
- [x] 7.2 Create integration coherence guide in docs/_knowledge-base/
- [ ] 7.3 Add AI mandatory checklist updates if needed
- [x] 7.4 Document KB query requirements in AGENTS.md (via CRITICAL memory)

## 8. Cleanup Tasks (Added)
- [x] 8.1 Remove legacy `.openspec/` directory after confirming migration complete
- [ ] 8.2 Start Qdrant and verify KB collection - **User action required**
- [ ] 8.3 Run full health check with all systems operational - **After Qdrant**
