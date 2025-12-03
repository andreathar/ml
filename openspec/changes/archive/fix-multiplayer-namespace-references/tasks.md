# Implementation Tasks

## 1. Fix PowerGrid Conditions (7 files)
- [ ] 1.1 Add `using GameCreator.Runtime.VisualScripting;` to `ConditionPowerConsumerIsConnected.cs`
- [ ] 1.2 Add `using GameCreator.Runtime.VisualScripting;` to `ConditionPowerConsumerIsPowered.cs`
- [ ] 1.3 Add `using GameCreator.Runtime.VisualScripting;` to `ConditionPowerDeviceHasPower.cs`
- [ ] 1.4 Add `using GameCreator.Runtime.VisualScripting;` to `ConditionPowerDeviceIsActive.cs`
- [ ] 1.5 Add `using GameCreator.Runtime.VisualScripting;` to `ConditionPowerDeviceIsOperational.cs`
- [ ] 1.6 Add `using GameCreator.Runtime.VisualScripting;` to `ConditionPowerSourceIsActive.cs`
- [ ] 1.7 Add `using GameCreator.Runtime.VisualScripting;` to `ConditionPowerSourceIsOverloaded.cs`

## 2. Fix PowerGrid Instructions (6 files)
- [ ] 2.1 Add `using GameCreator.Runtime.VisualScripting;` to `InstructionPowerConsumerConnect.cs`
- [ ] 2.2 Add `using GameCreator.Runtime.VisualScripting;` to `InstructionPowerConsumerDisconnect.cs`
- [ ] 2.3 Add `using GameCreator.Runtime.VisualScripting;` to `InstructionPowerDeviceToggle.cs`
- [ ] 2.4 Add `using GameCreator.Runtime.VisualScripting;` to `InstructionPowerDeviceTurnOff.cs`
- [ ] 2.5 Add `using GameCreator.Runtime.VisualScripting;` to `InstructionPowerDeviceTurnOn.cs`
- [ ] 2.6 Add `using GameCreator.Runtime.VisualScripting;` to `InstructionPowerSourceSetActive.cs`

## 3. Fix Missing Icons in PowerGrid
- [ ] 3.1 Replace `IconPlug` with `IconJoint` in all affected files
- [ ] 3.2 Replace `IconLightbulb` with `IconBoltSolid` in all affected files
- [ ] 3.3 Replace `IconHouse` with `IconCubeOutline` in affected files

## 4. Fix NetworkTraitsAdapter IEquatable
- [ ] 4.1 Add `IEquatable<StatState>` to `StatState` struct
- [ ] 4.2 Implement `Equals(StatState other)` method in `StatState`
- [ ] 4.3 Add `IEquatable<StatusEffectState>` to `StatusEffectState` struct
- [ ] 4.4 Implement `Equals(StatusEffectState other)` method in `StatusEffectState`

## 5. Fix InstructionRPCToNearby Override
- [ ] 5.1 Remove `override` modifier from `Run(Args args)` method (method should not exist as Run is sealed)
- [ ] 5.2 Move logic to override `RunLocally(Args args)` instead (abstract method in base)
- [ ] 5.3 Replace `IconRadius` with `IconCircleOutline`

## 6. Verification
- [ ] 6.1 Verify Unity compilation succeeds with zero errors
- [ ] 6.2 Test PowerGrid instructions execute in single-player mode
- [ ] 6.3 Test PowerGrid conditions evaluate correctly
