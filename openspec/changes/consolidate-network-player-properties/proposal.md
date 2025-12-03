# Change: Consolidate Network Character Property Targeting

## Why

GameCreator's property system (`PropertyGetPosition`, `PropertyGetGameObject`, etc.) relies on `ShortcutPlayer` for "Player Position" targeting, which works in single-player but breaks in multiplayer scenarios where multiple clients each have their own "local player". Currently:

1. **`GetPositionCharactersPlayer`** uses `ShortcutPlayer.Transform` - works only for the single registered player
2. **`GetGameObjectNetworkLocalPlayer`** exists but isn't integrated with position/rotation/scale properties
3. **Triggers and ListVariables** that reference "Player Position" fail in multiplayer because:
   - Each client has a different local player
   - `ShortcutPlayer` only tracks one player per scene (the one that was set via `IsPlayer = true`)
   - Remote characters are visible but not targetable as "Player" from their perspective

This creates a fundamental disconnect: GameCreator components (Triggers, Conditions, Instructions) cannot seamlessly target "the network-local player's properties" without custom code.

## What Changes

### New Network-Aware Property Types

1. **`GetPositionNetworkLocalPlayer`** - Position of the local client's NetworkCharacter
2. **`GetRotationNetworkLocalPlayer`** - Rotation of the local client's NetworkCharacter
3. **`GetScaleNetworkLocalPlayer`** - Scale of the local client's NetworkCharacter
4. **`GetLocationNetworkLocalPlayer`** - Location (position+rotation) of the local client's NetworkCharacter

### Enhanced Property Discovery

5. **`GetGameObjectNetworkCharacterByOwner`** - Get any client's character by ClientId
6. **`GetPositionNetworkCharacterByOwner`** - Get position of any client's character

### ListVariables Integration

7. **`GetListNetworkCharacters`** - Returns all spawned NetworkCharacters as a list
8. **`GetListNetworkCharacterPositions`** - Returns positions of all NetworkCharacters

### Trigger Compatibility

9. **Network-aware distance checking** - Triggers that check "distance to Player" should use network-local player in multiplayer context

## Impact

- **Affected specs**: `network-synchronization`, `visual-scripting`, `character-system`
- **New spec**: `network-property-system` (this proposal creates it)
- **Affected code**:
  - `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/Runtime/VisualScripting/Properties/`
  - New property type classes for position/rotation/scale
  - Integration with existing GameCreator property serialization

### Breaking Changes

**None** - All changes are additive. Existing single-player projects continue to work with `ShortcutPlayer`. Multiplayer projects gain new property types selectable in the Inspector.

## Technical Approach

The implementation follows GameCreator's established patterns:

```
PropertyTypeGetPosition (base)
├── GetPositionCharactersPlayer (existing - uses ShortcutPlayer)
├── GetPositionCharacter (existing - uses specified Character)
└── GetPositionNetworkLocalPlayer (NEW - uses NetworkCharacter lookup)
```

Each new property type:
1. Inherits from the appropriate `PropertyTypeGet*` base class
2. Uses `[Title]`, `[Category]`, `[Image]`, `[Description]` attributes for Inspector display
3. Implements `Get(Args args)` and `Get(GameObject gameObject)` methods
4. Provides a static `Create` factory method
5. Falls back gracefully when not in multiplayer context

## Success Criteria

1. A designer can select "Network Local Player Position" from any `PropertyGetPosition` field
2. Triggers/Conditions using player position work correctly for each client's local player
3. ListVariables can be populated with all NetworkCharacter GameObjects
4. No performance regression (character lookup is cached)
5. Works in both Edit mode (preview) and Play mode (runtime)
