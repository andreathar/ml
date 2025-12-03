# AI Integration Coherence Guide

**Last Updated:** 2025-12-02
**Status:** Active

## Overview

This guide documents the integrated AI tooling ecosystem for the MLCreator Unity project. Four systems work together to provide intelligent development assistance.

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     AI Assistant Layer                       │
│         (Claude Code, Serena, Antigravity, Cursor)          │
└─────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        ▼                     ▼                     ▼
┌───────────────┐    ┌───────────────┐    ┌───────────────┐
│   OpenSpec    │    │    Serena     │    │   Unity MCP   │
│   Workflow    │    │   Memories    │    │    Server     │
│               │    │               │    │               │
│ - Proposals   │    │ - CRITICAL    │    │ - Editor API  │
│ - Specs       │    │ - CONTEXT     │    │ - Scene Mgmt  │
│ - Tasks       │    │ - INTEGRATION │    │ - Assets      │
└───────────────┘    └───────────────┘    └───────────────┘
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              ▼
                    ┌───────────────────┐
                    │   Unity-KB        │
                    │   (Qdrant)        │
                    │                   │
                    │   77,914+ symbols │
                    │   Semantic search │
                    └───────────────────┘
```

## The Four Systems

### 1. OpenSpec (Specification-Driven Development)

**Purpose:** Manage change proposals, specifications, and implementation tasks.

**Location:** `openspec/`

**Key Files:**
- `openspec/AGENTS.md` - AI assistant instructions
- `openspec/project.md` - Project conventions and context
- `openspec/changes/` - Active change proposals
- `openspec/specs/` - Capability specifications

**Workflow:**
1. Create proposal in `openspec/changes/{change-id}/`
2. Write `proposal.md`, `tasks.md`, optional `design.md`
3. Add spec deltas in `specs/{capability}/spec.md`
4. Validate: `openspec validate {change-id} --strict`
5. Implement after approval
6. Archive when complete

**CLI Commands:**
```bash
openspec list              # List active changes
openspec list --specs      # List specifications
openspec show {item}       # View details
openspec validate --strict # Validate all
```

### 2. Serena (Hierarchical Memory System)

**Purpose:** Manage AI context with tiered memories and MCP integration.

**Location:** `.serena/`

**Memory Tiers:**

| Tier | Purpose | Auto-Load |
|------|---------|-----------|
| CRITICAL | Architecture decisions, never-forget rules | Always |
| CONTEXT | Project patterns, conventions | On context |
| INTEGRATION | Framework rules (GameCreator, Netcode) | When relevant |
| TOOLS | Dev utility configurations | On-demand |

**Key Files:**
- `.serena/config.yaml` - MCP server configuration
- `.serena/project.yml` - Project metadata
- `.serena/memories/CRITICAL/` - 7 critical memory files

**CRITICAL Memories:**
1. `001_gamecreator_invasive_integration.md` - IsNetworkOwner pattern
2. `002_network_architecture_never_forget.md` - No NetworkTransform on players
3. `003_multiplayer_rpc_patterns.md` - RPC naming and patterns
4. `004_visual_scripting_task_signatures.md` - Task Run(Args args) only
5. `005_networkbehaviour_inheritance_pattern.md` - Avoid field conflicts
6. `006_network_spawn_workflow.md` - Runtime spawn, not scene placement
7. `007_ai_integration_rules.md` - This integration system

### 3. Unity MCP Server

**Purpose:** Direct Unity Editor integration via stdio protocol.

**Location:** `Library/mcp-server/win-x64/unity-mcp-server.exe`

**Configuration:** `.mcp.json`

```json
{
  "mcpServers": {
    "Unity-MCP": {
      "type": "stdio",
      "command": "Library/mcp-server/win-x64/unity-mcp-server.exe",
      "args": ["--port=8081", "--client-transport=stdio"]
    }
  }
}
```

**Capabilities:**
- Scene management
- Asset operations
- Script compilation status
- Play mode control
- Editor window access

### 4. Unity Knowledge Base (Qdrant)

**Purpose:** Semantic search across 77,914+ indexed code symbols.

**Backend:** Qdrant vector database at `localhost:6333`
**Collection:** `unity_project_kb`

**Query Guide:** `.serena/How_to_make_queries_to_KB_and_take_decisions.md`

**KB Scripts:** `openspec/unity-kb/`

**Health Check:**
```bash
curl http://localhost:6333/healthz
curl http://localhost:6333/collections/unity_project_kb
```

## Mandatory Workflows

### Before Proposing Multiplayer Solutions

```
1. Query Unity-KB for existing patterns
   → Open: http://localhost:6333/dashboard#/collections
   → Search relevant keywords

2. Check CRITICAL memories
   → Review .serena/memories/CRITICAL/

3. Reference findings in proposal
   → Include "KB Analysis Results" section
```

### Creating Changes

```
1. Determine if proposal required
   → Bug fix restoring spec: Direct fix OK
   → New feature/architecture: Proposal required

2. Create proposal
   → openspec/changes/{change-id}/
   → proposal.md, tasks.md, specs/

3. Validate
   → openspec validate {change-id} --strict

4. Implement after approval
```

### Recording Architecture Decisions

```
1. Identify critical decision
   → Affects architecture, breaking, or must-remember

2. Create CRITICAL memory
   → .serena/memories/CRITICAL/NNN_name.md
   → Follow existing template format

3. Include:
   → Priority, date, scope
   → Code patterns (correct/incorrect)
   → Quick reference table
```

## Configuration Files

| File | System | Purpose |
|------|--------|---------|
| `.mcp.json` | Claude Code | MCP server definitions |
| `.claude/settings.local.json` | Claude Code | Permissions |
| `.serena/config.yaml` | Serena | Full MCP + memory config |
| `.serena/project.yml` | Serena | Project metadata |
| `openspec/project.md` | OpenSpec | Conventions |
| `CLAUDE.md` | Claude Code | Entry point instructions |
| `AGENTS.md` | All AIs | OpenSpec redirect |

## Health Monitoring

Run the integration health check:

```powershell
.\scripts\check-integration-health.ps1
```

**Expected Results:**
- OpenSpec: directory exists, AGENTS.md present
- Serena: CRITICAL tier with 7 files
- Unity MCP: executable present, .mcp.json valid
- Unity-KB: Qdrant running, 77,914+ symbols

## Troubleshooting

### OpenSpec CLI Not Finding Changes

```bash
# Ensure using openspec/ not .openspec/
openspec list
```

### Qdrant Not Running

```bash
# Start Qdrant
docker start qdrant

# Or run locally
qdrant --config-path qdrant_config.yaml
```

### Unity MCP Connection Failed

1. Check Unity Editor is running
2. Verify port 8081 not blocked
3. Check `.mcp.json` paths are correct
4. Restart Unity Editor

### Memory Not Loading

1. Verify file is in correct tier directory
2. Check file has `.md` extension
3. Review `.serena/config.yaml` tier settings

## Quick Reference

| Need To... | Action |
|------------|--------|
| Find existing patterns | Query Unity-KB first |
| Add new feature | Create OpenSpec proposal |
| Record critical decision | Add to CRITICAL tier |
| Check system health | Run health check script |
| Add MCP server | Update all 3 config files |

## Related Documentation

- `openspec/AGENTS.md` - Complete OpenSpec instructions
- `.serena/memories/CRITICAL/007_ai_integration_rules.md` - Strong rules
- `.serena/memories/INTEGRATION/mcp_server_inventory.md` - MCP inventory
- `.serena/How_to_make_queries_to_KB_and_take_decisions.md` - KB queries
