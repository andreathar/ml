# ML Project Structure

**Last Updated:** 2025-12-04
**Purpose:** Clean GameCreator Core + Netcode + Perception integration for multiplayer games

## Project Overview

This is a fresh Unity project with essential modules for multiplayer AI integration:
- **GameCreator Core** - Character, Visual Scripting, Variables
- **GameCreator Netcode Integration** - Official Unity Netcode for GameObjects bridge
- **GameCreator Perception** - AI awareness, senses (See, Hear, Smell, Feel), evidence

## Key Directories

### Unity Assets
- `Assets/Plugins/GameCreator/Packages/Core/` - GameCreator Core module
- `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/` - Netcode bridge
- `Assets/Plugins/GameCreator/Packages/Perception/` - AI perception system
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
| `GameCreator.Runtime.Perception` | `GameCreator.Runtime.Perception` | AI senses, awareness, evidence |
| `GameCreator.Editor.Perception` | `GameCreator.Editor.Perception` | Editor tooling for Perception |
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
- **GameCreator:** 2.x Core + Netcode + Perception

## Key Components

### GameCreator.Netcode.Runtime
| Component | Purpose |
|-----------|---------|
| `NetworkCharacter` | Extended Character with network properties |
| `NetworkCharacterAdapter` | Bridges Character ↔ NetworkBehaviour |
| `NetworkCharacterRegistry` | Tracks all network characters |
| `NetworkVariablesSync` | GameCreator Variables ↔ NetworkVariables |

### GameCreator.Runtime.Perception
| Component | Purpose |
|-----------|---------|
| `Perception` | Main AI perception component |
| `Cortex` | Awareness tracking (per-target via Trackers) |
| `Tracker` | Per-target awareness (0-1 float, stage enum) |
| `SensorSee` | Visual detection sensor |
| `SensorHear` | Audio detection sensor |
| `SensorSmell` | Scent detection sensor |
| `SensorFeel` | Proximity detection sensor |
| `Evidence` | Investigation targets |
| `Luminance` | Light level affecting visibility |
| `Camouflage` | Stealth modifier |
| `Din` | Ambient noise modifier |

## Active OpenSpec Changes

See `openspec list` for current proposals. Key active change:
- `add-perception-netcode-integration` - Network sync for Perception module (0/64 tasks)

## Reference Files

- `.serena/ai/critical.llm.txt` - Quick critical rules (~1000 tokens)
- `.serena/symbols/assemblies.json` - Unity assemblies
- `.serena/symbols/gamecreator_modules.json` - GameCreator modules
