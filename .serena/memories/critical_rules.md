# Critical Project Rules (P0)

## File Organization - ZERO ROOT POLLUTION
- AI reports → `claudedocs/reports/`
- AI action items → `claudedocs/action-items/`
- AI guides → `claudedocs/guides/`
- User docs → `docs/`
- Temp files → `.temp/`

## Network Architecture
- **NEVER** use NetworkTransform on player prefabs (conflicts with CharacterController)
- Use NetworkCharacterAdapter for state-based sync
- Before spawn: `character.IsNetworkSpawned = true`
- Ownership: `if (character.IsNetworkOwner) { }`

## GameCreator Patterns
- Instructions: `protected override Task Run(Args args)` - NO CancellationToken!
- Conditions: `protected override bool Check(Args args)`
- RPC methods: Must end in `ServerRpc` or `ClientRpc`

## Deprecated Systems
- ⛔ serena-network (scripts/serena-network/) - Use `.serena/memories/` instead
