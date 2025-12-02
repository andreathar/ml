## 1. Proposal & Design
- [ ] 1.1 Review KB guide (`claudedocs/guides/How_to_make_queries_to_KB_and_take_decisions.md`) and confirm Qdrant endpoints/fields.
- [ ] 1.2 Finalize UI/UX for “KB Explorer” (filters, pagination, open-in-Cursor flow).
- [ ] 1.3 Define message payload schema and feature flag (`UnityKBQueries`); draft OpenAPI stub for optional bridge.

## 2. Implementation
- [ ] 2.1 Add KB client helper (UnityWebRequest-based) with queueing, debouncing, timeouts, and token support.
- [ ] 2.2 Implement `KB Explorer` EditorWindow with settings, search, filters, results list, and open-in-Cursor action.
- [ ] 2.3 Extend messaging (`MessageType`, `FeatureFlags`) and `VisualStudioIntegration` handlers for KB query request/response/error.
- [ ] 2.4 Add optional local bridge/OpenAPI schema and wiring (behind config).

## 3. Quality & Docs
- [ ] 3.1 Add tests: offline/timeout handling, filtered queries, messaging roundtrip via Python client.
- [ ] 3.2 Add docs: usage guide, config defaults, feature-flag expectations, IDE/client examples.
