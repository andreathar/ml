# MLcreator Coding Standards

## Naming Conventions
- **RPC methods**: End in `ClientRpc` or `ServerRpc`
- **NetworkVariables**: `m_[Name]` (private) + `[Name]` property (public)
- **Namespaces**: `GameCreator.Multiplayer.Runtime.*`

## Visual Scripting Signatures (EXACT)
```csharp
// Instructions
protected override Task Run(Args args)

// Conditions  
protected override bool Check(Args args)
```
❌ NO CancellationToken, NO extra parameters!

## Testing
- Use `UnityTest` fixtures with `NetworkManager` setup
- Reuse `NetworkTestHelpers` for spawn/despawn/timeouts
- Name: `NetworkVariable_WhenChanged_InvokesCallback`
- Coverage: 80% NetworkBehaviour, 90% RPC, 90% NetworkVariable

## Commits
- Conventional: `feat:`, `fix:`, `chore:`
- Link OpenSpec ID for non-trivial changes
