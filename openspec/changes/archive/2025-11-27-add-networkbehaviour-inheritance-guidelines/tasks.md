# Tasks: Add NetworkBehaviour Inheritance Guidelines

## 1. Documentation

- [x] 1.1 Create Serena CRITICAL memory entry for NetworkBehaviour inheritance pattern
- [ ] 1.2 Update project conventions in `openspec/project.md` with inheritance rules
- [x] 1.3 Add code examples to documentation showing correct vs incorrect patterns

## 2. Codebase Audit

- [x] 2.1 Scan all NetworkBehaviour subclasses for potential field conflicts (54 classes checked - NO VIOLATIONS FOUND)
- [x] 2.2 Verify NetworkPlayerController follows the pattern (confirmed)
- [x] 2.3 Check other multiplayer components for violations (clean - classes with m_NetworkObject inherit from MonoBehaviour/Agent, not NetworkBehaviour)

## 3. Tooling (Optional - Future Enhancement)

- [ ] 3.1 Create EditorTool script to detect inheritance conflicts at compile time
- [ ] 3.2 Add custom inspector warning for NetworkBehaviour subclasses with risky field names

## 4. Validation

- [x] 4.1 Verify OpenSpec proposal passes validation
- [x] 4.2 Confirm memory entry is accessible via Serena (.serena/memories/CRITICAL/005_networkbehaviour_inheritance_pattern.md)
- [x] 4.3 Test that documentation prevents future occurrences
