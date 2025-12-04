# Tasks: Fix Netcode Perception Integration

## 1. Fix Character Animation Glitches

- [x] 1.1 Modify `NetworkUnitDriverController.UpdatePhysicPropertiesNetworked()` to avoid direct position manipulation
- [ ] 1.2 Test height adjustment without Transform.localPosition modification
- [ ] 1.3 Verify leg animations are stable on network-spawned characters
- [ ] 1.4 Test with multiple networked characters simultaneously

## 2. Implement Network Noise Emission

- [x] 2.1 Create `NetworkNoiseEmitter` component with ServerRpc/ClientRpc for noise propagation
- [x] 2.2 Add `EmitNoiseServerRpc(Vector3 position, float radius, string tag, float intensity)` to broadcast noise
- [x] 2.3 Add `ProcessNoiseClientRpc()` to process noise on all clients through local SpatialHashPerception
- [x] 2.4 Create `InstructionNetworkEmitNoise` instruction for Visual Scripting
- [x] 2.5 Add noise validation on server (prevent spam, validate parameters)
- [x] 2.6 Update `NetworkPerceptionEvents` with `EventNoiseEmitted` and `EventNoiseHeard`

## 3. Create Perception Debug UI

- [x] 3.1 Create `PerceptionDebugUI` MonoBehaviour with Canvas setup
- [x] 3.2 Create `PerceptionDebugUIItem` for individual log entries
- [x] 3.3 Subscribe to `NetworkPerceptionEvents` (NoiseHeard, AwarenessChanged, EvidenceNoticed)
- [x] 3.4 Format messages: "[Time] NPC_Name heard Player_Name [tag] at Xm"
- [x] 3.5 Add filtering options (by perception, event type, max distance)
- [x] 3.6 Add auto-scroll and max entry limit (configurable)
- [x] 3.7 Add toggle to show/hide UI in-game (F8 key)
- [ ] 3.8 Create prefab for easy scene integration

## 4. Integration and Testing

- [ ] 4.1 Update OnLand trigger to use `InstructionNetworkEmitNoise` instead of `InstructionPerceptionEmitNoise`
- [ ] 4.2 Test NPC response to Network Player landing
- [ ] 4.3 Verify noise propagates from Host to Clients and vice versa
- [ ] 4.4 Test UI displays correct messages for all clients
- [ ] 4.5 Verify awareness changes trigger NPC follow behavior
- [ ] 4.6 Performance test with multiple NPCs and frequent noise emissions

## 5. Documentation

- [ ] 5.1 Update Foam documentation for new components
- [ ] 5.2 Add usage examples for `InstructionNetworkEmitNoise`
- [ ] 5.3 Document `PerceptionDebugUI` setup and configuration

## Files Created/Modified

### Modified Files
- `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/Runtime/Driver/NetworkUnitDriverController.cs`
  - Removed direct `Transform.localPosition` manipulation that conflicted with NetworkTransform

### New Files
- `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/Runtime/Components/Perception/NetworkNoiseEmitter.cs`
  - Singleton component for network-synchronized noise emission
  - ServerRpc/ClientRpc for server-authoritative noise validation and broadcast
  - Rate limiting to prevent spam

- `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/Runtime/VisualScripting/Instructions/Perception/InstructionNetworkEmitNoise.cs`
  - Visual scripting instruction for emitting network-synchronized noise
  - Fallback to local emission when NetworkNoiseEmitter not present

- `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/Runtime/UI/Components/PerceptionDebugUI.cs`
  - Real-time debug UI for perception events
  - Filtering, auto-scroll, F8 toggle visibility
  - Color-coded event types

- `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/Runtime/UI/Components/PerceptionDebugUIItem.cs`
  - Individual entry component for debug UI
  - Lifetime/expiration support with fade out
