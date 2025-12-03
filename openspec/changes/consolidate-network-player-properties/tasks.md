# Tasks: Consolidate Network Character Property Targeting

## 1. Core Position Property Types

- [x] 1.1 Create `GetPositionNetworkLocalPlayer` class
  - Inherit from `PropertyTypeGetPosition`
  - Use `NetworkCharacterRegistry.LocalPlayer` for character lookup
  - Add proper GC attributes (`[Title]`, `[Category]`, `[Image]`, `[Description]`)
  - Implement caching via registry to avoid per-frame FindObjectsByType calls

- [x] 1.2 Create `GetRotationNetworkLocalPlayer` class
  - Mirror position implementation for rotation
  - Return `Quaternion` from NetworkCharacter's transform
  - Support `RotationSpace` (Global/Local)

- [ ] 1.3 Create `GetScaleNetworkLocalPlayer` class
  - Mirror position implementation for scale
  - Return `Vector3` localScale from NetworkCharacter's transform

- [x] 1.4 Create `GetLocationNetworkLocalPlayer` class
  - Combine position and rotation into Location struct
  - Useful for spawn points and teleportation targets

## 2. Multi-Client Character Access

- [x] 2.1 `GetGameObjectNetworkPlayerByClientId` already exists
  - Updated to use `NetworkCharacterRegistry.GetByClientId()` for efficient lookup
  - Takes `PropertyGetInteger` for ClientId parameter (0 = Host, 1+ = Clients)

- [x] 2.2 Create `GetPositionNetworkCharacterByClientId` class
  - Uses `NetworkCharacterRegistry.GetPositionByClientId()` for direct position access
  - Common use case: "move towards player 2's position"

## 3. List Variable Integration

- [ ] 3.1 Create `GetListGameObjectNetworkCharacters` class
  - Returns all spawned NetworkCharacters as GameObject list
  - Filters by `IsNetworkSpawned` flag
  - Useful for "find closest enemy" patterns

- [x] 3.2 Create `InstructionCollectNetworkCharacters` class
  - Visual scripting action to populate a ListVariables
  - Option to include/exclude local player

## 4. Caching and Performance

- [x] 4.1 Implement `NetworkCharacterRegistry` static class
  - Maintains list of all active NetworkCharacters
  - Characters register on spawn, unregister on despawn
  - Eliminates per-query FindObjectsByType overhead
  - Supports Host/Client topology (ClientId 0 = Host, 1+ = Clients)
  - Caches LocalPlayer with dirty flag for invalidation

- [x] 4.2 Update `GetGameObjectNetworkLocalPlayer` to use registry
  - Replace FindObjectsByType with `NetworkCharacterRegistry.LocalPlayer`
  - Fallback to ShortcutPlayer for single-player/editor

- [x] 4.3 Update `GetGameObjectNetworkPlayerByClientId` to use registry
  - Replace FindObjectsByType with `NetworkCharacterRegistry.GetByClientId()`

- [x] 4.4 Create `NetworkCharacterSync` helper component
  - NetworkBehaviour that forwards OnNetworkSpawn/Despawn/OwnershipChanged
  - Required because Character doesn't inherit from NetworkBehaviour

## 5. Editor Integration

- [x] 5.1 Add EditorValue implementations for all new properties
  - Provide sensible preview values in edit mode
  - Use existing Character marked as Player for preview

- [ ] 5.2 Create custom property drawers if needed
  - Show network status indicator in Inspector
  - Warn if NetworkManager not in scene

## 6. Testing and Validation

- [ ] 6.1 Create test scene with multiple NetworkCharacters
  - Verify property resolution for local vs remote
  - Test in both host and client modes

- [ ] 6.2 Test ListVariables population
  - Ensure all spawned characters appear
  - Verify filtering options work

- [ ] 6.3 Performance testing
  - Measure property access times
  - Verify registry doesn't cause memory issues

## 7. Documentation

- [ ] 7.1 Update Foam documentation for new property types
- [ ] 7.2 Add usage examples to knowledge base
- [ ] 7.3 Document migration path from ShortcutPlayer usage
