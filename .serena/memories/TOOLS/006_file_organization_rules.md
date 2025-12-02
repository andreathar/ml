# File Organization Rules (P0 CRITICAL)

**Priority:** P0 CRITICAL - NON-NEGOTIABLE
**Last Updated:** 2025-11-23
**Enforcement:** Manual validation by AI assistants

## Zero Root Folder Pollution Policy

**CRITICAL RULE: AI assistants MUST NOT create unauthorized files at project root.**

### Allowed at Root (ONLY These Files)

✅ **Version Control:**
- `.gitignore`, `.gitattributes`, `.editorconfig`

✅ **AI Configuration:**
- `.claude.json`, `.mcp.json`

✅ **Documentation:**
- `README.md`, `LICENSE`, `LICENSE.txt`

✅ **Unity/Visual Studio:**
- `*.sln`, `*.csproj` (project/solution files)

✅ **That's It** - Nothing else allowed at root!

### Where AI-Generated Files MUST Go

| File Type | Correct Location | Example |
|-----------|-----------------|---------|
| AI reports/analysis | `claudedocs/reports/` | `IMPLEMENTATION_COMPLETE.md` |
| AI action items | `claudedocs/action-items/` | `NEXT_STEPS.md` |
| AI guides/tutorials | `claudedocs/guides/` | `WORKFLOW_GUIDE.md` |
| User documentation | `docs/` | `API_REFERENCE.md` |
| Temporary files | `.temp/` (gitignored) | `debug.txt` |
| AI memories | `.serena/memories/` | Critical knowledge files |

### Common Violations (AUTO-REDIRECT)

**❌ WRONG → ✅ CORRECT:**

```
❌ IMPLEMENTATION_COMPLETE.md      → ✅ claudedocs/reports/IMPLEMENTATION_COMPLETE.md
❌ ANALYSIS.md                     → ✅ claudedocs/reports/ANALYSIS.md
❌ NEXT_STEPS.md                   → ✅ claudedocs/action-items/NEXT_STEPS.md
❌ TODO.md                         → ✅ claudedocs/action-items/TODO.md
❌ WORKFLOW_GUIDE.md               → ✅ claudedocs/guides/WORKFLOW_GUIDE.md
❌ debugging.txt                   → ✅ .temp/debugging.txt
❌ temp_script.py                  → ✅ .temp/temp_script.py
❌ AGENTS.md (if AI reference)     → ✅ claudedocs/guides/AGENTS.md
```

## Pre-Write Validation Checklist

**BEFORE creating ANY file, AI assistants MUST:**

1. ✅ **Check:** Is this path at project root?
2. ✅ **If YES:** Is it in ALLOWED_ROOT list above?
3. ✅ **If NO:** Auto-redirect to proper directory
4. ✅ **Never ask user** - just redirect automatically

### Decision Tree

```
Creating file "REPORT.md"?
├─ Is path at root? → YES
│  ├─ Is in ALLOWED list? → NO
│  └─ AUTO-REDIRECT → claudedocs/reports/REPORT.md ✅
└─ Create file at redirected location
```

## Directory Structure Reference

### AI-Specific Directories

```
claudedocs/
├── reports/           # AI-generated analysis, implementation reports
├── action-items/      # TODOs, next steps, task tracking
└── guides/            # AI-created tutorials, workflows

.serena/
├── memories/          # Structured knowledge base
│   ├── CRITICAL/      # P0 knowledge
│   ├── TOOLS/         # Workflows
│   └── CONTEXT/       # Reference docs
├── ai/                # AI context files (critical.llm.txt)
└── symbols/           # Symbol maps (assemblies.json, etc.)

.project-brain/
├── core/              # Auto-generated state
├── registries/        # System registry
└── validators/        # Cross-reference validation

.temp/                 # Temporary files (gitignored)
└── ...                # Debugging, scratch work
```

### Project Directories

```
docs/                  # User-facing documentation
scripts/               # Utility scripts, automation
Assets/                # Unity assets
openspec/              # Spec-driven development
```

## Test File Organization

### ❌ WRONG: Tests next to source
```
Assets/Plugins/MyPlugin/
├── MyClass.cs
└── MyClass.test.cs          ❌ Don't put tests next to source!
```

### ✅ CORRECT: Tests in dedicated directory
```
Assets/Tests/Runtime/
└── MyPlugin/
    └── MyClassTests.cs      ✅ Proper test organization
```

### Test Directory Patterns

**Unity Test Directories:**
- `Assets/Tests/Runtime/` - Runtime tests
- `Assets/Tests/Editor/` - Editor tests
- `{Assembly}/Tests/` - Assembly-specific tests

**Standard Test Patterns:**
- `tests/` - Generic test directory
- `__tests__/` - JavaScript convention
- `test/` - Alternative pattern

## Script File Organization

### ❌ WRONG: Random script placement
```
project_root/
├── debug.sh             ❌
├── utility.py           ❌
└── helper.js            ❌
```

### ✅ CORRECT: Organized in scripts/
```
scripts/
├── debug.sh             ✅
├── utility.py           ✅
└── helper.js            ✅
```

### Script Categories

```
scripts/
├── build/               # Build automation
├── deploy/              # Deployment scripts
├── tools/               # Development utilities
└── serena-network/      # (Deprecated - marked with DEPRECATED.md)
```

## Workspace Hygiene

### Clean After Operations

**AI assistants MUST clean up:**
- Temporary files created during operations
- Build artifacts not needed long-term
- Debugging outputs after fixing issues
- Test data generated for validation

### Examples

```bash
# After debugging
rm .temp/debug_output.txt ✅

# After script execution
rm temp_script.py ✅

# After build testing
rm -rf .temp/build_artifacts/ ✅
```

### Gitignore Temporary Directories

```
# .gitignore should include:
.temp/
*.log
debug_*
temp_*
```

## Enforcement Mechanism

### Manual Enforcement (Current)

AI assistants self-enforce by:
1. Loading this file at session start via `critical.llm.txt`
2. Checking file paths before Write operations
3. Auto-redirecting unauthorized root files
4. Never asking user - just correct automatically

### Automated Enforcement (Future)

`.project-brain/automation/cleanup-scheduler.ps1`:
- Runs every 30 minutes
- Detects unauthorized root files
- Generates warnings in health report
- Suggests correct locations

## Violation Examples and Corrections

### Example 1: Implementation Report

**❌ User might say:** "Create IMPLEMENTATION_COMPLETE.md"

**❌ Naive AI would create:** `./IMPLEMENTATION_COMPLETE.md`

**✅ Correct AI behavior:**
```
Analyzing request: "Create IMPLEMENTATION_COMPLETE.md"
→ File type: Implementation report
→ Default path: ./IMPLEMENTATION_COMPLETE.md (ROOT)
→ Root allowed? NO (not in ALLOWED list)
→ Auto-redirect: claudedocs/reports/IMPLEMENTATION_COMPLETE.md ✅
→ Create file at redirected location
```

### Example 2: Debugging Script

**❌ User might say:** "Create debug.sh to test this"

**✅ Correct AI behavior:**
```
Analyzing request: "Create debug.sh"
→ File type: Script
→ Default path: ./debug.sh (ROOT)
→ Root allowed? NO
→ Auto-redirect: .temp/debug.sh ✅ (temporary + gitignored)
→ Create file at redirected location
```

### Example 3: User Documentation

**❌ User might say:** "Create API_GUIDE.md"

**✅ Correct AI behavior:**
```
Analyzing request: "Create API_GUIDE.md"
→ File type: Documentation
→ Is this AI-internal or user-facing? → Analyze content
→ If user-facing: docs/API_GUIDE.md ✅
→ If AI-internal: claudedocs/guides/API_GUIDE.md ✅
→ Create at appropriate location
```

## Current Violations (As of 2025-11-23)

**From `.project-brain/core/state.json`:**
```json
{
  "health_status": {
    "warnings": ["13 unauthorized files in project root"]
  }
}
```

**Action Required:**
- Project Brain identifies 13 root violations
- Manual cleanup needed for existing files
- This rule prevents new violations

## Related Documentation

- `.project-brain/core/state.json` - Tracks root violations
- `.project-brain/automation/cleanup-scheduler.ps1` - Automated enforcement (future)
- `.claude/CLAUDE.md` - Primary AI instructions (lines 44-87)

## Quick Reference Card

```
BEFORE CREATING ANY FILE:

1. Is file path at root?
   → YES: Check ALLOWED list
   → NO: Create normally

2. In ALLOWED list?
   → YES: Create at root ✅
   → NO: Go to step 3

3. Determine file type:
   → AI report: claudedocs/reports/
   → AI action: claudedocs/action-items/
   → AI guide: claudedocs/guides/
   → User docs: docs/
   → Temporary: .temp/
   → Test: Assets/Tests/ or tests/
   → Script: scripts/

4. Create at redirected location ✅

5. NEVER ask user about redirect - just do it!
```

## Exceptions (NONE)

**There are NO exceptions to this rule.**

- Not for simple files
- Not for "just this once"
- Not because user explicitly requested root location
- Not for any reason

**If user specifically asks for root location, educate and redirect anyway.**
