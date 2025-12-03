# Change: Add Netcode for GameObjects Editable Integration Package

## Why

The current GameCreator Core package is installed via Unity Package Manager and cannot be modified directly. However, multiplayer integration with Unity Netcode for GameObjects requires **invasive modifications** to core GameCreator files (Character.cs, UnitDriverController.cs) to:

1. Add `IsNetworkSpawned` flag to prevent CharacterController conflicts during network spawn
2. Modify driver logic to skip CharacterController.center resets for networked characters
3. Enable NetworkTransform to control character positioning without conflicts

Creating an editable integration package at `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration` provides a structured location for these modifications while maintaining separation from the original read-only GameCreator packages.

## What Changes

### New Capability: netcode-gamecreator-integration

Creates a dedicated integration package containing:

1. **Editable Core Overrides**
   - Modified `Character.cs` with `IsNetworkSpawned` property
   - Modified `UnitDriverController.cs` respecting network flag
   - Override scripts that extend/replace GameCreator components

2. **Network Components**
   - `NetworkCharacterAdapter.cs` - Bridge between Character and NetworkBehaviour
   - `NetworkVariablesSync.cs` - GameCreator variable synchronization
   - `NetworkCharacterMotion.cs` - Motion state replication

3. **Visual Scripting Extensions**
   - Network-aware Instructions (Actions)
   - Network Conditions (IsServer, IsOwner, IsSpawned)
   - Network Triggers (OnPlayerSpawned, OnOwnershipChanged)

4. **Prefab Templates**
   - Network_Manager.prefab - NetworkManager setup
   - Network_Player_Manager.prefab - Player spawn handling
   - Network_Spawn_Manager.prefab - Spawn point management
   - Player_Network.prefab - Network-ready player template

5. **Assembly Definition**
   - `GameCreator.Netcode.asmdef` with proper references to:
     - Unity.Netcode.Runtime
     - Unity.Netcode.Components
     - GameCreator.Runtime.Characters
     - GameCreator.Runtime.Common
     - GameCreator.Runtime.VisualScripting

## Impact

- **Affected specs**:
  - `character-system` (implements IsNetworkSpawned requirement)
  - `network-synchronization` (GameCreator variable sync)
  - `spawn-system` (network spawn integration)
  - `visual-scripting` (network instructions/conditions)

- **Affected code**:
  - `Assets/Plugins/GameCreator/Packages/Core/Runtime/Characters/` (read-only reference)
  - `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/` (new editable package)

- **Breaking changes**: None - existing non-networked GameCreator projects work unchanged. This is an opt-in integration package.

## Rationale

**Why a separate package instead of modifying Core directly?**

1. **Upgrade Safety**: GameCreator Core updates won't overwrite custom network integration
2. **Clear Separation**: Network-specific code isolated from single-player logic
3. **Selective Inclusion**: Projects can include/exclude multiplayer support
4. **Version Control**: Track network integration changes independently
5. **Assembly Boundaries**: Proper dependency management via asmdef files

**Why editable files are necessary?**

The CharacterController conflict cannot be solved via composition alone. The `UnitDriverController.UpdatePhysicProperties()` method (line 264-267) unconditionally resets `controller.center = Vector3.zero` every frame, which conflicts with NetworkTransform positioning. This requires source modification to add a conditional check.
