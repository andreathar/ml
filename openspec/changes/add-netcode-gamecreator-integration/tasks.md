# Tasks: Add Netcode for GameObjects Editable Integration Package

## 1. Package Structure Setup

- [x] 1.1 Create directory structure at `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/`
- [x] 1.2 Create `Runtime/` folder for runtime scripts
- [x] 1.3 Create `Editor/` folder for editor scripts
- [x] 1.4 Create `Prefabs/` folder for network prefab templates (existing prefabs in root)
- [x] 1.5 Create `Resources/` folder for runtime-loaded assets

## 2. Assembly Definition Setup

- [x] 2.1 Create `GameCreator.Netcode.Runtime.asmdef` with references:
  - Unity.Netcode.Runtime
  - Unity.Netcode.Components
  - GameCreator.Runtime.Characters
  - GameCreator.Runtime.Common
  - GameCreator.Runtime.VisualScripting
- [x] 2.2 Create `GameCreator.Netcode.Editor.asmdef` for editor tools
- [x] 2.3 Verify assembly compilation with no errors

## 3. Core Override Scripts

- [x] 3.1 Create `NetworkCharacter.cs` extending Character with:
  - `IsNetworkSpawned` property
  - Network-aware lifecycle hooks
  - Override for Awake/Start to handle network timing
- [x] 3.2 Create `NetworkUnitDriverController.cs` extending UnitDriverController:
  - Override `UpdatePhysicProperties()` to skip center reset when networked
  - Add conditional logic for CharacterController management
- [ ] 3.3 Create `CharacterNetworkExtensions.cs` for extension methods (deferred - not critical)

## 4. Network Components

- [x] 4.1 Create `NetworkCharacterAdapter.cs`:
  - NetworkBehaviour component for Character sync
  - Manages IsNetworkSpawned flag lifecycle
  - ServerRpc/ClientRpc for ownership transfer
- [x] 4.2 Create `NetworkVariablesSync.cs`:
  - Sync GameCreator Local Variables via NetworkVariables
  - Configurable sync mode (Everyone/OwnerOnly/ServerOnly)
- [x] 4.3 Create `NetworkCharacterMotion.cs`:
  - Replicate Motion.MoveToDirection/MoveToPosition
  - NetworkVariables for motion state
  - Jump/Dash via RPCs
- [ ] 4.4 Create `NetworkCharacterAnimation.cs` (deferred - use Unity's NetworkAnimator)

## 5. Visual Scripting Instructions

- [x] 5.1 Create `InstructionNetworkSpawnPlayer.cs`
- [x] 5.2 Create `InstructionNetworkDespawn.cs`
- [x] 5.3 Create `InstructionNetworkStartHost.cs`
- [x] 5.4 Create `InstructionNetworkStartClient.cs`
- [x] 5.5 Create `InstructionNetworkDisconnect.cs`
- [x] 5.6 Create `InstructionNetworkChangeOwnership.cs`

## 6. Visual Scripting Conditions

- [x] 6.1 Create `ConditionNetworkIsServer.cs`
- [x] 6.2 Create `ConditionNetworkIsClient.cs`
- [x] 6.3 Create `ConditionNetworkIsHost.cs`
- [x] 6.4 Create `ConditionNetworkIsOwner.cs`
- [x] 6.5 Create `ConditionNetworkIsSpawned.cs`
- [x] 6.6 Create `ConditionNetworkIsConnected.cs`

## 7. Visual Scripting Triggers

- [x] 7.1 Create `EventNetworkOnClientConnected.cs`
- [x] 7.2 Create `EventNetworkOnClientDisconnected.cs`
- [x] 7.3 Create `EventNetworkOnNetworkSpawn.cs`
- [x] 7.4 Create `EventNetworkOnNetworkDespawn.cs`
- [x] 7.5 Create `EventNetworkOnOwnershipChanged.cs`
- [x] 7.6 Create `EventNetworkOnLocalPlayerSpawned.cs`

## 8. Prefab Templates

- [x] 8.1 Existing `Network_Manager.prefab` (template placeholder)
- [x] 8.2 Existing `Network_Player_Manager.prefab` (template placeholder)
- [x] 8.3 Renamed `Network_Spawn_Manager.prefab` (fixed typo: Networl -> Network)
- [ ] 8.4 Update `Player_Network.prefab` template with full component setup (manual Unity work)

## 9. Documentation

- [x] 9.1 Create README.md for the integration package
- [x] 9.2 Document IsNetworkSpawned usage pattern
- [x] 9.3 Document prefab setup workflow
- [x] 9.4 Add inline XML documentation to all public APIs

## 10. Testing

- [ ] 10.1 Create test scene with networked character
- [ ] 10.2 Verify single-player backward compatibility
- [ ] 10.3 Test host/client movement synchronization
- [ ] 10.4 Test ownership transfer scenario
- [ ] 10.5 Test disconnect/reconnect handling

## 11. Validation

- [x] 11.1 Run Unity compilation - no errors
- [x] 11.2 Verify assembly references resolve correctly
- [ ] 11.3 Test in Play Mode with network simulation
- [ ] 11.4 Update existing specs to reflect implementation

---

**Implementation Status**: Core implementation complete. Testing and prefab configuration pending.

**Files Created**:
- `Runtime/GameCreator.Netcode.Runtime.asmdef`
- `Editor/GameCreator.Netcode.Editor.asmdef`
- `Runtime/Components/NetworkCharacter.cs`
- `Runtime/Components/NetworkCharacterAdapter.cs`
- `Runtime/Driver/NetworkUnitDriverController.cs`
- `Runtime/Motion/NetworkCharacterMotion.cs`
- `Runtime/Sync/NetworkVariablesSync.cs`
- `Runtime/VisualScripting/Instructions/*.cs` (6 files)
- `Runtime/VisualScripting/Conditions/*.cs` (6 files)
- `Runtime/VisualScripting/Triggers/*.cs` (6 files)
- `README.md`
