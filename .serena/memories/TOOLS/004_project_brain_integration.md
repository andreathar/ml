# Project Brain Integration

**Category:** TOOLS - Intelligence Engine
**Last Updated:** 2025-11-23
**System:** Project Intelligence Engine (`.project-brain/`)

## Overview

Project Intelligence Engine provides:
- **Auto-generated state** (`state.json`) - Single source of truth
- **Health monitoring** - Validates project integrity
- **Manifest generation** - Smart context loading
- **Cross-validation** - Detects broken references
- **Cleanup automation** - Enforces file organization

## Core Files

### state.json (Auto-Generated)

**Location:** `.project-brain/core/state.json`
**Regeneration:** Every session (auto-updates)
**Purpose:** Single source of truth for project state

**Contains:**
```json
{
  "generated_at": "2025-11-23T22:17:01Z",
  "project_name": "MLCreator",
  "unity_version": "6000.2.13f1",
  "health_status": { ... },
  "active_systems": [ ... ],
  "deprecated_systems": [ ... ],
  "critical_constraints": [ ... ],
  "task_context_map": { ... },
  "cross_references": { ... }
}
```

### systems.json (Manual Registry)

**Location:** `.project-brain/registries/systems.json`
**Purpose:** System definitions and task blueprints

**Defines:**
- Active systems (Serena MCP, OpenSpec, Unity MCP)
- Deprecated systems (serena-network)
- Knowledge graph (domains and relationships)
- Task blueprints (character-movement, multiplayer-networking, etc.)
- Integration matrix (system dependencies)

## AI Session Initialization

### Step 1: Load state.json First

```
On session start:
1. Load .project-brain/core/state.json (~2000 tokens)
2. Check health_status for warnings/issues
3. Review deprecated_systems to avoid recommending them
4. Load task_context_map for user's intended task
```

### Step 2: Detect Task Intent

**Task context maps provide optimized loading:**

```json
{
  "task_context_map": {
    "character-movement": {
      "load_order": [
        ".project-brain/core/state.json",
        ".serena/memories/CRITICAL/001_gamecreator_invasive_integration.md",
        ".serena/ai/critical.llm.txt"
      ],
      "skip_if_unrelated": [
        ".serena/memories/CONTEXT/gamecreator_shooter_module.md"
      ],
      "estimated_tokens": 4500,
      "proactive_warnings": [
        "⚠️ NetworkTransform is REMOVED from player prefabs"
      ]
    }
  }
}
```

### Step 3: Load Context Efficiently

**Use task blueprint to minimize token usage:**

```
Task: "Fix character networking"
→ Detect intent: character-movement
→ Load state.json
→ Load critical memory (001_gamecreator_invasive_integration.md)
→ Load critical.llm.txt
→ SKIP shooter/factions modules (not relevant)
→ Total: ~4500 tokens vs 15000+ if loaded everything
```

## Health Monitoring

### Health Status Structure

```json
{
  "health_status": {
    "overall": "healthy",
    "issues": [],
    "warnings": [
      "13 unauthorized files in project root"
    ],
    "last_validation": "2025-11-23T22:17:01Z"
  }
}
```

### Health Check Triggers

- **Auto:** Every state.json generation
- **Manual:** `python .project-brain/core/health-monitor.py`
- **Scheduled:** Via cleanup automation (every 30 min)

### Validation Rules

```json
{
  "validation_rules": [
    {
      "rule_id": "no-deprecated-usage",
      "severity": "error",
      "validator": ".project-brain/validators/deprecation-scanner.py"
    },
    {
      "rule_id": "root-organization",
      "severity": "warning",
      "validator": ".project-brain/automation/cleanup-scheduler.ps1"
    },
    {
      "rule_id": "cross-reference-integrity",
      "severity": "warning",
      "validator": ".project-brain/validators/cross-reference.py"
    }
  ]
}
```

## Cross-Reference Validation

### Purpose

Ensures all documentation references point to existing files.

### Validation Process

```python
# .project-brain/validators/cross-reference.py
for reference in find_all_references(project):
    if not file_exists(reference):
        report_broken_reference(reference)
```

### Common Reference Patterns

**In CLAUDE.md:**
```markdown
- `.serena/ai/critical.llm.txt` → Validates file exists
- `openspec/specs/character-system/spec.md` → Validates spec exists
```

**In Serena memories:**
```markdown
## Related Documentation
- `.serena/memories/CRITICAL/001_*.md` → Validates sibling files
- `Assets/Plugins/GameCreator_Multiplayer/Runtime/Character/NetworkCharacterAdapter.cs` → Validates code file
```

### Broken Reference Handling

**Detected by health monitor:**
```json
{
  "issues": [
    {
      "type": "broken_reference",
      "source": "CLAUDE.md:line 42",
      "target": ".serena/memories/CRITICAL/003_missing_file.md",
      "severity": "warning"
    }
  ]
}
```

## Task Context Maps

### Purpose

Pre-configured optimal file loading for common tasks.

### Available Maps

```json
{
  "character-movement": {
    "estimated_tokens": 4500,
    "proactive_warnings": ["NetworkTransform removed", "IsNetworkSpawned flag required"]
  },
  "multiplayer-networking": {
    "estimated_tokens": 6000,
    "proactive_warnings": ["RPC naming", "Server authority"]
  },
  "visual-scripting": {
    "estimated_tokens": 5000,
    "proactive_warnings": ["No CancellationToken", "Use existing icons"]
  }
}
```

### Using Task Maps

**AI assistant workflow:**

```
User: "Help me with character networking"
→ Load state.json
→ Detect intent: "character-networking"
→ Use task_context_map["character-movement"]
→ Load files in load_order
→ Skip files in skip_if_unrelated
→ Deliver proactive_warnings
→ Start work with optimized context
```

## Integration with Other Systems

### Serena MCP Integration

```json
{
  "integration_matrix": {
    "serena-mcp": {
      "reads_from": [".project-brain/core/state.json"],
      "writes_to": [".serena/memories/"],
      "dependencies": ["C# LSP", "project structure"]
    }
  }
}
```

**Serena uses state.json for:**
- Deprecation warnings before symbol search
- Task context map file loading
- Cross-reference validation

### OpenSpec Integration

```json
{
  "openspec": {
    "reads_from": [".project-brain/core/state.json"],
    "writes_to": [".openspec/specs/", ".openspec/changes/"]
  }
}
```

**Project Brain validates:**
- Active OpenSpec changes
- Spec-to-code alignment
- Change implementation status

## Cleanup Automation

### Scheduler Script

**Location:** `.project-brain/automation/cleanup-scheduler.ps1`

**Functions:**
- Detects unauthorized root files
- Suggests correct locations
- Can auto-move files (with confirmation)
- Runs every 30 minutes (if scheduled)

### Setup Scheduled Cleanup

```powershell
# Dry run (see what would be cleaned)
.\.project-brain\automation\cleanup-scheduler.ps1 -DryRun

# Setup automatic scheduling
.\.project-brain\automation\cleanup-scheduler.ps1 -SetupSchedule
```

## State Generation

### Manual Regeneration

```bash
python .project-brain/core/state-generator.py
```

### What Triggers Regeneration

- Manual invocation
- AI session start (recommended)
- After major changes (OpenSpec archival, new systems)
- Health validation runs

### Generation Process

```python
1. Scan project directories
2. Validate active systems
3. Check deprecated system references
4. Validate critical constraints
5. Cross-reference integrity check
6. Generate task context maps
7. Write state.json
8. Return health status
```

## Deprecation Detection

### Deprecated Systems Registry

```json
{
  "deprecated": [
    {
      "id": "serena-network",
      "replacement_id": "serena-mcp",
      "ai_blocklist": {
        "block_recommendation": true,
        "warning_message": "⛔ serena-network is DEPRECATED. Use .serena/memories/ instead."
      }
    }
  ]
}
```

### AI Assistant Check

**Before recommending any system:**
```
1. Check state.json → deprecated_systems
2. If system.id matches, STOP
3. Return warning_message instead
4. Suggest replacement_id
```

## Manifest Generation (Advanced)

### Purpose

Generate optimal context loading plan for complex tasks.

### Usage

```bash
python .project-brain/context-engine/manifest-generator.py "character networking task"
```

### Output

```json
{
  "intent": "character-movement",
  "files": {
    "layer_0_critical": ["state.json", "systems.json"],
    "layer_1_constraints": ["critical.llm.txt", "CLAUDE.md"],
    "layer_2_task_specific": ["001_gamecreator_invasive_integration.md"],
    "layer_3_skip": ["shooter_module.md", "factions_module.md"]
  },
  "estimated_tokens": 4500,
  "warnings": ["NetworkTransform removed"],
  "suggestions": ["Check IsNetworkSpawned flag"]
}
```

## Related Documentation

- `.project-brain/docs/AI_ONBOARDING_PROTOCOL.md` - Session initialization
- `.project-brain/registries/systems.json` - System definitions
- `.serena/memories/TOOLS/006_file_organization_rules.md` - File placement rules

## Quick Reference

| Task | Command |
|------|---------|
| Regenerate state | `python .project-brain/core/state-generator.py` |
| Run health check | `python .project-brain/core/health-monitor.py` |
| Generate manifest | `python .project-brain/context-engine/manifest-generator.py "<query>"` |
| Dry run cleanup | `.\.project-brain\automation\cleanup-scheduler.ps1 -DryRun` |

## Best Practices

1. **Always load state.json first** - Single source of truth
2. **Check health_status** - Address warnings before starting work
3. **Use task context maps** - Optimize token usage
4. **Validate cross-references** - Keep documentation accurate
5. **Regenerate after major changes** - Keep state current
