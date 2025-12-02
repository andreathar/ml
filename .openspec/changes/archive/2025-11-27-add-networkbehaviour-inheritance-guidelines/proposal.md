# Change: Add NetworkBehaviour Inheritance Guidelines

## Why

Developers commonly create Unity serialization conflicts when inheriting from `NetworkBehaviour` by adding redundant fields that the base class already provides. The most common issue is adding a separate `NetworkObject` reference field when `NetworkBehaviour` already exposes `NetworkObject` as a property.

This causes Unity's serialization system to fail with cryptic errors about duplicate field names, breaking compilation and confusing developers about the root cause.

**Problem Pattern:**
```csharp
public class MyController : NetworkBehaviour
{
    [SerializeField] private NetworkObject m_NetworkObject; // BUG: Conflicts with base class!
}
```

## What Changes

- **ADDED**: New specification for NetworkBehaviour inheritance patterns
- **ADDED**: Documentation of common anti-patterns and their solutions
- **ADDED**: Code analyzer integration for catching violations at edit-time
- **ADDED**: Memory/documentation entry in Serena CRITICAL tier

### Key Requirements

1. **Never Duplicate Base Class Fields**: Classes inheriting from `NetworkBehaviour` SHALL NOT declare fields with names that shadow base class properties
2. **Use Base Class Properties**: Always use `NetworkObject`, `IsOwner`, `IsServer`, `IsClient`, `OwnerClientId` from `NetworkBehaviour` directly
3. **Static Analysis**: Add Roslyn analyzer or EditorTool to detect inheritance conflicts

### Affected Patterns

| Anti-Pattern | Correct Pattern |
|--------------|-----------------|
| `private NetworkObject m_NetworkObject;` | Use `this.NetworkObject` |
| `private NetworkManager m_NetworkManager;` | Use `NetworkManager.Singleton` |
| `private ulong m_OwnerClientId;` | Use `this.OwnerClientId` |
| `private bool m_IsOwner;` | Use `this.IsOwner` |

## Impact

- **Affected specs**: `netcode-patterns` (new)
- **Affected code**: All NetworkBehaviour subclasses in `GameCreator_Multiplayer`
- **Affected memory**: New CRITICAL tier memory entry

### Benefits

1. **Prevents Cryptic Errors**: Eliminates confusing Unity serialization conflicts
2. **Improves Onboarding**: Clear documentation of the correct pattern
3. **Automated Detection**: Catches issues before they cause runtime problems
4. **AI Assistant Guidance**: Serena memory ensures AI assistants avoid this anti-pattern

### Risk Assessment

- **Breaking Changes**: None - this is additive documentation and tooling
- **Migration**: Existing code already follows the pattern (verified in NetworkPlayerController)
- **Rollback**: Documentation only - can be updated anytime
