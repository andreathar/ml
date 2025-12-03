# Persona Schemas Index

**Project:** ML - GameCreator Core + Netcode
**Last Updated:** 2025-12-03

## Overview

This project uses a minimal set of personas focused on the core multiplayer integration:

| Persona | File | Purpose |
|---------|------|---------|
| Index | `_index.llm.txt` | Master index of all modules |
| Character Core | `character-core.llm.txt` | GameCreator Character system |
| Netcode Core | `netcode-core.llm.txt` | Network integration patterns |

## Installed Modules

### GameCreator Core
- **Assembly:** `GameCreator.Runtime.Core`
- **Persona:** `character-core.llm.txt`
- **Components:** Character, Args, Instructions, Conditions, Triggers

### GameCreator Netcode Integration
- **Assembly:** `GameCreator.Netcode.Runtime`
- **Persona:** `netcode-core.llm.txt`
- **Components:** NetworkCharacterAdapter, NetworkCharacter, NetworkVariablesSync

## Usage

Before working on a module:

```
1. Load the relevant persona file
2. Review the key patterns and constraints
3. Check openspec/specs/ for requirements
4. Consult decision trees for networking choices
```

## Not Installed (Future Expansion)

The following GameCreator modules are NOT installed in this project:
- Inventory, Stats, Quests, Dialogue
- Perception, Behavior, Shooter, Melee
- Factions, Mailbox

When/if these are added, corresponding personas should be created.

## Related Files

- `.serena/personas/_index.llm.txt` - Module overview
- `.serena/symbols/gamecreator_modules.json` - Module registry
- `.serena/memories/CRITICAL/` - Critical patterns
