# GameCreator Networked Persona Schemas

**Location**: `.serena/personas/`
**Generated**: 2025-12-01
**KB Source**: 77,914 indexed symbols from Qdrant

## Available Persona Schemas (13 Modules)

### Core Foundation Modules (NEW)

| Module | Slug | Network Sync | Symbols | Focus |
|--------|------|--------------|---------|-------|
| **Character Core** | gc-character-core | NetworkCharacterAdapter | 2042 | CharacterController, Transform, Mannequin, FBX, Animations, Input, IK, Skeleton |
| **Netcode Core** | unity-netcode-core | N/A (Foundation) | 2013 | NetworkBehaviour, NetworkVariable, RPCs, Transport, WebGL, Spawning, Pooling, Ownership |

### GameCreator Networked Modules

| Module | Slug | Network Sync | Symbols | Dependencies |
|--------|------|--------------|---------|--------------|
| Inventory | gc-inventory-networked | NetworkInventorySync | 175 | Variables, Stats |
| Stats | gc-stats-networked | NetworkStatsSync | 134 | Variables |
| Character | gc-character-networked | NetworkCharacterAdapter | 190 | None |
| Perception | gc-perception-networked | NetworkPerceptionSync | 132 | Character, Behavior |
| Variables | gc-variables-networked | NetworkVariablesSync | 190 | None (FOUNDATIONAL) |
| Behavior | gc-behavior-networked | NetworkBehaviorSync | 114 | Variables, Perception |
| Mailbox | gc-mailbox-networked | NetworkMailboxSync | 128 | Variables |
| Factions | gc-factions-networked | NetworkFactionsSync | 125 | Variables, Perception |
| Tactile | gc-tactile-networked | NetworkTactileSync | 181 | Variables |
| Quests | gc-quests-networked | NetworkQuestsSync | 133 | Variables, Inventory |
| Dialogue | gc-dialogue-networked | NetworkDialogueSync | 116 | Variables, Character |

## Dependency Graph

```
Character Core <- ARCHITECTURE (how Character component works internally)
    |
    +-- Motion, Mannequin, Animation, Input, IK, Driver systems
    +-- Foundation for gc-character-networked

Netcode Core <- INFRASTRUCTURE (Unity networking foundation)
    |
    +-- NetworkBehaviour, NetworkVariable, NetworkList, RPCs
    +-- Transport (UDP, WebSocket for WebGL)
    +-- Spawning, Pooling, Ownership, Authority
    +-- Foundation for ALL networked modules

Variables <- FOUNDATIONAL (all modules depend on this)
    |
    +-- Stats -> Inventory, Character
    +-- Behavior -> Perception -> Factions
    +-- Quests -> Dialogue
    +-- Mailbox (decoupled messaging)
    +-- Tactile (input layer)
```

## How to Use

1. **Load schema**: Read `.serena/personas/{module}.llm.txt`
2. **For character work**: Start with `character-core.llm.txt` then `character.llm.txt`
3. **For network work**: Start with `netcode-core.llm.txt` then specific module
4. **Check cross-integrations**: Each schema includes integration proposals
5. **Respect limitations**: Network constraints documented per module
6. **JSON summary**: `.serena/personas/_summary.json` for programmatic access

## Key Integration Points

- **Character + Netcode**: Use NetworkCharacterAdapter, NOT NetworkTransform
- **WebGL Games**: Require WebSocket transport configuration
- **Hotspot Interactions**: Validate ownership before execution
- **Player Targeting**: Use NetworkObjectId/ClientId, never direct references
