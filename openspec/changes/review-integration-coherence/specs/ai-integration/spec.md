# AI Integration Coherence Specification

## ADDED Requirements

### Requirement: Mandatory KB Query Before Analysis

AI assistants SHALL query the Unity Knowledge Base before proposing any solution that involves:
- Adding new multiplayer components
- Modifying existing sync patterns
- Creating new GameCreator integrations
- Architectural decisions

#### Scenario: AI proposes multiplayer feature
- **WHEN** AI assistant receives a request to add multiplayer functionality
- **THEN** AI SHALL first query Unity-KB for existing patterns
- **AND** AI SHALL reference KB results in the proposal rationale

#### Scenario: KB unavailable fallback
- **WHEN** Unity-KB health check fails (http://localhost:6333/healthz returns error)
- **THEN** AI SHALL notify user of degraded capability
- **AND** AI SHALL proceed with filesystem-based search as fallback

### Requirement: Memory Tier Compliance

All AI-discovered critical decisions SHALL be stored in the appropriate Serena memory tier:
- **CRITICAL**: Architecture decisions, breaking changes, never-forget rules
- **CONTEXT**: Project-specific patterns, conventions
- **INTEGRATION**: Framework-specific rules (GameCreator, Netcode)
- **TOOLS**: Development utility configurations

#### Scenario: Critical architecture decision
- **WHEN** AI identifies a critical architecture decision (e.g., "never use NetworkTransform on players")
- **THEN** AI SHALL create or update a file in `.serena/memories/CRITICAL/`
- **AND** file SHALL follow naming convention `NNN_descriptive_name.md`

#### Scenario: Tool configuration change
- **WHEN** AI modifies MCP server or tool configuration
- **THEN** AI SHALL update `.serena/memories/TOOLS/` with the change rationale

### Requirement: OpenSpec Proposal Workflow

All non-trivial changes SHALL follow the OpenSpec proposal workflow:
- Bug fixes restoring existing behavior: Direct fix allowed
- New features: Proposal required
- Architecture changes: Proposal required
- Breaking changes: Proposal required with migration plan

#### Scenario: New feature request
- **WHEN** user requests a new feature affecting multiple files
- **THEN** AI SHALL create an OpenSpec proposal in `.openspec/changes/`
- **AND** AI SHALL await user approval before implementation

#### Scenario: Simple bug fix
- **WHEN** user reports a bug that restores existing spec behavior
- **THEN** AI MAY fix directly without proposal
- **AND** AI SHALL reference the spec being restored

### Requirement: MCP Configuration Consistency

MCP server configurations SHALL be consistent across all configuration files:
- `.mcp.json` (Claude Code)
- `.serena/config.yaml` (Serena)
- `.serena/project.yml` (Serena project settings)

#### Scenario: Adding new MCP server
- **WHEN** a new MCP server is added to the project
- **THEN** configuration SHALL be added to all three files
- **AND** server paths and arguments SHALL be identical

#### Scenario: Configuration mismatch detected
- **WHEN** AI detects inconsistent MCP configurations across files
- **THEN** AI SHALL notify user of the mismatch
- **AND** AI SHALL propose unified configuration update

### Requirement: Architecture Decision Recording

All significant architecture decisions SHALL be recorded in `.serena/memories/CRITICAL/` with:
- Priority designation (P0 CRITICAL)
- Last updated date
- Applies-to scope
- Core principle explanation
- Code patterns (correct and incorrect)
- Related documentation links

#### Scenario: New architecture decision
- **WHEN** team makes a decision that affects system design
- **THEN** decision SHALL be documented following the template in `001_gamecreator_invasive_integration.md`
- **AND** document SHALL include Quick Reference table

#### Scenario: Architecture decision violation
- **WHEN** AI detects code that violates a documented architecture decision
- **THEN** AI SHALL warn user with reference to the CRITICAL memory file
- **AND** AI SHALL NOT proceed with implementation until user confirms override

## ADDED Requirements

### Requirement: Integration Health Monitoring

The project SHALL maintain health monitoring for all AI integrations:
- Unity MCP Server: Stdio connection and response
- Unity-KB (Qdrant): HTTP health endpoint and symbol count
- Serena: Memory tier accessibility
- OpenSpec: CLI command availability

#### Scenario: Startup health check
- **WHEN** AI assistant session begins
- **THEN** AI SHOULD verify Unity-KB accessibility via dashboard or API
- **AND** AI SHOULD note any degraded services

#### Scenario: Service recovery
- **WHEN** a previously unavailable service becomes available
- **THEN** AI SHALL resume using that service for subsequent queries
