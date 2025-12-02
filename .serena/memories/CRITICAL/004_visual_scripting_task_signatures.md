# Visual Scripting Task Signatures (GameCreator)

**Priority:** P0 CRITICAL
**Last Updated:** 2025-11-23
**Applies To:** All GameCreator Instruction and Condition implementations

## CRITICAL: Task Signature Pattern

### Instructions: Task Run(Args args)

**The ONLY correct signature for GameCreator Instructions:**

```csharp
public class MyInstruction : Instruction
{
    // ✅ CORRECT - This is the ONLY valid signature
    protected override Task Run(Args args)
    {
        // Implementation
        return DefaultResult;
    }

    // ❌ WRONG - Do NOT add CancellationToken
    protected override Task Run(Args args, CancellationToken ct)
    {
        // This is NOT the GameCreator pattern!
    }

    // ❌ WRONG - Do NOT add other parameters
    protected override Task Run(Args args, GameObject target)
    {
        // GameCreator Instructions only take Args!
    }
}
```

### Why This Matters

**GameCreator's visual scripting system expects exactly:**
- Method name: `Run`
- Parameters: `Args args` only
- Return type: `Task`
- Modifier: `protected override`

**If you deviate:**
- Runtime errors when visual scripts try to call your Instruction
- Silent failures (Instruction won't execute)
- Visual scripting editor may not recognize your Instruction

## Conditions: bool Check(Args args)

**The ONLY correct signature for GameCreator Conditions:**

```csharp
public class MyCondition : Condition
{
    // ✅ CORRECT
    protected override bool Check(Args args)
    {
        // Return true/false based on condition
        return someCondition;
    }

    // ❌ WRONG - Conditions return bool, not Task
    protected override Task<bool> Check(Args args)
    {
        // This is NOT the pattern!
    }
}
```

## Args Parameter Pattern

### Accessing GameObject Context
```csharp
protected override Task Run(Args args)
{
    // Get the GameObject this Instruction is running on
    GameObject target = args.Self;

    // Get Character if available
    Character character = args.Self.Get<Character>();

    // Common pattern
    if (character != null)
    {
        // Do something with character
        character.Motion.MoveToDirection(Vector3.forward);
    }

    return DefaultResult;
}
```

### Accessing Variables
```csharp
protected override Task Run(Args args)
{
    // GameCreator uses its own variable system
    // Access via LocalVariables or GlobalVariables

    var health = args.Self.Get<LocalVariables>().Get("health");
    Debug.Log($"Health: {health}");

    return DefaultResult;
}
```

## Async Operations in Instructions

### The Right Way to Handle Async
```csharp
protected override async Task Run(Args args)
{
    Character character = args.Self.Get<Character>();

    // Start async operation
    await character.Animator.CrossFade("Attack", 0.2f);

    // Wait for animation completion
    await Task.Delay(1000); // Or await animation event

    // Return when complete
    return;
}
```

### Using DefaultResult
```csharp
protected override Task Run(Args args)
{
    // Synchronous operation - return immediately
    Character character = args.Self.Get<Character>();
    character.Combat.Health.Current -= 10;

    return DefaultResult; // Equivalent to Task.CompletedTask
}
```

## Multiplayer Instructions Pattern

### Network-Aware Instruction
```csharp
public class NetworkInstruction : Instruction
{
    protected override Task Run(Args args)
    {
        Character character = args.Self.Get<Character>();

        // Check if networked and if we're the owner
        if (character.IsNetworkSpawned && !character.IsNetworkOwner)
        {
            // Non-owners don't execute local-only instructions
            return DefaultResult;
        }

        // Execute for owner or offline
        ExecuteLocalAction(character);

        // If networked and owner, send to server
        if (character.IsNetworkSpawned)
        {
            var networkBehaviour = character.Get<MyNetworkBehaviour>();
            networkBehaviour?.ExecuteServerRpc();
        }

        return DefaultResult;
    }
}
```

## Common Patterns

### Movement Instruction
```csharp
public class MoveToPosition : Instruction
{
    [SerializeField] private PropertyGetPosition m_Target;

    protected override Task Run(Args args)
    {
        Character character = args.Self.Get<Character>();
        if (character == null) return DefaultResult;

        Vector3 targetPos = m_Target.Get(args);
        character.Motion.MoveToPosition(targetPos);

        return DefaultResult;
    }
}
```

### Condition Check
```csharp
public class IsCharacterGrounded : Condition
{
    protected override bool Check(Args args)
    {
        Character character = args.Self.Get<Character>();
        if (character == null) return false;

        return character.Driver.IsGrounded;
    }
}
```

### Async Wait Instruction
```csharp
public class WaitForAnimation : Instruction
{
    [SerializeField] private string m_AnimationName;

    protected override async Task Run(Args args)
    {
        Character character = args.Self.Get<Character>();
        if (character == null) return;

        // Play animation
        await character.Animator.CrossFade(m_AnimationName, 0.2f);

        // Wait for animation to complete
        float animLength = character.Animator.GetAnimationLength(m_AnimationName);
        await Task.Delay((int)(animLength * 1000));
    }
}
```

## Visual Scripting Editor Integration

### Icon Assignment
```csharp
public class MyInstruction : Instruction
{
    public override string Title => "My Instruction Title";

    // Use existing GameCreator icons
    public override Texture2D Icon => EditorIcons.Character; // ✅ Use built-in icons

    // DON'T create custom icons unless absolutely necessary
    // public override Texture2D Icon => LoadCustomIcon(); // ❌ Avoid
}
```

### Category Assignment
```csharp
[Category("Multiplayer/Character")]
public class NetworkMoveInstruction : Instruction
{
    // Places this Instruction in "Multiplayer > Character" category in visual scripting
}
```

## Common Pitfalls

### ❌ Adding CancellationToken
```csharp
// This is a COMMON mistake - don't do it!
protected override Task Run(Args args, CancellationToken ct)
{
    // GameCreator doesn't pass CancellationToken to Instructions
}
```

### ❌ Wrong Return Type
```csharp
protected override void Run(Args args) // Wrong - must return Task
{
}

protected override IEnumerator Run(Args args) // Wrong - this isn't Unity coroutine
{
}
```

### ❌ Public Instead of Protected
```csharp
public override Task Run(Args args) // Should be 'protected'
{
}
```

### ❌ Not Checking for Null
```csharp
protected override Task Run(Args args)
{
    Character character = args.Self.Get<Character>();
    character.Motion.Move(Vector3.forward); // CRASH if character is null!

    // ✅ ALWAYS check
    if (character == null) return DefaultResult;
}
```

## Testing Pattern

```csharp
[UnityTest]
public IEnumerator Instruction_WhenRun_ExecutesCorrectly()
{
    // Arrange
    var character = CreateCharacter();
    var instruction = new MyInstruction();
    var args = new Args(character.gameObject);

    // Act
    var task = instruction.Run(args);
    yield return new WaitUntil(() => task.IsCompleted);

    // Assert
    Assert.IsTrue(task.IsCompletedSuccessfully);
    Assert.AreEqual(expectedResult, character.SomeProperty);
}
```

## Namespace and Using Directives

```csharp
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.VisualScripting;
using System.Threading.Tasks;
using UnityEngine;

namespace GameCreator.Multiplayer.Runtime.VisualScripting
{
    [Title("My Instruction")]
    [Category("Multiplayer/Actions")]
    [Image(typeof(IconPlayer))] // Use GameCreator icon types

    public class MyInstruction : Instruction
    {
        protected override Task Run(Args args)
        {
            return DefaultResult;
        }
    }
}
```

## Quick Reference

| Element | Pattern | Example |
|---------|---------|---------|
| Instruction signature | `Task Run(Args args)` | Movement, actions, effects |
| Condition signature | `bool Check(Args args)` | State checks, comparisons |
| Get GameObject | `args.Self` | Current target |
| Get Character | `args.Self.Get<Character>()` | Character component |
| Sync operation | `return DefaultResult;` | Instant completion |
| Async operation | `await ...; return;` | Animation waits |
| Network check | `character.IsNetworkOwner` | Multiplayer guard |
| Icon | `EditorIcons.Character` | Use built-in icons |

## Related Documentation

- `.serena/memories/CRITICAL/001_gamecreator_invasive_integration.md` - Character patterns
- `.serena/memories/CRITICAL/003_multiplayer_rpc_patterns.md` - Network RPCs
- `.serena/memories/CONTEXT/roo_modes_reference.md` - Icon reference (.roomodes)
- `Assets/Plugins/GameCreator/Runtime/VisualScripting/` - Base classes
