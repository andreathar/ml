# AI Integration Rules: Mandatory Workflows

**Priority:** P0 CRITICAL
**Last Updated:** 2025-12-02
**Applies To:** All AI assistants (Claude Code, Serena, Antigravity, Cursor)

## Core Integration Systems

This project uses four interconnected AI/tooling systems:

| System | Purpose | Config Location |
|--------|---------|-----------------|
| **OpenSpec** | Specification-driven development | `openspec/` |
| **Serena** | Hierarchical memory management | `.serena/` |
| **Unity MCP** | Unity Editor integration | `.mcp.json` |
| **Unity-KB** | Knowledge base (77,914+ symbols) | Qdrant localhost:6333 |

## CRITICAL: Mandatory KB Query Before Analysis

### The Rule

**Before proposing any solution involving multiplayer, networking, or GameCreator integration, AI assistants MUST query the Unity Knowledge Base.**

### Why This Is Critical

The KB contains 77,914+ indexed symbols including:
- All GameCreator module APIs
- Existing sync patterns and implementations
- Network architecture decisions
- Known conflicts and resolutions

**Skipping KB queries leads to:**
- Duplicate implementations
- Pattern violations
- Reinventing existing solutions
- Missing optimization opportunities

### The Pattern

```markdown
## Before Proposing Solution

1. Query KB for existing patterns:
   - Open KB Dashboard: http://localhost:6333/dashboard#/collections
   - Select collection: unity_project_kb
   - Search relevant keywords

2. Include KB findings in proposal:
   ## KB Analysis Results
   - Found X similar implementations
   - Identified Y relevant patterns
   - Noted Z potential conflicts

3. Reference KB results in rationale
```

### KB Query Keywords

| Domain | Keywords |
|--------|----------|
| Character Sync | `NetworkCharacter`, `CharacterAdapter`, `sync` |
| Inventory | `NetworkInventory`, `InventorySync`, `item` |
| Stats | `NetworkStats`, `StatsSync`, `attribute` |
| RPCs | `ServerRpc`, `ClientRpc`, `rpc` |
| GameCreator | `gc.character`, `gc.inventory`, `gc.stats` |

### Fallback When KB Unavailable

```markdown
## If Qdrant Not Running

1. Note degraded capability in response
2. Use filesystem-based search as fallback:
   - Grep for patterns in Assets/Plugins/
   - Check .serena/memories/ for documented patterns
3. Proceed with caution, noting KB was not consulted
```

## CRITICAL: OpenSpec Proposal Workflow

### When Proposals Are Required

| Scenario | Proposal Required? |
|----------|-------------------|
| Bug fix restoring spec behavior | No - direct fix |
| Typo/comment/formatting | No - direct fix |
| New feature | **YES** |
| Architecture change | **YES** |
| Breaking change | **YES** |
| Non-trivial modification | **YES** |

### Proposal Structure

```
openspec/changes/{change-id}/
├── proposal.md     # Why, what, impact
├── tasks.md        # Implementation checklist
├── design.md       # Technical decisions (if needed)
└── specs/          # Delta specifications
    └── {capability}/
        └── spec.md # ADDED/MODIFIED/REMOVED requirements
```

### Validation Before Implementation

```bash
# Always validate proposals before implementing
openspec validate {change-id} --strict
```

## CRITICAL: Memory Tier Compliance

### Tier Usage

| Tier | Content | Auto-Load |
|------|---------|-----------|
| **CRITICAL** | Architecture decisions, never-forget rules | Always |
| **CONTEXT** | Project patterns, conventions | On context |
| **INTEGRATION** | Framework rules (GC, Netcode) | When relevant |
| **TOOLS** | Dev utility configs | On-demand |

### Recording Architecture Decisions

When discovering or making critical decisions:

1. Create file in `.serena/memories/CRITICAL/`
2. Use naming: `NNN_descriptive_name.md`
3. Follow template from existing files (001-006)
4. Include:
   - Priority designation
   - Last updated date
   - Applies-to scope
   - Code patterns (correct/incorrect)
   - Quick reference table

## CRITICAL: MCP Configuration Consistency

### Canonical Configuration

`.serena/config.yaml` is the canonical MCP configuration source.

### Required Synchronization

When adding/modifying MCP servers, update ALL locations:

1. `.mcp.json` - Claude Code configuration
2. `.serena/config.yaml` - Serena configuration
3. `.claude/settings.local.json` - Permissions

### Configuration Template

```yaml
# .serena/config.yaml format
mcp:
  servers:
    {server-name}:
      enabled: true
      command: "{path-to-executable}"
      args: ["--arg1", "--arg2"]
```

```json
// .mcp.json format
{
  "mcpServers": {
    "{server-name}": {
      "type": "stdio",
      "command": "{path-to-executable}",
      "args": ["--arg1", "--arg2"]
    }
  }
}
```

## Health Check

Run the integration health check regularly:

```powershell
.\scripts\check-integration-health.ps1
```

Expected output:
- OpenSpec: directory exists, AGENTS.md present
- Serena: CRITICAL tier populated
- Unity MCP: executable present, .mcp.json valid
- Unity-KB: Qdrant running, 77,914+ symbols

## Quick Reference

| Question | Answer |
|----------|--------|
| Before proposing multiplayer solution? | **Query Unity-KB first** |
| New feature implementation? | **Create OpenSpec proposal** |
| Critical decision discovered? | **Add to CRITICAL tier** |
| Adding MCP server? | **Update all 3 config files** |
| Check system health? | **Run check-integration-health.ps1** |

## Related Documentation

- `openspec/AGENTS.md` - OpenSpec workflow
- `.serena/memories/INTEGRATION/mcp_server_inventory.md` - MCP inventory
- `.serena/How_to_make_queries_to_KB_and_take_decisions.md` - KB query guide
- `openspec/changes/review-integration-coherence/` - Integration review proposal
