# Persona Schemas Index

**Project:** ML - GameCreator Core + Netcode + Perception
**Last Updated:** 2025-12-04

## Overview

This project uses personas focused on the multiplayer AI integration:

| Persona | File | Purpose |
|---------|------|---------|
| Index | `_index.llm.txt` | Master index of all modules |
| Character Core | `character-core.llm.txt` | GameCreator Character system |
| Netcode Core | `netcode-core.llm.txt` | Network integration patterns |
| Perception | `perception.llm.txt` | AI senses and awareness |

## Installed Modules

### GameCreator Core
- **Assembly:** `GameCreator.Runtime.Core`
- **Persona:** `character-core.llm.txt`
- **Components:** Character, Args, Instructions, Conditions, Triggers

### GameCreator Perception
- **Assembly:** `GameCreator.Runtime.Perception`
- **Persona:** `perception.llm.txt`
- **Components:** Perception, Cortex, Tracker, Sensors, Evidence
- **Network Status:** Local-only (integration planned)

### GameCreator Netcode Integration
- **Assembly:** `GameCreator.Netcode.Runtime`
- **Persona:** `netcode-core.llm.txt`
- **Components:** NetworkCharacterAdapter, NetworkCharacter, NetworkVariablesSync

## Active OpenSpec Changes

### Perception Network Integration
- **Change ID:** `add-perception-netcode-integration`
- **Status:** 0/64 tasks
- **Goal:** Server-authoritative perception sync
- **Location:** `openspec/changes/add-perception-netcode-integration/`

## Usage

Before working on a module:

```
1. Load the relevant persona file
2. Review the key patterns and constraints
3. Check openspec/specs/ for requirements
4. Check openspec/changes/ for active proposals
```

## Not Installed (Future Expansion)

The following GameCreator modules are NOT installed:
- Inventory, Stats, Quests, Dialogue
- Behavior, Shooter, Melee
- Factions, Mailbox

## Related Files

- `.serena/personas/_index.llm.txt` - Module overview
- `.serena/symbols/gamecreator_modules.json` - Module registry
- `.serena/memories/CRITICAL/` - Critical patterns
