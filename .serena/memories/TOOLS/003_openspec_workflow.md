# OpenSpec Change Proposal Workflow

**Category:** TOOLS - Spec-Driven Development
**Last Updated:** 2025-11-23
**Documentation:** `openspec/AGENTS.md`

## Three-Stage Workflow

### Stage 1: Creating Changes

**When to create a proposal:**
- Adding features or functionality
- Making breaking changes (API, schema)
- Changing architecture or patterns
- Optimizing performance (changes behavior)
- Updating security patterns

**Skip proposal for:**
- Bug fixes (restoring intended behavior)
- Typos, formatting, comments
- Dependency updates (non-breaking)
- Configuration changes
- Tests for existing behavior

### Stage 2: Implementing Changes

**Sequential implementation:**
1. Read `proposal.md` - Understand what's being built
2. Read `design.md` (if exists) - Review technical decisions
3. Read `tasks.md` - Get implementation checklist
4. Implement tasks sequentially
5. Confirm completion
6. Update checklist - Set every task to `- [x]`

### Stage 3: Archiving Changes

After deployment:
```bash
openspec archive <change-id> --yes
```

Or for tooling-only changes (no spec updates):
```bash
openspec archive <change-id> --skip-specs --yes
```

## Creating a Proposal

### Step 1: Choose Change ID

**Pattern:** `verb-noun-description`

```
✅ Good IDs:
- add-two-factor-auth
- update-network-sync
- remove-legacy-api
- refactor-character-movement

❌ Bad IDs:
- feature1
- update
- my-change
```

### Step 2: Create Directory Structure

```bash
mkdir -p openspec/changes/<change-id>/specs/<capability>
```

**Example:**
```bash
mkdir -p openspec/changes/add-inventory-sync/specs/inventory-system
```

### Step 3: Write proposal.md

```markdown
# Change: [Brief description]

## Why
[1-2 sentences on problem/opportunity]

## What Changes
- [Bullet list of changes]
- [Mark breaking changes with **BREAKING**]

## Impact
- Affected specs: [list capabilities]
- Affected code: [key files/systems]
```

### Step 4: Write tasks.md

```markdown
## 1. Implementation
- [ ] 1.1 Create database schema
- [ ] 1.2 Implement API endpoint
- [ ] 1.3 Add frontend component
- [ ] 1.4 Write tests
```

### Step 5: Write design.md (if needed)

**Create design.md when:**
- Cross-cutting change (multiple services/modules)
- New external dependency or data model changes
- Security, performance, or migration complexity
- Ambiguity requiring technical decisions upfront

**Skip design.md when:**
- Simple, localized change
- Well-understood pattern
- No architectural impact

### Step 6: Create Spec Deltas

**Location:** `openspec/changes/<change-id>/specs/<capability>/spec.md`

```markdown
## ADDED Requirements
### Requirement: New Feature
The system SHALL provide...

#### Scenario: Success case
- **WHEN** user performs action
- **THEN** expected result

## MODIFIED Requirements
### Requirement: Existing Feature
[Complete modified requirement with all scenarios]

## REMOVED Requirements
### Requirement: Old Feature
**Reason**: [Why removing]
**Migration**: [How to handle]
```

## Critical Formatting Rules

### Scenario Format (REQUIRED)

**✅ CORRECT:**
```markdown
#### Scenario: User login success
- **WHEN** valid credentials provided
- **THEN** return JWT token
```

**❌ WRONG:**
```markdown
- **Scenario: User login**       ← Don't use bullet
**Scenario**: User login          ← Don't use bold
### Scenario: User login          ← Wrong header level
```

### Requirement Operations

**ADDED vs MODIFIED:**
- **ADDED:** New capability that can stand alone
- **MODIFIED:** Changes to existing requirement behavior

**When using MODIFIED:**
1. Find existing requirement in `openspec/specs/<capability>/spec.md`
2. Copy entire requirement block (header + scenarios)
3. Paste under `## MODIFIED Requirements`
4. Edit to reflect new behavior
5. Keep header text matching exactly (whitespace-insensitive)

## Validation

### Before Sharing Proposal

```bash
openspec validate <change-id> --strict
```

**Common errors:**
- "Change must have at least one delta" → Check `specs/` directory exists
- "Requirement must have at least one scenario" → Check `#### Scenario:` format
- Silent parsing failures → Debug with `openspec show <change-id> --json --deltas-only`

## Implementation Pattern

### Before Starting

```bash
# Check current state
openspec list                # Active changes
openspec list --specs        # Existing capabilities
openspec show <change-id>    # View proposal details
```

### During Implementation

**Update tasks.md as you work:**

```markdown
## 1. Implementation
- [x] 1.1 Create database schema ✅ Completed
- [x] 1.2 Implement API endpoint ✅ Completed
- [ ] 1.3 Add frontend component ← Currently working
- [ ] 1.4 Write tests
```

### After Completion

**Verify all tasks are `- [x]` before archiving.**

## Multi-Capability Changes

**When change affects multiple capabilities:**

```
openspec/changes/add-2fa-notify/
├── proposal.md
├── tasks.md
├── design.md
└── specs/
    ├── auth/
    │   └── spec.md        # ADDED: Two-Factor Authentication
    └── notifications/
        └── spec.md        # ADDED: OTP email notification
```

## Integration with Other Systems

### With Serena Memories

**Reference specs in memory files:**
```markdown
## Related Documentation
- `openspec/specs/character-system/spec.md` - Character requirements
```

### With Project Brain

**Project Brain validates spec references:**
- Detects broken links to OpenSpec files
- Includes active changes in health monitoring
- Suggests spec updates for outdated references

### With Unity MCP

**OpenSpec tasks can trigger Unity MCP operations:**
```markdown
## 1. Unity Asset Creation
- [ ] 1.1 Create network player prefab (Unity MCP)
- [ ] 1.2 Configure NetworkObject component (Unity MCP)
```

## CLI Reference

```bash
# Essential commands
openspec list                         # List active changes
openspec list --specs                 # List specifications
openspec show [item]                  # Display change or spec
openspec validate [item]              # Validate changes/specs
openspec archive <change-id> --yes    # Archive after deployment

# Validation
openspec validate <change-id> --strict

# Debugging
openspec show <change-id> --json --deltas-only
```

## Best Practices

### 1. Small, Focused Changes

**Prefer:**
- Single capability per change
- < 100 lines of new code (when possible)
- Clear, testable requirements

**Avoid:**
- Mega-changes affecting 5+ capabilities
- Mixing unrelated features
- Vague requirements

### 2. Clear Requirements

**Use SHALL/MUST for normative requirements:**
```markdown
The system SHALL validate user input ✅
The system should validate user input ❌
```

### 3. Concrete Scenarios

**Every requirement needs ≥1 scenario:**
```markdown
### Requirement: User Authentication
The system SHALL authenticate users with valid credentials.

#### Scenario: Valid credentials
- **WHEN** user provides valid username and password
- **THEN** system grants access and returns JWT token

#### Scenario: Invalid credentials
- **WHEN** user provides invalid credentials
- **THEN** system denies access and returns error message
```

## Related Documentation

- `openspec/AGENTS.md` - Complete OpenSpec guide
- `openspec/project.md` - Project conventions
- `.serena/memories/TOOLS/004_project_brain_integration.md` - Project Brain integration

## Quick Decision Tree

```
Need to make a change?
├─ Bug fix? → Just fix it (no proposal)
├─ Typo/comment? → Just fix it
├─ New feature? → Create proposal
├─ Breaking change? → Create proposal
├─ Architecture change? → Create proposal (with design.md)
└─ Unclear? → Create proposal (safer)
```

## Quick Reference

| Task | Command |
|------|---------|
| List changes | `openspec list` |
| List specs | `openspec list --specs` |
| View details | `openspec show <change-id>` |
| Validate | `openspec validate <change-id> --strict` |
| Archive | `openspec archive <change-id> --yes` |
