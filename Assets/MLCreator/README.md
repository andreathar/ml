# MLCreator

Custom code for the MLCreator project, organized following GameCreator patterns.

## Directory Structure

```
MLCreator/
├── Runtime/                    # Runtime code (builds into game)
│   ├── Core/                   # Core systems, managers, initialization
│   │   ├── Managers/           # Singleton managers
│   │   ├── Initialization/     # Bootstrap code
│   │   └── Utilities/          # Shared utilities
│   ├── Gameplay/               # Game mechanics
│   │   ├── Effects/            # Visual effects (ProximityColorChanger)
│   │   └── Interactions/       # Interactive systems
│   ├── Multiplayer/            # Network synchronization
│   │   ├── Sync/               # State synchronization
│   │   └── RPC/                # Remote procedure calls
│   ├── AI/                     # Artificial intelligence
│   │   ├── Behaviors/          # AI state machines
│   │   └── Sensors/            # Custom sensing
│   └── UI/                     # User interface
│       ├── HUD/                # Heads-up display
│       └── Menus/              # Menu systems
├── Editor/                     # Editor-only code
│   ├── Core/                   # Editor core utilities
│   ├── Tools/                  # Development tools (HotReloadHandler)
│   ├── Inspectors/             # Custom inspectors
│   └── Drawers/                # Property drawers
├── MLCreator.Runtime.asmdef    # Runtime assembly definition
└── MLCreator.Editor.asmdef     # Editor assembly definition
```

## Namespaces

All code follows the pattern: `MLCreator.{Context}.{Module}.{Feature}`

### Runtime Namespaces
- `MLCreator.Runtime.Core` - Core systems
- `MLCreator.Runtime.Gameplay.Effects` - Visual effects
- `MLCreator.Runtime.Gameplay.Interactions` - Interactive systems
- `MLCreator.Runtime.Multiplayer.Sync` - Network synchronization
- `MLCreator.Runtime.AI.Behaviors` - AI state machines
- `MLCreator.Runtime.UI.HUD` - HUD components

### Editor Namespaces
- `MLCreator.Editor.Tools` - Development tools
- `MLCreator.Editor.Inspectors` - Custom inspectors
- `MLCreator.Editor.Drawers` - Property drawers

## Coding Conventions

### Naming
- Classes: `PascalCase` (e.g., `ProximityColorChanger`)
- Private fields: `m_` prefix (e.g., `m_DetectionRadius`)
- Static fields: `s_` prefix (e.g., `s_Instance`)
- Constants: `UPPER_SNAKE_CASE` (e.g., `DEFAULT_PORT`)
- Methods: `PascalCase` (e.g., `OnEditorUpdate`)

### Patterns
- Use `[SerializeField]` for inspector-exposed private fields
- Manager classes use singleton pattern
- Follow GameCreator attribute conventions for visual scripting nodes

## Claude Skills

Use these slash commands for AI-assisted development:
- `/mlcreator-core` - Core systems guidance
- `/mlcreator-gameplay` - Gameplay features
- `/mlcreator-multiplayer` - Network code
- `/mlcreator-ai` - AI systems
- `/mlcreator-ui` - UI components
- `/mlcreator-editor` - Editor tools
