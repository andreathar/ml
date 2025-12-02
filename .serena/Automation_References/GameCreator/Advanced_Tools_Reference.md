# GameCreator Advanced Tools Reference

**Source**: https://docs.gamecreator.io/gamecreator/advanced/
**Last Updated**: 2025-10-30
**Status**: ✅ Verified Reference

---

## Overview

GameCreator includes advanced tools and systems used throughout the ecosystem. This reference provides quick summaries with links to detailed documentation.

**Level**: Advanced (assumes Unity and GameCreator familiarity)

---

## Audio System

### 4-Channel Audio Architecture

GameCreator provides a 4-channel audio system for managing diegetic and non-diegetic sound effects.

**Features**:
- Volume control per channel
- Spatial audio support
- Easy sound effect triggering
- Music layer management

**Documentation**: https://docs.gamecreator.io/gamecreator/advanced/audio/

**Use Cases**:
- Background music layers
- UI sound effects (non-diegetic)
- 3D spatial sounds (diegetic)
- Dynamic audio mixing

---

## Signals System

### Custom Event Broadcasting

Communication system for broadcasting messages between game objects without direct references.

**How It Works**:
1. **Raise Signal** instruction broadcasts message with identifier
2. **On Receive Signal** trigger listens for specific identifier
3. Any number of listeners can respond to one signal

**Mark as Favorite**: Click star button to save frequently-used signal names for quick selection.

**Use Cases**:
- Decoupled event communication
- Global game state changes
- Cross-scene messaging
- Custom gameplay events

**Example**:
```
Signal ID: "player_died"
Broadcaster: Character health system
Listeners: UI manager, audio manager, game state controller
```

---

## Data Structures

Advanced Data Structures (ADS) are generic, serializable structures for specialized tasks.

### Unique ID
Uniquely identifies objects with serializable GUID.

**Docs**: https://docs.gamecreator.io/gamecreator/advanced/data-structures/unique-id/

### Singleton
Ensures zero or one instance of a class with global access.

**Docs**: https://docs.gamecreator.io/gamecreator/advanced/data-structures/singleton/

### Dictionary
Serializable key-value dictionary for Unity Inspector.

**Docs**: https://docs.gamecreator.io/gamecreator/advanced/data-structures/dictionary/

### Hash Set
Serializable hash set for unique element collections.

**Docs**: https://docs.gamecreator.io/gamecreator/advanced/data-structures/hashset/

### Linked List
Serializable linked list for ordered collections.

**Docs**: https://docs.gamecreator.io/gamecreator/advanced/data-structures/link-list/

### Matrix 2D
Serializable 2D matrix for grid-based data.

**Docs**: https://docs.gamecreator.io/gamecreator/advanced/data-structures/matrix-2d/

### Tree
Generic structure for acyclic parent-child dependencies.

**Docs**: https://docs.gamecreator.io/gamecreator/advanced/data-structures/tree/

**Use Cases**:
- Skill trees
- UI navigation hierarchies
- Dialogue trees

### Ring Buffer
Circular list where last element connects to first.

**Docs**: https://docs.gamecreator.io/gamecreator/advanced/data-structures/ring-buffer/

**Use Cases**:
- Weapon cycling
- Inventory rotation
- Camera mode switching

### State Machine
Dynamic state machine with independent node logic.

**Docs**: https://docs.gamecreator.io/gamecreator/advanced/data-structures/state-machine/

**Use Cases**:
- AI behavior states
- Game flow control
- Animation state management

### Spatial Hash
Advanced collision detection structure for infinite spatial domains with O(log n) complexity.

**Docs**: https://docs.gamecreator.io/gamecreator/advanced/data-structures/spatial-hash/

**Use Cases**:
- Large open worlds
- Efficient proximity queries
- Optimized collision detection

---

## Variables API

### Runtime Variable Manipulation

Access and modify Local/Global Variables programmatically.

**Documentation**: https://docs.gamecreator.io/gamecreator/advanced/variables-api/

### Local Variables (Component-based)

```csharp
using GameCreator.Runtime.Variables;

LocalNameVariables localVars = gameObject.Get<LocalNameVariables>();
if (localVars != null)
{
    // Get variable
    object value = localVars.Get("variableName");

    // Set variable
    localVars.Set("variableName", newValue);
}
```

### Global Variables (Singleton-based)

```csharp
using GameCreator.Runtime.Variables;

GlobalNameVariables globalVars = GlobalNameVariables.Instance;
if (globalVars != null)
{
    // Get variable
    object value = globalVars.Get("variableName");

    // Set variable
    globalVars.Set("variableName", newValue);
}
```

**Key Points**:
- Local Variables: Accessed via component on GameObject
- Global Variables: Accessed via singleton manager
- Type-safe casting required when retrieving values

---

## Properties System

### Dynamic Value Access

Properties allow dynamic value retrieval from multiple sources through dropdown menus.

**Documentation**: https://docs.gamecreator.io/gamecreator/advanced/properties/

### What Are Properties?

Properties are generic classes that retrieve values from different sources:
- Constant values
- Player position/rotation
- Camera transforms
- Local/Global Variables
- Component references
- Scene objects

### Example: PropertyGetPosition

Retrieves `Vector3` position from:
- Constant value
- Player position
- Main camera position
- Local Variable
- Global Variable
- Marker position
- Scene object position

### Creating Custom Properties

**Requires**: Unity UI Toolkit for Editor scripts

**Process**:
1. Inherit from base Property class
2. Implement value retrieval logic
3. Create UI Toolkit Editor script
4. Register in GameCreator system

**Documentation**: https://docs.gamecreator.io/gamecreator/advanced/properties/

---

## Saving & Loading

### Extensible Save System

Fully extensible system for tracking game progress and restoring state.

**Documentation**: https://docs.gamecreator.io/gamecreator/advanced/save-load-game/

### Core Interface: IGameSave

Implement `IGameSave` interface to make components saveable:

```csharp
using GameCreator.Runtime.Common;

public class MyComponent : MonoBehaviour, IGameSave
{
    private void OnEnable()
    {
        SaveLoadManager.Subscribe(this);
    }

    private void OnDisable()
    {
        SaveLoadManager.Unsubscribe(this);
    }

    public string SaveID => "my_component_unique_id";

    public object GetSaveData()
    {
        // Return data to save
        return new MySaveData();
    }

    public void LoadFromData(object data)
    {
        // Restore from saved data
        MySaveData saveData = (MySaveData)data;
    }
}
```

### Related Documentation

- **Saving Custom Data**: https://docs.gamecreator.io/gamecreator/advanced/save-load-game/custom-data/
- **Custom Databases**: https://docs.gamecreator.io/gamecreator/advanced/save-load-game/custom-database.md
- **Remember Component**: https://docs.gamecreator.io/gamecreator/advanced/save-load-game/remember/

### Remember Component

Special component for cherry-picking saveable data without implementing `IGameSave`.

**Use Cases**:
- Save object position/rotation
- Save object active state
- Save component values
- Selective state persistence

---

## Tweening System

### Automatic Frame Interpolation

Powerful "fire & forget" tweening system for smooth transitions.

**Documentation**: https://docs.gamecreator.io/gamecreator/advanced/tween/

### Features

- Linear interpolation
- Easing functions (ease in/out, bounce, elastic, etc.)
- Time-based or frame-based
- Callback support
- Cancellation support

### Basic Usage

```csharp
using GameCreator.Runtime.Common;

// Tween position over 2 seconds with ease-in-out
ITween tween = new TweenVector3(
    start: transform.position,
    end: targetPosition,
    duration: 2f,
    easing: Easing.Type.QuadInOut,
    onUpdate: (value) => transform.position = value
);

tween.Start();
```

### Easing Types

- Linear
- Quad (In/Out/InOut)
- Cubic (In/Out/InOut)
- Quart (In/Out/InOut)
- Quint (In/Out/InOut)
- Sine (In/Out/InOut)
- Expo (In/Out/InOut)
- Circ (In/Out/InOut)
- Bounce (In/Out/InOut)
- Elastic (In/Out/InOut)
- Back (In/Out/InOut)

**Use Cases**:
- UI animations
- Camera movements
- Object transitions
- Value interpolation

---

## Examples & Templates

### Reusable Content System

GameCreator and modules include examples/templates for games and applications.

**Documentation**: https://docs.gamecreator.io/gamecreator/advanced/custom-examples/

### Example Manager Window

Tool for managing examples:
- Install examples to project
- Uninstall examples
- Share examples (module developers)
- Version management

### Creating Custom Examples

**Process**:
1. Create example content (scenes, prefabs, scripts)
2. Package as Example asset
3. Define metadata (name, description, version)
4. Export for distribution

**Use Cases**:
- Tutorial scenes
- Gameplay templates
- Module demonstrations
- Reusable prefabs

---

## Domain Reload

### Play Mode Optimization

GameCreator supports skipping domain reloading for faster enter/exit play-mode.

**Setup**:
1. Open **Edit** → **Project Settings** → **Editor**
2. Enable **Enter Play Mode Options**
3. Disable **Reload Domain**

**Benefits**:
- Faster play mode entry
- Faster play mode exit
- Improved iteration speed
- Preserved static state (optional)

**Considerations**:
- Static fields not reset between play sessions
- Manual initialization may be required
- Test thoroughly with this setting

---

## Quick Reference Matrix

| Feature | Complexity | Use Case | Documentation |
|---------|-----------|----------|---------------|
| Audio System | Medium | Sound management | [Link](https://docs.gamecreator.io/gamecreator/advanced/audio/) |
| Signals | Low | Event broadcasting | Built-in |
| Data Structures | Medium | Specialized collections | [Link](https://docs.gamecreator.io/gamecreator/advanced/data-structures/) |
| Variables API | Low | Runtime variable access | [Link](https://docs.gamecreator.io/gamecreator/advanced/variables-api/) |
| Properties | High | Dynamic value access | [Link](https://docs.gamecreator.io/gamecreator/advanced/properties/) |
| Save/Load | Medium | Game state persistence | [Link](https://docs.gamecreator.io/gamecreator/advanced/save-load-game/) |
| Tweening | Medium | Smooth transitions | [Link](https://docs.gamecreator.io/gamecreator/advanced/tween/) |
| Examples | Low | Content management | [Link](https://docs.gamecreator.io/gamecreator/advanced/custom-examples/) |

---

## Integration Examples

### Example 1: Signal + Save System

```csharp
// When player levels up, broadcast signal and save
public void LevelUp()
{
    // Raise signal for all systems to respond
    SignalManager.Raise("player_level_up");

    // Save game state
    SaveLoadManager.Save(0); // Slot 0
}
```

### Example 2: Variables + Tweening

```csharp
// Tween global variable value smoothly
GlobalNameVariables globals = GlobalNameVariables.Instance;
float startValue = (float)globals.Get("health");
float endValue = 100f;

ITween tween = new TweenFloat(
    start: startValue,
    end: endValue,
    duration: 1f,
    easing: Easing.Type.QuadOut,
    onUpdate: (value) => globals.Set("health", value)
);

tween.Start();
```

### Example 3: Properties in Custom Instruction

```csharp
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

[Title("Move to Position")]
public class InstructionMoveToPosition : Instruction
{
    public PropertyGetGameObject target = GetGameObjectPlayer.Create();
    public PropertyGetPosition destination = GetPositionVector3.Create(Vector3.zero);

    protected override async Task Run(Args args)
    {
        GameObject targetObject = this.target.Get(args);
        Vector3 targetPosition = this.destination.Get(args);

        // Move logic here
    }
}
```

---

## Best Practices

### Audio
- Use separate channels for music, SFX, UI, and ambiance
- Preload frequently-used clips
- Clean up audio sources when done

### Signals
- Use descriptive signal identifiers: `"player_died"` not `"pd"`
- Mark frequently-used signals as favorites
- Document signal contracts (what data is passed)

### Data Structures
- Choose appropriate structure for use case
- Spatial Hash for large open worlds
- Ring Buffer for cycling systems
- State Machine for complex behaviors

### Variables
- Prefer Local Variables for per-instance data
- Use Global Variables for game-wide state
- Type-check when retrieving values
- Handle null cases gracefully

### Saving
- Always unsubscribe in OnDisable()
- Use unique SaveIDs (no duplicates)
- Test save/load thoroughly
- Handle versioning for data migration

### Tweening
- Store tween references if you need to cancel
- Choose appropriate easing for visual effect
- Don't tween every frame (performance)
- Clean up completed tweens

---

## Related Documentation

- `Creating_Custom_Instructions.md` - How to create custom Actions
- `Quick_Reference_Attributes.md` - Attribute cheat sheet
- **Official Docs**: https://docs.gamecreator.io/gamecreator/advanced/

---

**Status**: ✅ Complete Reference
**Verification**: Tested with GameCreator 2.0+
**Last Review**: 2025-10-30
