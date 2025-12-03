# OpenSpec ↔ Serena Complete Integration

**Category:** TOOLS - Cross-System Integration
**Project:** ML - GameCreator Core + Netcode
**Last Updated:** 2025-12-03

## Overview

This document defines the integration between OpenSpec (spec-driven development) and Serena (semantic coding tools) for the ML project.

## System Locations

| System | Location | Purpose |
|--------|----------|---------|
| OpenSpec CLI | `openspec/` | Spec-driven development workflow |
| OpenSpec Agents Guide | `openspec/AGENTS.md` | AI assistant instructions |
| OpenSpec Decision Trees | `openspec/.decision-trees/` | Interactive decision support |
| Serena Memories | `.serena/memories/` | Tiered knowledge storage |
| Serena Personas | `.serena/personas/` | Module-specific schemas |
| Serena Config | `.serena/config.yaml` | MCP and integration settings |

## Project Modules

This project has only 2 GameCreator modules:

| Module | Assembly | Persona |
|--------|----------|---------|
| Core | `GameCreator.Runtime.Core` | `character-core.llm.txt` |
| Netcode | `GameCreator.Netcode.Runtime` | `netcode-core.llm.txt` |

## Integration Points

### 1. OpenSpec → Serena (Spec Compliance)

**Before any code modification:**
1. Check `openspec/changes/` for active proposals
2. Read relevant specs from `openspec/specs/`
3. Validate changes against spec scenarios

```bash
# Check active changes
openspec list

# View specific spec
openspec show <spec-name>
```

### 2. Serena → OpenSpec (Code Discovery)

**When creating OpenSpec proposals:**
1. Use Serena `find_symbol` for existing implementations
2. Use `get_symbols_overview` to understand file structure
3. Reference actual code paths in proposals

### 3. Decision Trees

**Location:** `openspec/.decision-trees/`

Relevant trees for this project:
- `network-sync-method.yaml` - Choose sync approach
- `gc-character-sync.yaml` - Character networking
- `schema.yaml` - Decision tree schema

## Workflow

### Creating a New Feature

```
1. DISCOVER
   └── Serena: find_symbol, get_symbols_overview
   └── OpenSpec: openspec list --specs

2. PLAN
   └── OpenSpec: Create proposal.md, tasks.md
   └── Serena: Read personas, check constraints

3. VALIDATE
   └── OpenSpec: openspec validate <id> --strict
   └── Serena: Check against CRITICAL memories

4. IMPLEMENT
   └── Serena: replace_symbol_body, insert_after_symbol
   └── OpenSpec: Update tasks.md checkboxes

5. ARCHIVE
   └── OpenSpec: openspec archive <id> --yes
```

### Bug Fix vs Feature

```
BUG FIX (No OpenSpec needed):
├── Serena: Locate issue
├── Serena: Fix with editing tools
└── Verify against CRITICAL constraints

NEW FEATURE (OpenSpec required):
├── OpenSpec: Create proposal
├── Serena: Research existing code
├── OpenSpec: Define spec deltas
├── Serena: Implement per tasks.md
└── OpenSpec: Archive when complete
```

## Memory Tiers

### CRITICAL (Always loaded)
- `001_gamecreator_netcode_integration.md` - Adapter patterns
- `002_rpc_patterns_netcode2.md` - RPC syntax
- `003_visual_scripting_signatures.md` - VS patterns
- `004_network_variables_sync.md` - Variable sync

### TOOLS (Load on demand)
- `003_openspec_workflow.md` - OpenSpec workflow
- `008_openspec_serena_integration.md` - This file

## Quick Reference

| Task | Tool |
|------|------|
| Check active proposals | `openspec list` |
| View spec requirements | `openspec show <spec>` |
| Find code implementation | Serena `find_symbol` |
| Validate proposal | `openspec validate --strict` |
| Check constraints | `.serena/memories/CRITICAL/` |

## Related Documentation

- `openspec/AGENTS.md` - OpenSpec AI instructions
- `.serena/memories/TOOLS/003_openspec_workflow.md` - Workflow details
- `.serena/personas/_index.llm.txt` - Module index
