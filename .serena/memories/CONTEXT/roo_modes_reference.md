# Roo Modes Icon Reference (.roomodes)

**Category:** CONTEXT - Visual Scripting Icons
**Last Updated:** 2025-11-23
**Source:** `.roomodes` file in project root

## Purpose

The `.roomodes` file defines icon mappings for GameCreator visual scripting elements. When creating custom Instructions or Conditions, use these existing icons rather than creating new ones.

## Common GameCreator Icons

### Character Icons

```
IconPlayer        - Player character operations
IconCharacter     - General character operations
IconMotion        - Movement and locomotion
IconJump          - Jumping actions
IconGesture       - Animations and gestures
IconRagdoll       - Ragdoll physics
```

### Combat & Actions

```
IconWeapon        - Weapon-related operations
IconShooter       - Shooting mechanics
IconMelee         - Melee combat
IconTarget        - Targeting systems
IconDamage        - Damage dealing/receiving
IconHealth        - Health management
```

### Inventory & Items

```
IconItem          - General items
IconBag           - Inventory operations
IconCoin          - Currency
IconEquipment     - Equipment management
IconCraft         - Crafting systems
```

### UI & Interaction

```
IconButton        - Button interactions
IconDialogue      - Dialogue systems
IconQuest         - Quest operations
IconNotification  - Notifications
IconUI            - General UI
```

### Multiplayer Icons

```
IconNetwork       - Network operations
IconServer        - Server-side operations
IconClient        - Client-side operations
IconSync          - Synchronization
IconRPC           - RPC operations
```

### Utility Icons

```
IconVariable      - Variables
IconCondition     - Conditional logic
IconTrigger       - Trigger volumes
IconLocation      - Position/location
IconTimer         - Timing operations
IconMath          - Mathematical operations
```

## Usage in Visual Scripting

### Instruction Icon Assignment

```csharp
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

[Title("Network Spawn Character")]
[Category("Multiplayer/Character")]
[Image(typeof(IconPlayer))]  // ← Use existing icon
public class InstructionNetworkSpawn : Instruction
{
    protected override Task Run(Args args)
    {
        // Implementation
        return DefaultResult;
    }
}
```

### Condition Icon Assignment

```csharp
[Title("Is Network Owner")]
[Category("Multiplayer/Conditions")]
[Image(typeof(IconNetwork))]  // ← Use existing icon
public class ConditionIsNetworkOwner : Condition
{
    protected override bool Check(Args args)
    {
        Character character = args.Self.Get<Character>();
        return character != null && character.IsNetworkOwner;
    }
}
```

## Icon Selection Guidelines

### Match Semantic Meaning

Choose icons that represent the **operation**, not implementation details:

```
✅ Movement instruction → IconMotion
✅ Network sync → IconSync
✅ Inventory add → IconBag

❌ Movement instruction → IconCharacter (too generic)
❌ Network sync → IconVariable (wrong semantic)
❌ Inventory add → IconItem (represents item, not operation)
```

### Category-Based Selection

**For Multiplayer category:**
- Network operations → `IconNetwork`
- Server RPCs → `IconServer`
- Client RPCs → `IconClient`
- Sync operations → `IconSync`

**For Character category:**
- Movement → `IconMotion`
- Actions → `IconGesture`
- Properties → `IconCharacter`

**For Combat category:**
- Damage → `IconDamage`
- Weapons → `IconWeapon`
- Health → `IconHealth`

## Custom Icons (Avoid When Possible)

**Only create custom icons if:**
- No existing icon matches semantic meaning
- Feature is completely unique to your module
- Icon will be reused across multiple Instructions

**Don't create custom icons for:**
- One-off Instructions
- Slight variations of existing operations
- Internal/debugging tools

## .roomodes File Format

The `.roomodes` file is a binary/proprietary format managed by GameCreator. **Do not modify manually.**

## Related Documentation

- `.serena/memories/CRITICAL/004_visual_scripting_task_signatures.md` - Task signatures
- `Assets/Plugins/GameCreator/Runtime/Common/Icons/` - Icon assets
- GameCreator documentation - Complete icon reference

## Quick Reference

| Operation Type | Recommended Icon |
|---------------|-----------------|
| Character movement | `IconMotion` |
| Character action | `IconGesture` |
| Network sync | `IconSync` |
| Server RPC | `IconServer` |
| Client RPC | `IconClient` |
| Inventory operation | `IconBag` |
| Item operation | `IconItem` |
| Combat action | `IconDamage` |
| UI interaction | `IconButton` |
| Condition check | `IconCondition` |
| Variable operation | `IconVariable` |
| Trigger event | `IconTrigger` |
