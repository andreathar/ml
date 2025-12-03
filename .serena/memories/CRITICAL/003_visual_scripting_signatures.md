# Visual Scripting Signatures

**Priority:** P0 CRITICAL
**Last Updated:** 2025-12-03
**Assembly:** GameCreator.Runtime.Core, GameCreator.Netcode.Runtime

## Instruction Signature (EXACT)

```csharp
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

[Title("My Instruction")]
[Category("Custom/Category")]
[Description("Description of what this does")]
public class InstructionMyAction : Instruction
{
    // Exposed fields for inspector
    [SerializeField] private PropertyGetGameObject m_Target;
    
    protected override Task Run(Args args)
    {
        // Your implementation
        GameObject target = m_Target.Get(args);
        
        // For synchronous completion:
        return DefaultResult;
        
        // For async operations:
        // return SomeAsyncOperation();
    }
}
```

### ⚠️ CRITICAL: NO CancellationToken

```csharp
// ❌ WRONG - Never add CancellationToken
protected override Task Run(Args args, CancellationToken ct)

// ✅ CORRECT - Exact signature
protected override Task Run(Args args)
```

## Condition Signature (EXACT)

```csharp
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

[Title("My Condition")]
[Category("Custom/Category")]
[Description("Description of the check")]
public class ConditionMyCheck : Condition
{
    [SerializeField] private PropertyGetGameObject m_Target;
    
    protected override bool Check(Args args)
    {
        GameObject target = m_Target.Get(args);
        return target != null && SomeCondition(target);
    }
}
```

## Network Visual Scripting

### Network Instructions (from GameCreator.Netcode.Runtime)

```csharp
// These are already implemented - reference for patterns

// InstructionNetworkStartHost
protected override Task Run(Args args)
{
    NetworkManager.Singleton.StartHost();
    return DefaultResult;
}

// InstructionNetworkSpawnPlayer
protected override Task Run(Args args)
{
    // Spawn logic
    return DefaultResult;
}
```

### Network Conditions (from GameCreator.Netcode.Runtime)

```csharp
// ConditionNetworkIsOwner
protected override bool Check(Args args)
{
    NetworkObject netObj = args.Target?.GetComponent<NetworkObject>();
    return netObj != null && netObj.IsOwner;
}

// ConditionNetworkIsLocalPlayer
protected override bool Check(Args args)
{
    var adapter = args.Target?.GetComponent<NetworkCharacterAdapter>();
    return adapter != null && adapter.IsLocalPlayer;
}
```

## Args Usage

```csharp
protected override Task Run(Args args)
{
    // Get the object running this instruction
    GameObject self = args.Self;
    
    // Get the target (if specified)
    GameObject target = args.Target;
    
    // Get component from target
    Character character = args.Target?.GetComponent<Character>();
    
    // Get component using helper
    NetworkCharacterAdapter adapter = args.ComponentFromTarget<NetworkCharacterAdapter>();
    
    return DefaultResult;
}
```

## Property Getters

```csharp
// For inspector-configurable values
[SerializeField] private PropertyGetGameObject m_Target;
[SerializeField] private PropertyGetDecimal m_Value;
[SerializeField] private PropertyGetBool m_Condition;
[SerializeField] private PropertyGetString m_Name;

protected override Task Run(Args args)
{
    GameObject target = m_Target.Get(args);
    float value = (float)m_Value.Get(args);
    bool condition = m_Condition.Get(args);
    string name = m_Name.Get(args);
    
    return DefaultResult;
}
```

## Custom Network Instructions

```csharp
[Title("Network Custom Action")]
[Category("Network/Custom")]
public class InstructionNetworkCustomAction : Instruction
{
    [SerializeField] private PropertyGetGameObject m_Target;
    
    protected override Task Run(Args args)
    {
        var adapter = m_Target.Get(args)?.GetComponent<NetworkCharacterAdapter>();
        
        if (adapter == null) return DefaultResult;
        
        // Only owner can execute
        if (!adapter.IsOwner) return DefaultResult;
        
        // Do something network-aware
        // ...
        
        return DefaultResult;
    }
}
```

## Related Files

- `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/Runtime/VisualScripting/`
- `.serena/personas/character-core.llm.txt`
- `.serena/personas/netcode-core.llm.txt`
