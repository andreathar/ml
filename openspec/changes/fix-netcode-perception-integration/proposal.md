# Change: Fix Netcode Perception Integration and Add Perception Debug UI

## Why

Three critical issues are affecting multiplayer gameplay:

1. **Character Leg Animation Glitches**: The `NetworkUnitDriverController` modifies `Transform.localPosition` during height adjustments (line 100), which conflicts with `NetworkTransform` and causes erratic leg animations on network-spawned characters.

2. **NPC Noise Triggers Not Working for Network Players**: The `InstructionPerceptionEmitNoise` instruction only processes noise locally using `SpatialHashPerception.Find()`. When a Network Player emits noise (e.g., OnLand trigger), only local perceptions receive it - remote NPCs never hear the noise because there's no RPC-based noise propagation.

3. **No Perception Debug Visibility**: Developers have no way to see perception events in real-time. A debug UI showing messages like "NPC_Character1 just heard Player_Host noise" would greatly improve debugging and gameplay verification.

## What Changes

### Bug Fixes

- **BREAKING**: Refactor `NetworkUnitDriverController.UpdatePhysicPropertiesNetworked()` to avoid direct position manipulation that conflicts with NetworkTransform
- **Add**: `InstructionNetworkEmitNoise` - A networked version of noise emission that uses ServerRpc to broadcast noise to all clients
- **Add**: `NetworkNoiseEmitter` component to handle RPC-based noise propagation
- **Add**: Server-side noise validation and broadcasting in `NetworkPerception`

### New Features

- **Add**: `PerceptionDebugUI` - Real-time UI panel showing perception events:
  - Noise heard events: "NPC_Guard heard Player_Host [footstep] at 5.2m"
  - Awareness changes: "NPC_Guard awareness of Player_Host: 0.35 (Suspicious)"
  - Evidence noticed: "NPC_Guard noticed Evidence_BloodStain"
- **Add**: `NetworkPerceptionEvents` extensions for noise heard events
- **Add**: Configurable UI with filtering by perception, event type, and distance

## Impact

- **Affected specs**: `network-synchronization`, `character-system`
- **Affected code**:
  - `NetworkUnitDriverController.cs` - Height adjustment fix
  - `NetworkPerception.cs` - Add noise RPC support
  - `NetworkPerceptionEvents.cs` - Add noise heard event
  - New `InstructionNetworkEmitNoise.cs`
  - New `NetworkNoiseEmitter.cs`
  - New `PerceptionDebugUI.cs`
  - New `PerceptionDebugUIItem.cs`

## Root Cause Analysis

### Issue 1: Leg Animation Glitches

**Location**: `NetworkUnitDriverController.cs:100`

```csharp
this.Transform.localPosition += Vector3.down * offset;
```

**Problem**: This directly modifies `Transform.localPosition` which fights with `NetworkTransform`. Even though the controller skips `controller.center` modifications (correctly), the position adjustment still causes jitter.

**Solution**: Use `CharacterController.Move()` or queue position adjustments to be applied after NetworkTransform sync, or disable this adjustment entirely for network characters since the mannequin position is handled by `ApplyMannequinPosition()`.

### Issue 2: Network Noise Not Propagating

**Location**: `InstructionPerceptionEmitNoise.cs:63-80`

```csharp
SpatialHashPerception.Find(position, radius, this.m_ListPerceptions);
foreach (ISpatialHash spatialHash in this.m_ListPerceptions)
{
    Perception perception = spatialHash as Perception;
    sensorHear.OnReceiveNoise(stimulus);
}
```

**Problem**: `SpatialHashPerception.Find()` only queries local perceptions. When Client A's player lands and emits noise, only Client A's local perceptions receive it. The server and other clients never know about the noise.

**Solution**: Create `InstructionNetworkEmitNoise` that:
1. Client calls ServerRpc with noise parameters
2. Server validates and broadcasts ClientRpc to all clients
3. Each client processes noise through their local perceptions
4. Fire `NetworkPerceptionEvents.EventNoiseHeard` for UI updates

### Issue 3: No Debug Visibility

**Problem**: No UI exists to show perception events, making debugging impossible.

**Solution**: Create `PerceptionDebugUI` that subscribes to `NetworkPerceptionEvents` and displays formatted messages with timestamps, filtering, and auto-scroll.

## Screenshots Reference

- **Screenshot 1**: Shows OnLand trigger configured with "Network Local Player" position, radius 10, tag "player", intensity 5 - correctly configured but noise doesn't reach NPCs on other clients
- **Screenshot 2**: Shows `IndicatorAwarenessItemUI.cs` with unassigned SerializeField warnings - these are intentionally optional fields, not a bug
