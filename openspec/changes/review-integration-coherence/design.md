# Design: AI Integration Coherence

## Context

The MLCreator project uses four interconnected AI/tooling systems:

1. **OpenSpec** - Specification-driven development workflow
2. **Serena** - Hierarchical memory management with MCP integration
3. **Unity MCP Server** - Direct Unity Editor integration via stdio
4. **Unity-KB (Qdrant)** - Knowledge base with 77,914+ indexed symbols

These systems have been configured incrementally and require coherence review to ensure seamless AI assistant workflows.

## Goals / Non-Goals

### Goals
- Ensure all four systems are properly integrated
- Define and enforce strong rules for AI assistants
- Create consistent configuration across all files
- Document clear workflows for common AI tasks

### Non-Goals
- Adding new features to any system
- Changing existing architecture patterns
- Modifying Unity MCP Server implementation
- Restructuring Qdrant KB schema

## Decisions

### Decision 1: Canonical Configuration Location

**What**: `.serena/config.yaml` is the canonical MCP configuration source.

**Why**: Serena's hierarchical memory system is designed for AI context management. Other files (`.mcp.json`, `.claude/settings.local.json`) should reference or mirror this configuration.

**Alternatives considered**:
- `.mcp.json` as canonical: Rejected - Claude Code specific
- Separate configs per system: Rejected - causes drift

### Decision 2: Memory Tier Usage

**What**: Strict adherence to Serena memory tiers:

| Tier | Content | Access Pattern |
|------|---------|----------------|
| CRITICAL | Architecture decisions, never-forget rules | Always loaded |
| CONTEXT | Project-specific patterns | Loaded on context |
| INTEGRATION | Framework rules (GC, Netcode) | Loaded when relevant |
| TOOLS | Dev utility configs | On-demand |

**Why**: Consistent memory organization enables predictable AI behavior.

### Decision 3: KB Query Requirement

**What**: AI assistants MUST query Unity-KB before proposing solutions for:
- Multiplayer/sync components
- GameCreator integrations
- Architecture decisions

**Why**: KB contains 77,914+ indexed symbols with intelligent keyword taxonomy. Failing to query leads to:
- Duplicate implementations
- Pattern violations
- Missed optimization opportunities

### Decision 4: OpenSpec as Change Gate

**What**: All non-trivial changes require OpenSpec proposals.

**Why**: Prevents:
- Undocumented architecture decisions
- Incomplete implementations
- Spec-code drift

**Exception**: Bug fixes restoring existing spec behavior may proceed directly.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| KB unavailability blocks AI work | Filesystem fallback documented |
| Config drift between files | Periodic sync script + validation |
| Over-bureaucratization | Clear "skip proposal" criteria |
| Memory tier confusion | Template files in each tier |

## Migration Plan

1. **Phase 1: Audit** (Current)
   - Review all configuration files
   - Identify inconsistencies
   - Document current state

2. **Phase 2: Synchronization**
   - Update `.mcp.json` to match `.serena/config.yaml`
   - Update `.claude/settings.local.json` permissions
   - Synchronize AGENTS.md files

3. **Phase 3: Strong Rules**
   - Create enforcement checklist
   - Add validation to AI workflow
   - Update CRITICAL memories

4. **Rollback**: 
   - All changes are documentation/configuration
   - Git revert available for any file

## Open Questions

1. Should we add automated config validation script?
2. Should KB health check be mandatory or advisory?
3. Do we need additional CRITICAL memory files?
