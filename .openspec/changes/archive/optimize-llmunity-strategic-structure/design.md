# Design: LLMUnity Strategic Structure Optimization

## Architectural Principles

### 1. Unified Module Architecture
**Principle**: All multiplayer integrations live within the Game Creator Multiplayer strategic structure.

**Rationale**:
- **Cohesion**: Related functionality grouped together under `GameCreator_Multiplayer/`
- **Discoverability**: Developers know where to find multiplayer-related code
- **Consistency**: Follows established patterns from existing modules (Behavior, CityVehicles)
- **Maintenance**: Single structure simplifies updates and refactoring

**Anti-pattern to avoid**: Parallel structures at `Assets/Scripts/` that create namespace fragmentation.

### 2. Assembly-First Organization
**Principle**: Every module has a dedicated assembly definition with explicit dependencies.

**Rationale**:
- **Compilation Performance**: Only recompile affected assemblies
- **Dependency Management**: Explicit references prevent circular dependencies
- **Namespace Enforcement**: Assembly boundaries enforce clean architecture
- **IDE Performance**: Better IntelliSense and refactoring support

**Implementation**:
```json
{
  "name": "GameCreator.Multiplayer.Runtime.LLM",
  "rootNamespace": "GameCreator.Multiplayer.Runtime.LLM",
  "references": [
    "GameCreator.Runtime.Core",
    "GameCreator.Multiplayer.Runtime",
    "undream.llmunity.Runtime",
    "Unity.Netcode.Runtime"
  ]
}
```

### 3. Invasive Integration Pattern
**Principle**: Integrate with Game Creator at the framework level, not as external wrapper.

**Rationale** (from `.serena/memories/CRITICAL/001_gamecreator_invasive_integration.md`):
- **No API Changes**: Game Creator users don't change their code
- **Seamless Integration**: Multiplayer features work with existing Game Creator components
- **Performance**: No wrapper overhead, direct property access
- **Single Codebase**: Not separate "multiplayer LLM" - just LLM that works in multiplayer

**Applied to LLM**:
- `LLMNetworkCharacter` wraps `LLMCharacter` with NetworkBehaviour
- Visual scripting (Instructions/Events) integrate directly into Game Creator's system
- Server-authoritative pattern maintains consistency across clients

## Design Decisions

### Decision 1: Location within GameCreator_Multiplayer
**Options Considered**:
1. ❌ Keep at `Assets/Scripts/MLCreator/Runtime/LLM/`
2. ❌ Create separate `Assets/Plugins/MLCreator/LLM/`
3. ✅ Move to `Assets/Plugins/GameCreator/Packages/GameCreator_Multiplayer/Runtime/LLM/`

**Choice**: Option 3 - Within GameCreator_Multiplayer

**Reasoning**:
- **Consistency**: Matches existing module structure (Behavior, CityVehicles)
- **Strategic Alignment**: LLM is a multiplayer feature, not standalone
- **Namespace Unity**: `GameCreator.Multiplayer.Runtime.LLM` follows established pattern
- **Dependency Clarity**: Clear relationship to multiplayer base assembly

**Trade-offs**:
- ✅ Better organization and discoverability
- ✅ Follows project conventions
- ⚠️ Requires file moves and namespace updates (one-time cost)
- ⚠️ Couples LLM to Game Creator version (acceptable - already coupled via invasive integration)

### Decision 2: Assembly Granularity
**Options Considered**:
1. ❌ No assembly definition (compile with everything)
2. ❌ One assembly for all MLCreator features
3. ✅ Dedicated assembly per module (`GameCreator.Multiplayer.Runtime.LLM`)

**Choice**: Option 3 - Dedicated assembly per module

**Reasoning**:
- **Explicit Dependencies**: Clear what LLM module depends on
- **Compilation Performance**: Changes to LLM don't recompile unrelated code
- **Modularity**: Can be enabled/disabled independently
- **Testing Isolation**: Easier to test LLM module in isolation

**References Required**:
```
GameCreator.Runtime.Core          → Core Game Creator functionality
GameCreator.Multiplayer.Runtime   → Base multiplayer features
undream.llmunity.Runtime          → LLMUnity plugin
Unity.Netcode.Runtime             → Unity Netcode for GameObjects
```

### Decision 3: Namespace Strategy
**Options Considered**:
1. ❌ Keep `MLCreator.Runtime.LLM`
2. ❌ Use `GameCreator.Extensions.LLM`
3. ✅ Use `GameCreator.Multiplayer.Runtime.LLM`

**Choice**: Option 3 - Follow multiplayer module pattern

**Reasoning**:
- **Pattern Consistency**: Matches `GameCreator.Multiplayer.Runtime.Behavior`
- **Clear Ownership**: Obviously part of multiplayer system
- **IDE Navigation**: Groups with other multiplayer modules in IntelliSense
- **Assembly Alignment**: Namespace matches assembly name

**Migration Path**:
1. Move files to new location
2. Update namespace declarations in all `.cs` files
3. Unity's refactoring handles most reference updates automatically
4. Manual verification for any broken references

## Module Structure

### Directory Layout
```
GameCreator_Multiplayer/Runtime/LLM/
├── GameCreator.Multiplayer.Runtime.LLM.asmdef
│
├── Core/
│   ├── LLMNetworkCharacter.cs       # NetworkBehaviour wrapper
│   └── LLMService.cs                # Singleton service (future)
│
├── Instructions/                     # Visual Scripting Actions
│   ├── InstructionLLMChat.cs        # Send prompt to LLM
│   └── InstructionLLMStop.cs        # Cancel generation
│
└── Events/                          # Visual Scripting Triggers
    └── EventLLMReply.cs             # On LLM reply received
```

**Rationale**:
- **Core/**: Contains networking and service logic
- **Instructions/**: Game Creator Actions (player-initiated)
- **Events/**: Game Creator Triggers (system-initiated)
- **Future-proof**: Room for RAG/, Embeddings/, etc. as needed

### Assembly Reference Graph
```
GameCreator.Multiplayer.Runtime.LLM
├── → GameCreator.Runtime.Core (Game Creator base)
├── → GameCreator.Multiplayer.Runtime (Multiplayer base)
├── → undream.llmunity.Runtime (LLMUnity plugin)
└── → Unity.Netcode.Runtime (Networking)
```

**Validation**:
- No circular dependencies
- All dependencies are runtime assemblies (no Editor dependencies in Runtime)
- Follows layered architecture: Plugin → Framework → Integration

## Integration Points

### With Game Creator Core
- **Visual Scripting**: Instructions/Events appear in Game Creator menus
- **Property Binding**: Uses `PropertyGetGameObject`, `PropertyGetString` for variable binding
- **Character System**: Integrates with `Character` class via `LLMNetworkCharacter`

### With LLMUnity Plugin
- **LLMCharacter**: Wrapped by `LLMNetworkCharacter` for multiplayer
- **LLM**: Service singleton manages model lifecycle
- **RAG** (future): Semantic search integration

### With Unity Netcode
- **NetworkBehaviour**: `LLMNetworkCharacter` synchronizes LLM state
- **ServerRpc**: Client sends prompts to server
- **ClientRpc**: Server broadcasts replies to clients
- **Server-Authoritative**: LLM generation only on server/host

## Migration Strategy

### Phase 1: Preparation (Low Risk)
1. Create new directory structure
2. Create assembly definition with references
3. **Checkpoint**: Validate assembly definition compiles

### Phase 2: Migration (Medium Risk)
4. Copy files to new location (don't delete old yet)
5. Update namespaces in copied files
6. **Checkpoint**: Verify new files compile

### Phase 3: Transition (Medium Risk)
7. Update any manual references to old namespace
8. Test visual scripting components
9. **Checkpoint**: Full project compilation and basic functionality test

### Phase 4: Cleanup (Low Risk)
10. Delete old files at `Assets/Scripts/MLCreator/Runtime/LLM/`
11. Remove empty parent directories
12. **Checkpoint**: Final compilation check

### Rollback Plan
- If issues arise in Phase 2-3: Delete new files, keep old structure
- Unity's `.meta` files track GUIDs, so references should auto-restore
- Git provides safety net for file moves

## Validation Criteria

### Compilation Validation
```bash
# Unity should compile without errors
# Check console for:
- No "namespace not found" errors
- No "type not found" errors
- No assembly reference errors
```

### Assembly Reference Validation
```csharp
// All required types should resolve:
using GameCreator.Runtime.Core;               // ✓
using GameCreator.Multiplayer.Runtime.LLM;    // ✓
using LLMUnity;                               // ✓
using Unity.Netcode;                          // ✓
```

### Visual Scripting Validation
- Instructions appear under: `Game Creator > Actions > LLM > ...`
- Events appear under: `Game Creator > Events > LLM > ...`
- Property binding works (e.g., `GetGameObjectPlayer`)

### Namespace Consistency Validation
```bash
# All files in LLM/ should have matching namespace
grep -r "namespace" GameCreator_Multiplayer/Runtime/LLM/*.cs
# Expected: All show "GameCreator.Multiplayer.Runtime.LLM"
```

## Performance Considerations

### Compilation Time
**Before**: Single large assembly recompiles on any change
**After**: Only `GameCreator.Multiplayer.Runtime.LLM` recompiles for LLM changes

**Estimated Impact**:
- ~10-30 second reduction in compilation time for LLM-only changes
- Baseline Unity + Game Creator compilation: ~60-90 seconds
- LLM module only: ~5-10 seconds

### Runtime Performance
**No change**: This is purely organizational. Runtime behavior identical.

### IDE Performance
**Improvement**: Better IntelliSense performance with explicit assembly boundaries

## Security & Safety

### File Move Safety
- Unity tracks files by GUID in `.meta` files
- Moving files preserves GUIDs and references
- Use Unity Editor's move/rename (not file system) for safest operation
- Git provides rollback capability

### Dependency Safety
- Explicit assembly references prevent accidental dependencies
- Compilation errors immediately show broken references
- No runtime dependency loading - all compile-time

### Testing Safety
- Can test new structure alongside old (Phase 2)
- Incremental migration reduces risk
- Multiple checkpoints allow early problem detection

## Documentation Updates Required

### Update Locations
1. **LLMUnity_Integration_Architecture.md**
   - Update file paths to new location
   - Update namespace references
   - Note assembly definition

2. **AI_MANDATORY_CHECKLIST.md**
   - Update LLM integration location reference

3. **Project README**
   - Update directory structure diagram
   - Update namespace examples

4. **OpenSpec Proposals**
   - `fix-llmunity-gamecreator-integration`: Add note about relocation
   - `integrate-llmunity-workflow`: Update references

## Success Metrics

### Quantitative
- ✅ 0 compilation errors
- ✅ 0 orphaned files at old location
- ✅ 100% namespace consistency in LLM module
- ✅ All 3 visual scripting components (2 Instructions, 1 Event) functional

### Qualitative
- ✅ Structure matches existing multiplayer modules (Behavior pattern)
- ✅ Easy to navigate and discover LLM functionality
- ✅ Clear assembly dependency graph
- ✅ Documentation accurately reflects new structure

## Future Extensibility

### RAG Integration (Future)
```
GameCreator_Multiplayer/Runtime/LLM/
├── Core/
├── Instructions/
├── Events/
└── RAG/                     # Future: Semantic search
    ├── RAGSearch.cs
    ├── InstructionRAGSearch.cs
    └── InstructionRAGAdd.cs
```

### Embeddings Service (Future)
- Add to Core/ or separate Embeddings/ directory
- Maintain same assembly or create sub-assembly as needed
- Pattern established: easy to extend

### Multi-LLM Support (Future)
- Current: Single LLM instance
- Future: Multiple LLM models or providers
- Structure supports: Add provider abstraction in Core/

## Conclusion

This design establishes LLM integration as a **first-class multiplayer module** within the strategic Game Creator architecture. The reorganization:

1. **Eliminates parallel structures** that fragment the codebase
2. **Establishes clear assembly boundaries** for better compilation and IDE performance
3. **Follows proven patterns** from existing multiplayer modules
4. **Maintains project conventions** and strategic architecture principles
5. **Enables future growth** with clean, extensible structure

The one-time migration cost is justified by long-term maintainability, consistency, and alignment with project architecture goals.
