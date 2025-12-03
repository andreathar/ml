# ML Project Structure

**Last Updated:** 2025-12-03
**Purpose:** Clean GameCreator Core + Netcode integration for multiplayer games

## Project Overview

This is a fresh Unity project with only the essential modules for perfect multiplayer integration:
- **GameCreator Core** - Character, Visual Scripting, Variables
- **GameCreator Netcode Integration** - Official Unity Netcode for GameObjects bridge

## Key Directories

### Unity Assets
- `Assets/Plugins/GameCreator/Packages/Core/` - GameCreator Core module
- `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/` - Netcode bridge
- `Assets/Scripts/` - Project-specific scripts
- `Assets/Scenes/` - Game scenes
- `Assets/Resources/` - Runtime-loaded assets
- `Assets/Settings/` - Unity project settings

### AI/Development Infrastructure
- `.serena/memories/` - AI knowledge base (tiered: CRITICAL, CONTEXT, TOOLS, INTEGRATION)
- `.serena/personas/` - Module-specific schemas
- `.serena/Automation_References/` - Reference documentation
- `openspec/` - Spec-driven development (proposals, specs, decision trees)
- `openspec/.decision-trees/` - Interactive decision trees for networking decisions

## Assembly References

### GameCreator Assemblies
| Assembly | Namespace | Purpose |
|----------|-----------|---------|
| `GameCreator.Runtime.Core` | `GameCreator.Runtime.Core` | Characters, Visual Scripting, Variables |
| `GameCreator.Editor.Core` | `GameCreator.Editor.Core` | Editor tooling for Core |
| `GameCreator.Netcode.Runtime` | `GameCreator.Netcode.Runtime` | Network sync, RPCs, ownership |
| `GameCreator.Netcode.Editor` | `GameCreator.Netcode.Editor` | Editor tooling for Netcode |

### Unity Netcode Dependencies
| Assembly | Purpose |
|----------|---------|
| `Unity.Netcode.Runtime` | Core networking runtime |
| `Unity.Collections` | Native collections for networking |
| `Unity.InputSystem` | Input handling |
| `Unity.TextMeshPro` | UI text |
| `Unity.Mathematics` | Math utilities |

## Tech Stack

- **Unity:** 6000.x (Unity 6)
- **Language:** C# 9.0+
- **Netcode:** Unity Netcode for GameObjects 2.x (`UNITY_NETCODE_2` define)
- **GameCreator:** 2.x Core + Netcode Integration

## Key Components (GameCreator.Netcode.Runtime)

### Components/
| Component | Purpose |
|-----------|---------|
| `NetworkCharacter` | Extended Character with network properties |
| `NetworkCharacterAdapter` | Bridges Character ↔ NetworkBehaviour |
| `NetworkCharacterRegistry` | Tracks all network characters |

### Driver/
| Component | Purpose |
|-----------|---------|
| `NetworkUnitDriverController` | Network-aware movement driver |

### Motion/
| Component | Purpose |
|-----------|---------|
| `NetworkCharacterMotion` | Synced character motion |

### Sync/
| Component | Purpose |
|-----------|---------|
| `NetworkVariablesSync` | GameCreator Variables ↔ NetworkVariables |

### VisualScripting/

**Instructions:**
- `InstructionNetworkStartHost` - Start as host
- `InstructionNetworkStartClient` - Start as client
- `InstructionNetworkDisconnect` - Disconnect from network
- `InstructionNetworkSpawnPlayer` - Spawn player character
- `InstructionNetworkDespawn` - Despawn network object
- `InstructionNetworkChangeOwnership` - Transfer ownership

**Conditions:**
- `ConditionNetworkIsHost` - Check if host
- `ConditionNetworkIsClient` - Check if client
- `ConditionNetworkIsServer` - Check if server
- `ConditionNetworkIsOwner` - Check if owner
- `ConditionNetworkIsLocalPlayer` - Check if local player
- `ConditionNetworkIsConnected` - Check if connected
- `ConditionNetworkIsSpawned` - Check if spawned

**Triggers:**
- `EventNetworkOnClientConnected` - Client connected
- `EventNetworkOnClientDisconnected` - Client disconnected
- `EventNetworkOnLocalPlayerSpawned` - Local player spawned
- `EventNetworkOnNetworkSpawn` - Object spawned
- `EventNetworkOnNetworkDespawn` - Object despawned
- `EventNetworkOnOwnershipChanged` - Ownership changed

**Properties:**
- `GetGameObjectNetworkLocalPlayer` - Get local player GameObject
- `GetGameObjectNetworkPlayerByClientId` - Get player by client ID

## Reference Files

- `.serena/ai/critical.llm.txt` - Quick critical rules (~1000 tokens)
- `.serena/symbols/assemblies.json` - Unity assemblies
- `.serena/symbols/gamecreator_modules.json` - GameCreator modules
