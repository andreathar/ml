# Change: Implement Secure Environment Variable Management

## Why

The project currently has scattered API key management across multiple systems:
- `EditorPrefs` for Icon Generator OpenAI key (stored in registry, not portable)
- AI Toolbox has its own settings system with encryption options
- `.env.example` exists but no unified loader for Unity
- No runtime access to environment variables in builds
- Risk of accidentally committing secrets

A unified, secure environment variable system will:
1. **Centralize secrets** - Single source of truth for all API keys
2. **Improve security** - Proper encryption and no registry storage
3. **Enable portability** - Team members can share `.env.example`, use their own `.env`
4. **Support CI/CD** - Environment variables work in build pipelines
5. **Prevent leaks** - Runtime validation and warnings for missing/exposed keys

## What Changes

### 1. Enhanced .env.example Template
Update with all required variables across the project:
- OpenAI API Key (Icon Generator, AI Toolbox)
- Supabase credentials (Backend integration)
- Unity MCP configuration
- Future: Gemini, Azure, other AI services

### 2. Environment Variable Loader (Editor + Runtime)
Create `EnvLoader.cs` that:
- Loads `.env` file at editor startup
- Provides `EnvConfig.Get("KEY")` API for all scripts
- Falls back to `System.Environment.GetEnvironmentVariable` for CI/CD
- Caches values in memory (never persists secrets)

### 3. Secure Settings ScriptableObject
Create `SecureProjectSettings.asset` that:
- Stores encrypted API keys for builds (optional)
- Uses Unity's PlayerPrefs encryption or custom AES
- Provides inspector UI for key entry
- Never commits actual values (gitignored)

### 4. Update Existing Code
- Icon Generator: Use `EnvConfig.Get("OPENAI_API_KEY")` instead of EditorPrefs
- AI Toolbox integration: Bridge to EnvConfig
- Supabase setup: Read from environment

### 5. Security Validation
- Pre-commit hook to detect secrets in code
- Editor warning if `.env` contains placeholder values
- Runtime check for exposed keys in builds

## Current State Analysis

### Existing Environment Variables (.env.example)
```
GITHUB_COPILOT_TOKEN     # MCP authentication
SUPABASE_URL             # Backend
SUPABASE_ANON_KEY        # Backend
UNITY_MCP_PORT           # MCP server
UNITY_MCP_PLUGIN_TIMEOUT # MCP server
```

### Missing Variables (to be added)
```
OPENAI_API_KEY           # Icon Generator, AI Toolbox
GEMINI_API_KEY           # AI Toolbox (optional)
AZURE_OPENAI_ENDPOINT    # Enterprise AI (optional)
AZURE_OPENAI_KEY         # Enterprise AI (optional)
ELEVENLABS_API_KEY       # Text-to-speech (optional)
```

### Current API Key Storage Locations
| System | Current Storage | Risk Level | Migration |
|--------|-----------------|------------|-----------|
| Icon Generator | EditorPrefs (registry) | Medium | → EnvConfig |
| AI Toolbox | ScriptableObject + encryption | Low | → Bridge to EnvConfig |
| Supabase | Not implemented | N/A | → EnvConfig |
| MCP Config | .env file | Low | Already correct |

## Impact

### Affected Specs
- None directly affected

### Affected Code
- `Assets/Editor/Tools/IconGenerator/ImageGenService.cs`
- `Assets/Editor/Tools/IconGenerator/IconGeneratorWindow.cs`
- New: `Assets/Plugins/GameCreator_Multiplayer/Runtime/Config/EnvConfig.cs`
- New: `Assets/Editor/Config/EnvLoader.cs`

### Breaking Changes
- **Minor**: Icon Generator will need re-entry of API key after migration
- **Mitigation**: One-time migration, display helpful message

### Risk Assessment
- **Low risk**: Additive changes, backward compatible
- **Security improvement**: Reduces attack surface

## Success Criteria

1. **Single source of truth**: All API keys loaded from `.env` or environment
2. **No registry storage**: Remove EditorPrefs usage for secrets
3. **Portable configuration**: New team members can copy `.env.example` → `.env`
4. **CI/CD compatible**: Builds work with environment variables
5. **Validation**: Editor warns about missing or placeholder keys
6. **Documentation**: Clear setup guide for developers
