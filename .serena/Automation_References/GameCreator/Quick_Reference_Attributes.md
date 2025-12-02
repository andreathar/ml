# GameCreator Instruction Attributes - Quick Reference

**Source**: https://docs.gamecreator.io/gamecreator/visual-scripting/actions/custom-instructions/
**Last Updated**: 2025-10-30
**Status**: ✅ Cheat Sheet

---

## Essential Attributes (Always Use)

```csharp
[Title("Instruction Name")]
[Description("What this instruction does")]
[Category("Path/To/Category")]
[Serializable]
public class MyInstruction : Instruction { }
```

---

## Complete Attribute Reference

### [Title]

**Purpose**: Display name in editor
**Default**: Beautified class name
**Required**: No (but highly recommended)

```csharp
[Title("Move Player")]
[Title("Wait For Seconds")]
[Title("Play Sound Effect")]
```

---

### [Description]

**Purpose**: Explains what instruction does
**Used In**: Floating docs, Hub uploads
**Required**: No (but highly recommended)

```csharp
[Description("Moves the player character to a target position")]
[Description("Pauses execution for a specified duration")]
[Description("Plays a one-shot audio clip at a position")]
```

---

### [Image]

**Purpose**: Custom icon for instruction
**Parameters**: `(typeof(IconType), Color)`
**Required**: No

```csharp
// Red cube
[Image(typeof(IconCubeSolid), Color.red)]

// Yellow clock
[Image(typeof(IconClock), Color.yellow)]

// Blue character
[Image(typeof(IconCharacter), Color.blue)]

// Green checkmark
[Image(typeof(IconCheck), Color.green)]
```

**Common Icons**:
- `IconCubeSolid` - Solid cube
- `IconCube` - Wireframe cube
- `IconSphere` - Sphere
- `IconClock` - Clock/time
- `IconCharacter` - Character
- `IconCheck` - Checkmark
- `IconCross` - X/cross
- `IconArrow` - Arrow
- `IconHeart` - Heart
- `IconStar` - Star

---

### [Category]

**Purpose**: Organize in dropdown menus
**Format**: Slash-separated path
**Required**: No (but highly recommended)

```csharp
[Category("Combat/Melee")]
[Category("UI/Dialogs/Speech")]
[Category("Audio/Music")]
[Category("Debug/Logging")]
[Category("Movement/Jump")]
```

**Common Categories**:
- `"Combat/Melee"` - Melee combat
- `"Combat/Ranged"` - Ranged combat
- `"Movement/Walk"` - Walking movement
- `"Movement/Jump"` - Jumping
- `"Audio/Music"` - Music control
- `"Audio/SFX"` - Sound effects
- `"UI/Dialogs"` - Dialog systems
- `"UI/Menus"` - Menu systems
- `"Debug/Logging"` - Debug logs
- `"Variables/Set"` - Variable setters
- `"Variables/Get"` - Variable getters

---

### [Version]

**Purpose**: Semantic versioning
**Format**: `(major, minor, patch)`
**Required**: For Hub uploads

```csharp
[Version(1, 0, 0)] // Initial release
[Version(1, 5, 3)] // Bug fix release
[Version(2, 0, 0)] // Breaking changes
```

**When to Increment**:
- **Major**: Breaking changes
- **Minor**: New features (backward compatible)
- **Patch**: Bug fixes

**Reference**: https://semver.org

---

### [Parameter]

**Purpose**: Document exposed fields
**Format**: `("Field Name", "Description")`
**Required**: One per public field

```csharp
[Parameter("Duration", "How many seconds to wait")]
[Parameter("Target", "The GameObject to move")]
[Parameter("Speed", "Movement speed in units per second")]

public class MyInstruction : Instruction
{
    public float duration = 1f;
    public GameObject target;
    public float speed = 5f;
}
```

---

### [Keywords]

**Purpose**: Search optimization
**Format**: Multiple strings
**Required**: No (but helpful)

```csharp
// "Change Position" instruction
[Keywords("Move", "Translate", "Teleport")]

// "Wait" instruction
[Keywords("Delay", "Pause", "Sleep")]

// "Play Audio" instruction
[Keywords("Sound", "Music", "SFX", "Clip")]
```

**Best Practice**: Include synonyms and related terms.

---

### [Example]

**Purpose**: Usage examples
**Format**: String (supports Markdown)
**Multiple**: Yes
**Required**: No (but helpful for Hub)

```csharp
[Example("Use this to log debug messages during development")]

[Example(@"
    **Example 1**: Log player position

    1. Create a Trigger
    2. Add this instruction
    3. Run the scene
")]

[Example("Useful for debugging AI behavior")]
```

**Multi-line Format**:
- Use `@` prefix for multi-line strings
- Double line break = new paragraph
- Supports full Markdown syntax

---

### [Dependency]

**Purpose**: Required module check (Hub only)
**Format**: `("module.id", major, minor, patch)`
**Required**: Only for Hub uploads with dependencies

```csharp
[Dependency("gamecreator.inventory", 1, 5, 2)]
[Dependency("gamecreator.stats", 2, 0, 0)]
[Dependency("gamecreator.dialogue", 1, 0, 0)]
```

**Module IDs**:
- `"gamecreator.inventory"` - Inventory module
- `"gamecreator.stats"` - Stats module
- `"gamecreator.dialogue"` - Dialogue module
- `"gamecreator.quests"` - Quests module
- `"gamecreator.behavior"` - Behavior module
- `"gamecreator.shooter"` - Shooter module
- `"gamecreator.melee"` - Melee module
- `"gamecreator.perception"` - Perception module

---

## Complete Template

### Minimal (Recommended Minimum)

```csharp
using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

[Title("My Instruction")]
[Description("What it does")]
[Category("Category/Subcategory")]
[Serializable]
public class MyInstruction : Instruction
{
    protected override Task Run(Args args)
    {
        // Your code here
        return DefaultResult;
    }
}
```

### Full (All Attributes)

```csharp
using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Title("Complete Instruction")]
[Description("A fully documented instruction with all attributes")]
[Image(typeof(IconCubeSolid), Color.green)]
[Category("Examples/Complete")]
[Version(1, 0, 0)]
[Parameter("Duration", "How long to run")]
[Parameter("Target", "The target object")]
[Keywords("Example", "Complete", "Template")]
[Example("This is a complete example")]
[Example(@"
    **Usage**:

    1. Add to Trigger
    2. Configure parameters
    3. Test in play mode
")]
[Dependency("gamecreator.inventory", 1, 0, 0)]
[Serializable]
public class CompleteInstruction : Instruction
{
    [SerializeField]
    private float duration = 1f;

    [SerializeField]
    private PropertyGetGameObject target = GetGameObjectPlayer.Create();

    protected override async Task Run(Args args)
    {
        await this.Time(duration);
        GameObject targetObj = this.target.Get(args);
        // Your code here
    }
}
```

---

## Copy-Paste Templates

### Basic Log Instruction

```csharp
[Title("Log Message")]
[Description("Logs a message to the Unity console")]
[Category("Debug/Logging")]
[Serializable]
public class InstructionLogMessage : Instruction
{
    public string message = "Hello World";

    protected override Task Run(Args args)
    {
        Debug.Log(message);
        return DefaultResult;
    }
}
```

### Wait Instruction

```csharp
[Title("Wait")]
[Description("Pauses execution for a specified duration")]
[Category("Time/Wait")]
[Parameter("Duration", "Seconds to wait")]
[Serializable]
public class InstructionWait : Instruction
{
    public float duration = 1f;

    protected override async Task Run(Args args)
    {
        await this.Time(duration);
    }
}
```

### Move Object Instruction

```csharp
[Title("Move Object")]
[Description("Moves a GameObject to a target position")]
[Category("Movement/Basic")]
[Parameter("Target", "Object to move")]
[Parameter("Destination", "Where to move")]
[Parameter("Duration", "How long the movement takes")]
[Serializable]
public class InstructionMoveObject : Instruction
{
    public PropertyGetGameObject target = GetGameObjectPlayer.Create();
    public PropertyGetPosition destination = GetPositionVector3.Create(Vector3.zero);
    public float duration = 1f;

    protected override async Task Run(Args args)
    {
        GameObject targetObj = this.target.Get(args);
        Vector3 targetPos = this.destination.Get(args);

        Vector3 startPos = targetObj.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            targetObj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            await this.NextFrame();
        }

        targetObj.transform.position = targetPos;
    }
}
```

---

## Attribute Checklist

Use this checklist when creating instructions:

- [ ] `[Title]` - Clear, descriptive name
- [ ] `[Description]` - What it does and why
- [ ] `[Category]` - Logical organization path
- [ ] `[Image]` - Appropriate icon and color
- [ ] `[Version]` - Starting at 1.0.0
- [ ] `[Parameter]` - One per public field
- [ ] `[Keywords]` - Synonyms and related terms
- [ ] `[Example]` - At least one usage example
- [ ] `[Dependency]` - If uses other modules
- [ ] `[Serializable]` - On class definition
- [ ] Inherits from `Instruction`
- [ ] Overrides `Run(Args args)` method

---

## Quick Tips

### ✅ DO

- Always add `[Title]`, `[Description]`, `[Category]`
- Document all public fields with `[Parameter]`
- Use meaningful category paths
- Add keywords for discoverability
- Provide usage examples
- Version from 1.0.0
- Test thoroughly before sharing

### ❌ DON'T

- Skip basic attributes
- Use generic/vague titles
- Forget `[Serializable]`
- Leave public fields undocumented
- Mix unrelated functionality
- Break semantic versioning rules
- Upload without testing

---

## Common Patterns

### Instant Execution

```csharp
protected override Task Run(Args args)
{
    // Execute immediately
    return DefaultResult;
}
```

### Timed Delay

```csharp
protected override async Task Run(Args args)
{
    await this.Time(seconds);
    // Execute after delay
}
```

### Wait for Condition

```csharp
protected override async Task Run(Args args)
{
    await this.Until(() => condition);
    // Execute when condition is true
}
```

### Frame-by-Frame

```csharp
protected override async Task Run(Args args)
{
    while (running)
    {
        // Execute each frame
        await this.NextFrame();
    }
}
```

---

## Icon Color Conventions

**Recommendation**: Use color to indicate instruction category:

- 🔴 **Red**: Danger, destruction, combat
- 🟠 **Orange**: Warnings, alerts
- 🟡 **Yellow**: Time, waiting, delays
- 🟢 **Green**: Success, creation, health
- 🔵 **Blue**: Information, UI, water
- 🟣 **Purple**: Magic, special effects
- ⚫ **Gray**: Utilities, debug, neutral
- ⚪ **White**: General purpose

---

## Related Documentation

- `Creating_Custom_Instructions.md` - Detailed guide
- `Advanced_Tools_Reference.md` - Advanced features
- **Official Docs**: https://docs.gamecreator.io/gamecreator/visual-scripting/actions/custom-instructions/

---

**Status**: ✅ Quick Reference Complete
**Print-Friendly**: Yes
**Last Review**: 2025-10-30
