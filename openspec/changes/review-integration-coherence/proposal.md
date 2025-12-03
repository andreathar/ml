# Change: Review and Strengthen AI Integration Coherence

## Why

The MLCreator project has four major AI/tooling integrations (OpenSpec, Serena, Unity MCP, Unity-KB) that have been configured individually. A comprehensive review is needed to ensure:
1. All systems are properly integrated with each other
2. Strong rules and memories are defined and enforced
3. Configuration files are consistent and complete
4. No gaps exist in the AI assistant workflow

## What Changes

### Review Scope

1. **OpenSpec Integration**
   - Verify AGENTS.md is correctly referenced from all entry points
   - Ensure project.md reflects current architecture
   - Validate existing specs match implementation
   - Check decision-trees are current and useful

2. **Serena Integration**
   - Review memory tier structure (CRITICAL, CORE, INTEGRATION, TOOLS)
   - Validate config.yaml MCP server configurations
   - Ensure project.yml reflects current tech stack
   - Check memories are actionable and current

3. **Unity MCP Server**
   - Verify .mcp.json configuration is complete
   - Ensure stdio transport is properly configured
   - Validate connection health monitoring
   - Check safe mode settings

4. **Unity Knowledge Base (Qdrant)**
   - Verify KB is operational (77,914+ symbols expected)
   - Ensure keyword taxonomy is complete
   - Validate query workflows documented
   - Check decision patterns are defined

### Configuration Files to Review

| File | System | Status |
|------|--------|--------|
| `.openspec/AGENTS.md` | OpenSpec | Review & Update |
| `.openspec/project.md` | OpenSpec | Review & Update |
| `.serena/config.yaml` | Serena | Review & Update |
| `.serena/project.yml` | Serena | Review & Update |
| `.mcp.json` | Claude Code | Review & Update |
| `.claude/settings.local.json` | Claude Code | Review & Update |
| `CLAUDE.md` | Claude Code | Review & Update |
| `AGENTS.md` | AI Agents | Review & Update |

### Strong Rules to Define/Enforce

1. **Mandatory KB Query Before Analysis** - AI agents MUST query Unity-KB before proposing solutions
2. **Memory Tier Compliance** - Critical decisions must be stored in CRITICAL tier
3. **OpenSpec Proposal Workflow** - All non-trivial changes require proposals
4. **Configuration Consistency** - MCP server configs must match across all files
5. **Architecture Decision Recording** - All decisions in `.serena/memories/CRITICAL/`

## Impact

- Affected systems: OpenSpec, Serena, Unity MCP, Unity-KB, Claude Code
- Affected files: All configuration files listed above
- Risk: Low - review and documentation updates only
- **BREAKING**: None - this is a review and documentation proposal
