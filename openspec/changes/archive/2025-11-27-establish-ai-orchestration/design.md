# AI Orchestration System Design

## Context

The MLCreator project has evolved to include multiple AI systems that currently operate in isolation:
- **Antigravity Editor** (Gemini 3 Pro) - Code editing and generation
- **Serena MCP Server** - Memory management and context
- **Unity MCP Server** - Unity-specific operations
- **Foam Knowledge Base** - Documentation and knowledge graph
- **OpenSpec System** - Specification-driven development
- **Project Brain** - State management and health monitoring

These systems lack coordination, leading to:
- Redundant operations and conflicting instructions
- Inefficient resource usage and missed optimization opportunities
- Inconsistent quality standards and rule enforcement
- Fragmented knowledge and context across systems

## Goals / Non-Goals

### Goals
- Establish single source of truth for all AI behavior
- Enable coordinated multi-agent operations with optimal task routing
- Unify rule enforcement and quality standards across all systems
- Maximize efficiency through intelligent resource allocation
- Provide comprehensive control and monitoring capabilities
- Maintain backward compatibility with existing workflows

### Non-Goals
- Replace existing AI systems (they remain as execution engines)
- Modify core Unity or Game Creator functionality
- Create new AI models or training systems
- Implement cloud-based orchestration (remains local)
- Change existing file formats or project structure

## Decisions

### Decision: Hierarchical Orchestration Architecture

**Chosen Approach**: Three-tier orchestration hierarchy
```
┌─────────────────────────────────┐
│   Orchestration Controller      │ <- Master control layer
├─────────────────────────────────┤
│   Coordination Layer            │ <- Task routing & optimization
├─────────────────────────────────┤
│   Execution Engines             │ <- Individual AI systems
└─────────────────────────────────┘
```

**Rationale**:
- Clear separation of concerns
- Allows independent scaling of each layer
- Maintains existing system interfaces
- Enables gradual migration

**Alternatives Considered**:
- Flat peer-to-peer coordination: Too complex for consistency
- Complete replacement: Too disruptive and risky
- Wrapper-only approach: Insufficient control

### Decision: Configuration-Driven Behavior

**Chosen Approach**: JSON-based configuration with override hierarchy
```json
{
  "orchestration": {
    "version": "1.0.0",
    "rules": {
      "global": { /* Base rules */ },
      "system": { /* System-specific */ },
      "override": { /* Emergency overrides */ }
    },
    "workflows": { /* Workflow definitions */ },
    "agents": { /* Agent registry */ }
  }
}
```

**Rationale**:
- Human-readable and AI-parseable
- Version control friendly
- Easy to modify without code changes
- Supports hot-reloading

### Decision: Event-Driven Coordination

**Chosen Approach**: Asynchronous event bus for inter-system communication
```
Agent Request → Orchestrator → Route Decision → Target System(s) → Results → Orchestrator → Response
```

**Rationale**:
- Non-blocking operations
- Supports parallel execution
- Enables monitoring and logging
- Allows for retry and fallback logic

### Decision: Unified Memory Architecture

**Chosen Approach**: Shared memory pool with orchestrator-managed access
```
Orchestration Memory
├── Context (shared across all systems)
├── Rules (unified rule system)
├── State (current execution state)
├── History (audit trail)
└── Cache (optimized access patterns)
```

**Rationale**:
- Eliminates redundant memory usage
- Enables cross-system learning
- Provides single source of truth
- Supports intelligent preloading

## Architecture Components

### 1. Orchestration Controller
- **Location**: `claudedocs/orchestration/controller/`
- **Responsibilities**:
  - Master rule enforcement
  - Workflow coordination
  - Resource allocation
  - Quality gate management
  - Emergency overrides

### 2. Agent Registry
- **Location**: `claudedocs/orchestration/agents/`
- **Format**: JSON manifests for each agent
```json
{
  "agent_id": "claude-code",
  "capabilities": ["code_generation", "analysis", "testing"],
  "constraints": ["token_limit": 200000],
  "priority": 1,
  "routing_rules": { /* ... */ }
}
```

### 3. Rule Engine
- **Location**: `claudedocs/orchestration/rules/`
- **Components**:
  - Rule parser and validator
  - Priority resolver
  - Conflict detection
  - Override mechanism

### 4. Workflow Manager
- **Location**: `claudedocs/orchestration/workflows/`
- **Features**:
  - Template-based workflows
  - Dynamic workflow generation
  - Progress tracking
  - Rollback capabilities

### 5. Prompt Framework
- **Location**: `claudedocs/orchestration/prompts/`
- **Structure**:
```
prompts/
├── base/           # Foundation prompts
├── personas/       # Specialized personalities
├── contexts/       # Context-specific modifications
└── optimized/      # Performance-tuned variants
```

### 6. Quality Gates
- **Location**: `claudedocs/orchestration/quality/`
- **Checks**:
  - Pre-execution validation
  - Code quality assessment
  - Security scanning
  - Performance analysis
  - Post-execution verification

## Integration Points

### Antigravity Editor Integration
```python
# Before orchestration
antigravity.execute(prompt)

# After orchestration
orchestrator.request({
    "agent": "antigravity",
    "task": prompt,
    "context": orchestrator.get_context(),
    "rules": orchestrator.get_rules()
})
```

### Serena MCP Integration
- Memory tiers become orchestration-managed
- Predictive preloading coordinated across systems
- Unified context window optimization

### Unity MCP Integration
- Tool invocations routed through orchestrator
- Batch operations optimized by orchestrator
- Resource limits enforced by orchestration

### OpenSpec Integration
- Proposals auto-generated from orchestration templates
- Validation automated through quality gates
- Archive operations triggered by orchestrator

## Risks / Trade-offs

### Risk: Increased Complexity
- **Impact**: Harder to debug and maintain
- **Mitigation**: Comprehensive logging, clear documentation, gradual rollout

### Risk: Performance Overhead
- **Impact**: Slower individual operations
- **Mitigation**: Aggressive caching, parallel execution, bypass mode for emergencies

### Risk: Single Point of Failure
- **Impact**: All AI operations fail if orchestrator fails
- **Mitigation**: Fallback mode, health checks, automatic recovery

### Trade-off: Flexibility vs Control
- **Choice**: Prioritize control and consistency
- **Rationale**: Project requires high quality and reliability
- **Mitigation**: Override mechanisms for special cases

### Trade-off: Migration Disruption
- **Choice**: Gradual migration with compatibility layer
- **Rationale**: Minimize risk and maintain productivity
- **Timeline**: 4-week migration window

## Migration Plan

### Phase 1: Foundation (Week 1)
1. Deploy orchestration framework
2. Create agent registry
3. Implement basic routing
4. Set up monitoring

### Phase 2: Integration (Week 2)
1. Integrate Antigravity Editor
2. Connect Serena MCP
3. Link Unity MCP
4. Update documentation systems

### Phase 3: Unification (Week 3)
1. Migrate rules to orchestration
2. Unify workflow templates
3. Consolidate prompts
4. Implement quality gates

### Phase 4: Optimization (Week 4)
1. Performance tuning
2. Resource optimization
3. Parallel execution
4. Final validation

### Rollback Strategy
1. Maintain legacy configurations in `.archive/`
2. Feature flags for orchestration bypass
3. Incremental rollback capability
4. Emergency shutdown procedure

## Performance Targets

- **Task Routing**: <100ms decision time
- **Context Loading**: <500ms for full context
- **Rule Evaluation**: <50ms per rule set
- **Quality Gates**: <2s for standard checks
- **Memory Usage**: <500MB orchestration overhead
- **Parallel Execution**: Up to 10 concurrent operations

## Monitoring and Metrics

### Key Metrics
- Task success rate
- Average routing time
- Resource utilization
- Rule conflicts detected
- Quality gate failures
- System availability

### Dashboards
- Real-time orchestration status
- Agent performance comparison
- Resource usage trends
- Quality metrics
- Error analysis

## Open Questions

1. Should orchestration support external AI services beyond current systems?
2. What level of manual override should be available during normal operations?
3. How should orchestration handle conflicting rules from different sources?
4. Should there be a learning component that optimizes routing over time?
5. What emergency procedures should bypass orchestration entirely?

## Success Criteria

- All AI systems successfully integrated
- 10x improvement in coordinated task execution
- Zero conflicts in rule enforcement
- 50% reduction in redundant operations
- 90% automation of routine workflows
- Full traceability of all AI operations