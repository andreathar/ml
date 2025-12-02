# Sentry Usage Guide

## Purpose
Sentry is a real-time error tracking and performance monitoring platform. It captures errors, exceptions, and performance data from applications.

## Current Configuration
**File**: `sentry_config.py` (Python tools only)
```python
sentry_sdk.init(
    dsn="https://...",
    environment="development",
    release="mlcreator-python-tools",
    traces_sample_rate=1.0,
    debug=True
)
```

## When to Use
- Investigating runtime errors
- Analyzing error patterns
- Monitoring performance
- Debugging production issues

## Output Patterns

### Error Event
```json
{
  "event_id": "abc123",
  "level": "error",
  "exception": {
    "type": "NullReferenceException",
    "value": "Object reference not set to an instance of an object",
    "stacktrace": {
      "frames": [
        {"filename": "PlayerController.cs", "lineno": 45, "function": "Update"}
      ]
    }
  },
  "breadcrumbs": [
    {"category": "navigation", "message": "Loaded MainScene"},
    {"category": "user", "message": "Clicked Play button"}
  ]
}
```

### Performance Transaction
```json
{
  "transaction": "PlayerSpawn",
  "duration": 1250,
  "spans": [
    {"op": "db.query", "duration": 200},
    {"op": "serialize", "duration": 50}
  ]
}
```

## AI Interpretation Guide

### Error Severity Levels
| Level | Meaning | Action |
|-------|---------|--------|
| `fatal` | Application crashed | Immediate fix required |
| `error` | Operation failed | Fix in current sprint |
| `warning` | Potential issue | Monitor, fix if recurring |
| `info` | Informational | No action needed |

### Interpreting Stack Traces
1. **Top of stack**: Where error was thrown
2. **Middle frames**: Call chain
3. **Bottom frames**: Entry point

```
PlayerController.Update() ← Error here
  ↑
CharacterMovement.Move()
  ↑
GameManager.Tick()
  ↑
MonoBehaviour.Update() ← Unity called this
```

### Breadcrumb Analysis
Breadcrumbs show what happened before the error:
- **navigation**: Scene/page changes
- **user**: User interactions
- **http**: Network requests
- **console**: Log messages

### Common Error Patterns
| Pattern | Cause | Fix |
|---------|-------|-----|
| `NullReferenceException` | Uninitialized object | Add null checks |
| `IndexOutOfRangeException` | Invalid array index | Validate bounds |
| `MissingReferenceException` | Destroyed Unity object | Check IsDestroyed |
| `SocketException` | Network failure | Add retry logic |

## Python Integration
```python
from sentry_config import init_sentry
init_sentry()

# Errors automatically captured
# Manual capture:
sentry_sdk.capture_exception(error)
sentry_sdk.capture_message("Custom event")
```

## Unity Integration (Future)
Unity SDK package: `Sentry.Unity`
- Captures Unity-specific errors
- Includes device/platform info
- Integrates with Unity logging

## Performance Monitoring
- **Transactions**: Full operation timing
- **Spans**: Sub-operation breakdown
- **Slow queries**: Database bottlenecks
- **Web vitals**: Frontend metrics

## Troubleshooting
| Issue | Solution |
|-------|----------|
| Events not appearing | Check DSN configuration |
| Missing stack traces | Enable debug symbols |
| Too many events | Adjust sample rate |
| Sensitive data leaked | Configure data scrubbing |
