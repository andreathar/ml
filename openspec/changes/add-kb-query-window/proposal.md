# Change: Unity KB Query Window & IDE Bridge

## Why
- Developers need an in-Editor way to query the local Unity KB (Qdrant `unity_project_kb`) without leaving Unity.
- IDEs (Cursor/VS Code) should be able to request KB searches over the existing messaging channel, but current protocol lacks a KB query feature and safe opt-in flag.
- Centralizing filters/throttling avoids noisy or conflicting requests while keeping current clients unaffected.

## What Changes
- Add a Unity Editor window (“KB Explorer”) with configurable host/port/token, debounced search, filters, pagination, and open-in-Cursor actions.
- Add a Qdrant HTTP client helper with request queueing, timeouts, and lightweight caching; default targets localhost:6333 `unity_project_kb`.
- Extend messaging with KB query request/response/error types plus a gated feature flag `UnityKBQueries`; IDE can request queries when negotiated.
- Optional OpenAPI schema for the KB bridge endpoint to support future VS Code extension or local daemon queueing.

## Impact
- Affected specs: messaging, editor tooling, IDE integration.
- Affected code: `Editor/VisualStudioIntegration.cs`, `Editor/Messaging/MessageType.cs`, new editor window + KB client, docs.
- Non-breaking: fully gated by new feature flag; default behavior unchanged for existing clients.
