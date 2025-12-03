# Change: Research GameCreator Player Component Targeting in Multiplayer

## Why

The screenshot evidence shows critical multiplayer issues:
1. **Player positioning is incorrect** - Character appears floating/sinking, not properly grounded at spawn
2. **Component targeting is broken** - Multiple "+35 XP" popups appearing, indicating Triggers/Hotspots are targeting wrong players or firing multiple times
3. **Scene components lack proper network player resolution** - GameCreator components (Hotspots, Triggers, Camera Shots, etc.) don't correctly resolve to the local network player

These issues stem from a fundamental gap: **no comprehensive audit exists of which GameCreator components interact with players and how they should behave in multiplayer**.

## What Changes

### Research Phase (This Proposal)

1. **Deep audit of ALL GameCreator components that reference players**
   - Identify every component type that uses `PropertyGetGameObject` with player references
   - Document which use tag-based lookup ("Player" tag) vs direct reference
   - Map which need local-only targeting vs network-aware targeting

2. **Character positioning research**
   - Investigate spawn position flow: NetworkPlayerManager → NetworkCharacterAdapter → Character
   - Document CharacterController.center initialization timing
   - Identify where position is getting corrupted

3. **Component interaction patterns**
   - Trigger activation logic in multiplayer (who fires, when, for whom)
   - Hotspot interaction authority (local execution vs server validation)
   - Camera shot targeting for local player in multiplayer

4. **Create targeting strategy matrix**
   - Which components should use `GetGameObjectLocalNetworkPlayer`
   - Which components need server authority
   - Which components are inherently local-only

### Implementation Phase (Future Proposal)

Based on research findings:
- Fix spawn positioning pipeline
- Create additional PropertyGetGameObject types as needed
- Retrofit existing scene components with correct targeting
- Add Editor validation to prevent misconfiguration

## Impact

- **Affected specs:** `player-component-targeting` (new capability spec)
- **Affected code (research targets):**
  - `NetworkCharacterAdapter.cs` - spawn positioning logic
  - `NetworkPlayerManager.cs` - player spawn orchestration
  - `LocalPlayerResolver.cs` - player reference management
  - All `PropertyGet*Player*.cs` files
  - All Visual Scripting components with player targets
  - Scene Trigger/Hotspot configurations

## Evidence

### Screenshot Analysis (2025-11-27)
- **Issue 1:** Player model feet not touching ground (positioning error)
- **Issue 2:** 8+ "+35 XP" popups visible (component firing multiple times)
- **Issue 3:** Debug panel shows "Sync (Owner)" suggesting ownership is correct but targeting isn't

### Known Related Issues
- `fix-hotspot-interaction-conflicts` proposal addresses part of this
- CharacterController.center was previously fixed but may still have race conditions
- `GetGameObjectLocalNetworkPlayer` exists but isn't widely deployed

## Success Criteria

1. Complete inventory of 100+ GameCreator components that reference players
2. Decision matrix for each component's multiplayer targeting strategy
3. Root cause identified for spawn positioning issue
4. Root cause identified for duplicate trigger firing
5. Technical design for systematic fix

## Research Questions

1. **Positioning:** Why does the character spawn at wrong height despite CharacterController.center fix?
2. **Triggers:** Are XP triggers firing for each network player or once incorrectly duplicated?
3. **Tag timing:** Does the "Player" tag get set before or after component initialization?
4. **PropertyGet:** Which GameCreator PropertyGetGameObject implementations need network-aware variants?
5. **Camera:** How should Camera Shots target local player in multiplayer?
6. **Hotspots:** Should hotspot interactions be validated server-side or client-authoritative?
