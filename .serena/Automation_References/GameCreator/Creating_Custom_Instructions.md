# Creating Custom Instructions for GameCreator

**Source**: https://docs.gamecreator.io/gamecreator/visual-scripting/actions/custom-instructions/
**Last Updated**: 2025-10-30
**Status**: ✅ Verified Reference

---

## Overview

GameCreator allows developers to create custom **Instructions** (also called Actions) that integrate seamlessly with the visual scripting system. This guide covers the complete process from template creation to decoration and documentation.

**Programming Knowledge Required**: This assumes some C# and Unity knowledge.

---

## Quick Start

### Step 1: Create Template Script

**Right-click** in Project panel → **Create** → **Game Creator** → **Developer** → **C# Instruction**

This creates a boilerplate template:

```csharp
using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

[Serializable]
public class MyInstruction : Instruction
{
    protected override Task Run(Args args)
    {
        // Your code here...
        return DefaultResult;
    }
}
```

### Step 2: Implement Logic

Override the `Run()` method with your instruction's behavior.

### Step 3: Add Decoration

Use attributes to document and organize your instruction (see Decoration section below).

---

## Anatomy of an Instruction

### Core Structure

**Inheritance**: All instructions inherit from `Instruction` base class.

**Entry Point**: The `Run(Args args)` method is called when the instruction executes.

**Args Parameter**: Helper class containing:
- `args.Self` - GameObject that initiated the call
- `args.Target` - Targeted GameObject (if any)

**Return Type**: `Task` (supports async/await)

### Example: Simple Instruction

```csharp
using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

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

---

## Time Management (Async/Await)

Instructions use `async/await` to manage execution flow over time.

### Enable Async

Add `async` keyword to method signature:

```csharp
protected override async Task Run(Args args)
{
    // Can now use await
}
```

### NextFrame() - Pause for One Frame

```csharp
protected override async Task Run(Args args)
{
    Debug.Log("Frame 1");
    await this.NextFrame();
    Debug.Log("Frame 2");
}
```

### Time(float seconds) - Pause for Duration

```csharp
protected override async Task Run(Args args)
{
    Debug.Log("Start");
    await this.Time(5f); // Wait 5 seconds
    Debug.Log("5 seconds later");
}
```

### While(Func<bool>) - Pause While Condition True

Executes every frame until condition returns `false`:

```csharp
protected override async Task Run(Args args)
{
    await this.While(() => this.IsPlayerMoving());
    Debug.Log("Player stopped moving");
}

private bool IsPlayerMoving()
{
    // Your logic here
    return false;
}
```

### Until(Func<bool>) - Pause Until Condition True

Executes every frame until condition returns `true`:

```csharp
protected override async Task Run(Args args)
{
    await this.Until(() => this.PlayerHasReachedDestination());
    Debug.Log("Player reached destination");
}

private bool PlayerHasReachedDestination()
{
    // Your logic here
    return true;
}
```

---

## Decoration & Documentation

Use class attributes to document and organize instructions.

### [Title("Name")] - Instruction Title

Sets the display name in the editor. If omitted, uses beautified class name.

```csharp
[Title("Hello World")]
public class MyInstruction : Instruction { }
```

### [Description("Text")] - What It Does

Describes the instruction's purpose. Used in floating documentation and Hub uploads.

```csharp
[Description("Logs a custom message to the Unity console")]
```

### [Image(typeof(Icon), Color)] - Custom Icon

Changes the default icon. Requires icon type and color.

**Example**: Red solid cube

```csharp
[Image(typeof(IconCubeSolid), Color.red)]
```

**Available Icons**: GameCreator includes many `IIcon` derived classes (IconCubeSolid, IconSphere, etc.)

### [Category("Path/To/Category")] - Organization

Organizes instructions in dropdown menus using slash-separated paths.

```csharp
[Category("Combat/Weapons/Shooting")]
```

This creates: **Combat** → **Weapons** → **Shooting** → Instruction Name

### [Version(major, minor, patch)] - Semantic Versioning

Tracks instruction development. Required when uploading to GameCreator Hub.

```csharp
[Version(1, 5, 3)] // Version 1.5.3
```

**Format**: Major.Minor.Patch (see https://semver.org)

### [Parameter("Name", "Description")] - Field Documentation

Documents exposed fields in the Inspector. Add one attribute per field.

```csharp
[Parameter("Wait For Time", "Whether to wait or not")]
[Parameter("Duration", "The amount of seconds to wait")]
public class MyInstruction : Instruction
{
    public bool waitForTime = true;
    public float duration = 5f;
}
```

### [Keywords("word1", "word2")] - Search Optimization

Adds search terms for the fuzzy finder. Useful for synonyms.

```csharp
[Keywords("Move", "Translate", "Position")]
```

**Example**: "Change Position" instruction doesn't mention "move" but users might search for it.

### [Example("text")] - Usage Examples

Shows usage examples. Multiple `[Example]` attributes allowed. **Supports Markdown**.

```csharp
[Example("Use this instruction to log debug information during development.")]
[Example(@"
    **Example 1**: Log player position

    1. Create a new Trigger
    2. Add this instruction
    3. Set message to 'Player position: {0}'
")]
```

**Multi-line**: Use `@` prefix and double line breaks for paragraphs.

### [Dependency("module.id", major, minor, patch)] - Module Requirements

Specifies required modules (for GameCreator Hub uploads). Prevents downloads if module missing.

```csharp
[Dependency("gamecreator.inventory", 1, 5, 2)]
```

**Parameters**:
- Module ID (e.g., `gamecreator.inventory`)
- Minimum major version
- Minimum minor version
- Minimum patch version

---

## Complete Example: Fully Decorated Instruction

```csharp
using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Title("Wait and Log")]
[Description("Waits for a specified duration then logs a message")]
[Image(typeof(IconClock), Color.yellow)]
[Category("Debug/Time")]
[Version(1, 0, 0)]
[Parameter("Duration", "How many seconds to wait")]
[Parameter("Message", "The message to log after waiting")]
[Keywords("Wait", "Delay", "Log", "Debug")]
[Example("Useful for debugging time-dependent behavior")]
[Example(@"
    **Example Usage**:

    1. Add this instruction to a Trigger
    2. Set duration to 3 seconds
    3. Set message to 'Timer Complete'
    4. Run the scene and observe the console after 3 seconds
")]
[Serializable]
public class InstructionWaitAndLog : Instruction
{
    [SerializeField]
    private float duration = 1f;

    [SerializeField]
    private string message = "Timer finished!";

    protected override async Task Run(Args args)
    {
        await this.Time(duration);
        Debug.Log($"[{args.Self.name}] {message}");
        return;
    }
}
```

---

## Best Practices

### 1. Always Decorate
Add at least `[Title]`, `[Description]`, and `[Category]` to every instruction.

### 2. Use Meaningful Categories
Organize by feature area: `"Combat/Melee"`, `"UI/Dialogs"`, `"Audio/Music"`

### 3. Document Parameters
Always add `[Parameter]` attributes for public fields.

### 4. Add Keywords
Include synonyms and related terms users might search for.

### 5. Version Your Work
Start at `[Version(1, 0, 0)]` and increment appropriately.

### 6. Provide Examples
Show real-world usage scenarios with `[Example]` attributes.

### 7. Use Async Appropriately
Only add `async` keyword if you're using `await` inside the method.

### 8. Test Thoroughly
Test instructions in multiple scenarios before sharing.

---

## Common Patterns

### Pattern 1: Instant Execution

```csharp
protected override Task Run(Args args)
{
    // Do something immediately
    return DefaultResult;
}
```

### Pattern 2: Time-Based Delay

```csharp
protected override async Task Run(Args args)
{
    await this.Time(delaySeconds);
    // Do something after delay
}
```

### Pattern 3: Condition-Based Waiting

```csharp
protected override async Task Run(Args args)
{
    await this.Until(() => conditionMet);
    // Do something when condition is met
}
```

### Pattern 4: Frame-by-Frame Processing

```csharp
protected override async Task Run(Args args)
{
    for (int i = 0; i < iterations; i++)
    {
        // Do something each frame
        await this.NextFrame();
    }
}
```

---

## Accessing GameCreator Systems

### Get Character Component

```csharp
Character character = args.Self.Get<Character>();
if (character != null)
{
    // Use character
}
```

### Get Properties

```csharp
using GameCreator.Runtime.Characters;

public PropertyGetGameObject target = GetGameObjectPlayer.Create();

protected override Task Run(Args args)
{
    GameObject targetObject = this.target.Get(args);
    // Use targetObject
    return DefaultResult;
}
```

### Access Variables

```csharp
using GameCreator.Runtime.Variables;

LocalNameVariables localVars = args.Self.Get<LocalNameVariables>();
if (localVars != null)
{
    object value = localVars.Get("variableName");
}
```

---

## Troubleshooting

### Instruction Not Appearing in List

**Check**:
- Class inherits from `Instruction`
- Class is `[Serializable]`
- Script compiled without errors
- Try refreshing Unity (Ctrl+R)

### Async/Await Errors

**Issue**: `await` not recognized

**Solution**: Add `async` keyword to method signature

```csharp
protected override async Task Run(Args args)
{
    await this.Time(1f);
}
```

### Icon Not Displaying

**Check**:
- Icon type exists (e.g., `typeof(IconCubeSolid)`)
- Color is valid (use `UnityEngine.Color`)
- Assembly references are correct

---

## Resources

- **Official Docs**: https://docs.gamecreator.io/gamecreator/visual-scripting/actions/custom-instructions/
- **GameCreator Hub**: https://gamecreator.io/hub
- **Semantic Versioning**: https://semver.org
- **Markdown Guide**: https://www.markdownguide.org

---

## Related Documentation

- `Advanced_Tools_Reference.md` - Advanced GameCreator features
- `Quick_Reference_Attributes.md` - Attribute cheat sheet
- **Properties System**: https://docs.gamecreator.io/gamecreator/advanced/properties/
- **Variables API**: https://docs.gamecreator.io/gamecreator/advanced/variables-api/

---

**Status**: ✅ Complete Guide
**Verification**: Tested with GameCreator 2.0+
**Last Review**: 2025-10-30
