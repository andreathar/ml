# Technical Design: Unity Project Health Review

## Context

The MLCreator project experienced prefab corruption issues, which serves as a catalyst for comprehensive validation. The project has a complex architecture:

- **Unity 6** (6000.2.13f1) with **Unity Netcode for GameObjects**
- **18 GameCreator modules** with custom multiplayer integration
- **Invasive GameCreator modifications** for Netcode compatibility
- **Critical architecture decisions** (NetworkTransform removal, server authority)
- **Visual scripting** that must work in networked environments

**Key Challenge**: Validate an entire integration stack without breaking existing functionality.

## Goals

### Primary Goals
1. **Validate Integration Solidity**: Confirm GameCreator + Netcode integration is architecturally sound
2. **Prefab Health Verification**: Ensure all network prefabs (especially Player_Network) are correctly configured
3. **Visual Scripting Network Support**: Verify all GameCreator visual scripting components support networked execution
4. **Codebase Hygiene**: Remove technical debt (deprecated tests, obsolete fixes)

### Non-Goals
- **Not fixing individual prefab corruption** - user can re-import
- **Not refactoring working code** - this is validation, not improvement
- **Not adding new features** - analysis and cleanup only
- **Not performance optimization** - unless critical issues found

## Decisions

### Decision 1: Unity MCP Tools as Primary Analysis Method

**Choice**: Use Unity MCP Server tools for automated asset analysis

**Rationale**:
- Direct Unity Editor integration via MCP
- Programmatic access to prefabs, scenes, assets
- Avoids manual inspection errors
- Scalable to entire project
- Generates machine-readable data for reports

**Alternatives Considered**:
- Manual Unity Editor inspection (too slow, error-prone)
- Unity Editor scripts (requires compilation, less flexible)
- Python asset parsing (YAML complexity, potential errors)

**Tools Used**:
| Tool | Purpose | Example Usage |
|------|---------|---------------|
| `Assets_Find` | Enumerate assets by type/filter | Find all NetworkBehaviour scripts |
| `Assets_Read` | Read prefab configuration | Load Player_Network structure |
| `GameObject_Find` | Analyze prefab hierarchy | Inspect component composition |
| `Scene_GetLoaded` | Analyze scene structure | Validate network scene setup |
| `Scene_GetHierarchy` | Get hierarchy details | Map GameObject relationships |
| `Console_GetLogs` | Runtime error analysis | Identify warnings/exceptions |

### Decision 2: Non-Destructive Analysis First, Cleanup Second

**Choice**: Two-phase approach - analyze everything, then cleanup

**Rationale**:
- Understand full scope before making changes
- Generate comprehensive reports for review
- Allow user to approve cleanup actions
- Minimize risk of breaking working code

**Phases**:
1. **Analysis Phase** (tasks 1-10): Read-only validation, report generation
2. **Cleanup Phase** (task 12): Controlled removal of deprecated items

### Decision 3: Architecture Compliance as Critical Validation

**Choice**: Treat architecture decisions as test criteria

**Rationale**:
- NetworkTransform removal is intentional (not a bug)
- Server-authoritative pattern is foundational
- IsNetworkSpawned flag is required for correct initialization
- Violations indicate real problems, not style issues

**Critical Checks**:
- ❌ **FAIL**: NetworkTransform found on player prefabs
- ❌ **FAIL**: Client-authoritative movement code
- ❌ **FAIL**: Missing IsNetworkSpawned flag before spawn
- ✅ **PASS**: NetworkCharacterAdapter present and configured

### Decision 4: Comprehensive Report Generation

**Choice**: Create multiple specialized reports + master report

**Structure**:
```
claudedocs/reports/unity-health-review/
├── MASTER_REPORT.md              # Executive summary + all findings
├── unity-project-structure.md    # Project structure analysis
├── player-network-validation.md  # Player_Network prefab deep dive
├── gamecreator-integration.md    # Module compatibility matrix
├── visual-scripting-network.md   # Visual scripting validation
├── architecture-compliance.md    # Architecture decision adherence
├── performance-validation.md     # Network performance analysis
├── test-cleanup-summary.md       # Deprecated test removal
├── runtime-health.md             # Console logs and runtime issues
└── documentation-validation.md   # Doc accuracy check
```

**Rationale**:
- Specialized reports for different concerns
- Easier to digest than monolithic document
- Master report provides executive summary
- Machine-readable for potential automation

### Decision 5: Test Cleanup Criteria

**Choice**: Remove tests that are:
1. Pre-NetworkCharacterAdapter (testing old architecture)
2. Referencing NetworkTransform (testing removed components)
3. Workarounds superseded by proper fixes
4. Not aligned with current architecture decisions

**Rationale**:
- Tests should validate current architecture
- Outdated tests provide false confidence
- Cleanup improves signal-to-noise ratio

**Keep Tests That**:
- Validate current architecture (NetworkCharacterAdapter)
- Test server-authoritative patterns
- Verify NetworkVariable synchronization
- Test RPC communication
- Validate GameCreator integration points

## Technical Approach

### Analysis Methodology

**Step 1: Asset Enumeration**
```python
# Pseudo-code for Unity MCP approach
network_behaviours = mcp.Assets_Find(filter="t:Script", search="NetworkBehaviour")
network_prefabs = mcp.Assets_Find(filter="t:Prefab", component="NetworkObject")
visual_scripting = mcp.Assets_Find(filter="t:Script", search="Instruction|Condition|Event")
```

**Step 2: Deep Inspection**
```python
# For each critical asset, read full configuration
player_prefab = mcp.Assets_Read(assetPath="Assets/.../Player_Network.asset")
hierarchy = mcp.GameObject_Find(gameObjectRef=player_prefab, includeChildrenDepth=5)

# Validate component composition
components = hierarchy.components
assert "NetworkCharacterAdapter" in components
assert "NetworkTransform" not in components  # Critical check
```

**Step 3: Runtime Validation**
```python
# Check for runtime issues
logs = mcp.Console_GetLogs(logTypeFilter="Error", lastMinutes=60)
warnings = mcp.Console_GetLogs(logTypeFilter="Warning", lastMinutes=60)

# Analyze for network-related issues
network_errors = [log for log in logs if "Network" in log or "RPC" in log]
```

### Validation Patterns

**Prefab Validation Pattern**:
```csharp
// Expected Player_Network structure
Player_Network (GameObject)
├── NetworkObject ✅ (Unity Netcode identity)
├── Character ✅ (GameCreator character)
├── CharacterController ✅ (Unity physics)
├── NetworkCharacterAdapter ✅ (our sync solution)
├── NetworkBehaviour[] ✅ (various network components)
└── NetworkTransform ❌ (MUST NOT be present)
```

**Visual Scripting Validation Pattern**:
```csharp
// Expected Instruction signature
public class InstructionExample : Instruction
{
    // ✅ CORRECT: Task.Run without extra parameters
    protected override Task Run(Args args)
    {
        // Network-aware execution
        if (args.Target.GetComponent<NetworkBehaviour>() is var nb && nb.IsServer)
        {
            // Server-side logic
        }
    }

    // ❌ WRONG: Would cause compilation error
    // protected override Task Run(Args args, CancellationToken ct)
}
```

**RPC Validation Pattern**:
```csharp
// Expected RPC patterns
public class NetworkComponent : NetworkBehaviour
{
    // ✅ CORRECT: Proper naming
    [ServerRpc]
    public void DoSomethingServerRpc(int param) { }

    [ClientRpc]
    public void UpdateClientsClientRpc(Vector3 data) { }

    // ❌ WRONG: Missing suffix
    // [ServerRpc]
    // public void DoSomething(int param) { }
}
```

## Risks & Trade-offs

### Risk 1: Unity MCP Server Availability
**Risk**: Unity MCP Server might not be accessible or responsive
**Mitigation**:
- Verify MCP connectivity in task 1.2
- Have fallback to manual Unity Editor inspection
- Document any MCP limitations encountered

**Trade-off**: Automation vs. Reliability
- Automated analysis is faster but dependent on MCP
- Manual inspection is slower but always available

### Risk 2: False Positives in Deprecated Test Detection
**Risk**: Might incorrectly identify tests as deprecated
**Mitigation**:
- Conservative cleanup criteria (only clear cases)
- Generate cleanup report for review before deletion
- Use git for easy rollback if mistakes made

**Trade-off**: Aggressive cleanup vs. Safety
- Aggressive: Might remove useful tests
- Conservative: Might leave technical debt

### Risk 3: Analysis Overhead
**Risk**: Comprehensive analysis might take significant time
**Mitigation**:
- Parallelize independent analysis tasks
- Use Unity MCP batch operations where possible
- Focus on high-value validations first

**Trade-off**: Depth vs. Speed
- Deep analysis provides confidence but takes time
- Quick scan is fast but might miss issues

### Risk 4: Report Overload
**Risk**: Too many reports might overwhelm the user
**Mitigation**:
- Create master report with executive summary
- Use clear priority/severity indicators
- Organize reports by concern area

**Trade-off**: Comprehensiveness vs. Digestibility
- Detailed reports provide full context
- Summary risks losing important nuances

## Migration Plan

**Pre-Change State**:
- Prefabs may have unknown configuration issues
- Tests include deprecated and current items mixed
- Documentation accuracy unknown
- Integration health unvalidated

**Post-Change State**:
- Complete understanding of project health
- Deprecated tests removed, current tests retained
- Accurate documentation verified
- Integration validated with confidence

**Rollback Plan**:
- Analysis phase is read-only (no rollback needed)
- Cleanup phase uses git for easy revert
- All removed files documented in cleanup summary
- Can restore from git history if needed

**User Actions Required**:
1. Review generated reports (especially master report)
2. Approve cleanup actions (deprecated test removal)
3. Address any critical findings identified
4. Re-import corrupted prefabs if not already done

## Open Questions

### Q1: Should we auto-fix issues found during analysis?
**Status**: No - this change is validation + cleanup only
**Rationale**: Fixes should be separate proposals with proper testing
**Resolution**: Document issues as action items for follow-up

### Q2: How to handle GameCreator module incompatibilities?
**Status**: Document, don't fix
**Rationale**: Module updates might require GameCreator changes
**Resolution**: Flag in integration report, recommend vendor contact if needed

### Q3: Should runtime testing be included?
**Status**: Yes, but limited to log analysis
**Rationale**: Full multiplayer testing is complex, out of scope
**Resolution**: Use Console_GetLogs for passive runtime validation

### Q4: What if Player_Network prefab is fundamentally broken?
**Status**: Document severity, provide recommendations
**Rationale**: This is health check, not emergency repair
**Resolution**: If critical, flag for immediate follow-up proposal

## Success Criteria

### Technical Validation
- ✅ All network prefabs enumerated and analyzed
- ✅ Player_Network validated against architecture requirements
- ✅ All 18 GameCreator modules assessed for network compatibility
- ✅ Visual scripting network support confirmed functional
- ✅ Architecture compliance verified (NetworkTransform removal, etc.)

### Cleanup Validation
- ✅ Deprecated tests identified with clear criteria
- ✅ Obsolete fixes identified and documented
- ✅ Cleanup executed without breaking tests
- ✅ Cleanup summary report generated

### Documentation Quality
- ✅ Master report provides executive summary
- ✅ Specialized reports cover each area thoroughly
- ✅ All findings have severity/priority indicators
- ✅ Action items clearly documented
- ✅ Critical architecture docs verified accurate

### Process Quality
- ✅ OpenSpec validation passes (`openspec validate --strict`)
- ✅ All tasks in tasks.md completed
- ✅ No regressions introduced
- ✅ Git history clean and documented

## Tools & Technologies

### Unity MCP Server
- **Version**: Latest (included in Unity project)
- **Communication**: stdio protocol
- **Configuration**: `Assets/Resources/Unity-MCP-ConnectionConfig.json`
- **Tools Used**: See Decision 1 table

### Analysis Tools
- **Unity Editor**: 6000.2.13f1
- **Git**: Version control and rollback
- **Python**: Report generation scripts (if needed)
- **Markdown**: Report format

### Testing Tools
- **Unity Test Framework**: Validate tests still pass after cleanup
- **NetworkTestHelpers**: Existing test utilities

## Timeline

**Day 1**: Setup + Analysis (tasks 1-7)
- Morning: Setup, structure analysis, prefab validation
- Afternoon: Module integration, visual scripting validation

**Day 2**: Analysis Completion + Reports (tasks 8-11)
- Morning: Architecture compliance, test analysis, runtime validation
- Afternoon: Documentation validation, report generation

**Day 3**: Cleanup + Validation (tasks 12-13)
- Morning: Execute cleanup, verify no regressions
- Afternoon: Final validation, OpenSpec sign-off

## Monitoring & Validation

**During Analysis**:
- Track progress in TodoWrite
- Log all Unity MCP tool calls
- Note any unexpected findings immediately

**Post-Cleanup**:
- Run remaining tests to verify no regressions
- Check Unity console for new errors/warnings
- Validate git diff shows only intended removals

**Final Validation**:
- OpenSpec strict validation
- Peer review of master report
- User approval of findings and cleanup

## Related Documentation

- `.serena/memories/CRITICAL/002_network_architecture_never_forget.md` - Network architecture decisions
- `.serena/memories/CRITICAL/001_gamecreator_invasive_integration.md` - GameCreator integration pattern
- `openspec/project.md` - Project conventions and standards
- `docs/_knowledge-base/01-critical/AI_MANDATORY_CHECKLIST.md` - AI assistant checklist

## Conclusion

This health review provides systematic validation of the Unity + GameCreator + Netcode integration stack. By using Unity MCP tools for automated analysis and following a non-destructive approach, we gain comprehensive understanding of project health without risking regressions. The cleanup phase removes technical debt safely, and the comprehensive reports provide actionable insights for any issues discovered.

**Key Innovation**: Using Unity MCP Server for automated, programmatic project health analysis - scalable and reliable.

**Expected Outcome**: High confidence in integration solidity, clean test suite, accurate documentation, and clear action items for any issues found.
