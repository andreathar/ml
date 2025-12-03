# Change: Fix Multiplayer Module Namespace References and Compilation Errors

## Why
The GameCreator_Multiplayer module has 23 compilation errors preventing Unity from compiling the project. These are caused by:
1. Missing `using` directives for GameCreator base classes (`Condition`, `Instruction`)
2. References to non-existent icon classes (`IconPlug`, `IconLightbulb`, `IconHouse`, `IconRadius`)
3. Network structs missing `IEquatable<T>` implementation required by `NetworkList<T>`
4. Incorrect method override in `InstructionRPCToNearby` (overriding sealed method)

## What Changes

### PowerGrid Visual Scripting (16 files)
- Add `using GameCreator.Runtime.VisualScripting;` to all Condition and Instruction files
- Replace missing icons with existing GameCreator icons:
  - `IconPlug` → `IconJoint` (represents connection)
  - `IconLightbulb` → `IconBoltSolid` (represents power)
  - `IconHouse` → `IconCubeOutline` (represents building)

### NetworkTraitsAdapter (1 file)
- Add `IEquatable<T>` implementation to `StatState` struct
- Add `IEquatable<T>` implementation to `StatusEffectState` struct

### InstructionRPCToNearby (1 file)
- Remove incorrect `Run(Args)` override since base class `RPCInstructionBase.Run()` is sealed
- Replace `IconRadius` with `IconCircleOutline` (represents range)

## Impact
- **Affected specs**: gamecreator-multiplayer (PowerGrid, RPC, NetworkAdapters)
- **Affected code**:
  - `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/PowerGrid/VisualScripting/`
  - `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/Components/NetworkTraitsAdapter.cs`
  - `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/VisualScripting/RPC/Instructions/InstructionRPCToNearby.cs`
- **Risk**: Low - these are compilation fixes, not behavioral changes
- **Testing**: Unity compilation verification, basic instruction/condition execution tests
