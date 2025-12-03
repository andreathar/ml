# Tasks: Implement Secure Environment Variable Management

**Status**: IN PROGRESS
**Started**: 2025-11-27

## 1. Update .env.example Template

- [x] 1.1 Add OpenAI configuration section:
  ```
  OPENAI_API_KEY=sk-your-openai-api-key-here
  OPENAI_ORG_ID=org-optional-organization-id
  ```

- [x] 1.2 Add optional AI service sections:
  ```
  GEMINI_API_KEY=your-gemini-api-key-here
  ELEVENLABS_API_KEY=your-elevenlabs-key-here
  AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
  AZURE_OPENAI_KEY=your-azure-openai-key-here
  ```

- [x] 1.3 Add security documentation comments explaining:
  - Never commit `.env` file
  - How to generate each API key (with URLs)
  - Which keys are required vs optional
  - Cost information for pay-per-use services

- [x] 1.4 Validate .gitignore includes all sensitive patterns:
  - `.env`, `.env.*` (except `.env.example`)
  - `.env.local`, `.env.*.local`
  - `*.secret`, `*secret*`
  - `*credentials*`, `*.pem`, `*.key`
  - `SecureProjectSettings.asset`

## 2. Create Environment Variable Loader

- [x] 2.1 Create `Assets/Editor/Config/EnvLoader.cs`:
  - `[InitializeOnLoad]` attribute for editor startup
  - Parse `.env` file format (KEY=VALUE, comments, quotes)
  - Store in static dictionary
  - Log warning if `.env` not found (with helpful copy command)
  - Menu item: `Tools/MLCreator/Reload Environment Variables`

- [x] 2.2 Create `Assets/Plugins/GameCreator_Multiplayer/Runtime/Config/EnvConfig.cs`:
  - Static `Get(string key)` method
  - Static `Get(string key, string defaultValue)` method
  - Static `Has(string key)` method
  - Falls back to `System.Environment.GetEnvironmentVariable`
  - `Keys` static class with common key constants

- [x] 2.3 Add validation methods:
  - `ValidateRequiredKeys(params string[] keys)` - returns list of issues
  - `IsPlaceholder(string value)` - detects "your-key-here" patterns
  - `WarnIfPlaceholder(string key)` - logs warning for dev keys
  - `ValidateKey(string key)` - single key validation with logging

- [ ] 2.4 Add editor window for key management:
  - Menu item: `GameCreator/MLcreator/Environment Configuration`
  - Shows all defined keys with masked values
  - Button to open `.env` file in editor
  - Status indicators (missing, placeholder, valid)
  - **SKIPPED**: Not essential for MVP, can add later

## 3. Create Secure Build Settings (Optional)

- [ ] 3.1 Create `SecureProjectSettings` ScriptableObject:
  - Located at `Assets/Settings/SecureProjectSettings.asset`
  - Gitignored (add pattern to .gitignore)
  - Stores encrypted versions of API keys for builds
  - **DEFERRED**: Not needed for initial implementation

- [ ] 3.2 Implement encryption:
  - Use AES-256 with machine-specific key derivation
  - Or use Unity's PlayerPrefs obfuscation
  - Keys decrypted only at runtime
  - **DEFERRED**: Not needed for initial implementation

- [ ] 3.3 Add build preprocessing:
  - `IPreprocessBuildWithReport` implementation
  - Validates required keys exist before build
  - Optionally embeds encrypted keys in build
  - **DEFERRED**: Not needed for initial implementation

## 4. Migrate Icon Generator

- [x] 4.1 Update `IconGeneratorWindow.cs`:
  - Now uses `EnvLoader.Get("OPENAI_API_KEY")` as primary source
  - Falls back to `EditorPrefs` for legacy compatibility
  - Auto-migrates: Deletes old EditorPrefs key when .env is configured
  - Shows status message indicating configuration source

- [x] 4.2 Update `ImageGenService.cs`:
  - Kept constructor parameter for flexibility
  - `IconGeneratorWindow` now passes key from EnvLoader

- [x] 4.3 Add validation in OnEnable:
  - Checks if key exists in .env
  - Checks if key is a placeholder using `IsPlaceholder()`
  - Shows info box when using .env (green)
  - Shows warning box with tip when using manual entry (yellow)

## 5. Create AI Toolbox Bridge (Optional)

- [ ] 5.1 Research AI Toolbox settings system
- [ ] 5.2 Create bridge to sync EnvConfig → AI Toolbox settings
- [ ] 5.3 Document integration approach
- **DEFERRED**: Can be added when AI Toolbox integration is needed

## 6. Add Security Validation

- [ ] 6.1 Create pre-commit hook script:
  - Scan staged files for API key patterns
  - Block commit if secrets detected
  - Located at `.githooks/pre-commit`
  - **DEFERRED**: Can add later for extra security

- [ ] 6.2 Add patterns to detect:
  - `sk-[a-zA-Z0-9]{48}` (OpenAI)
  - `eyJ[a-zA-Z0-9_-]+\.eyJ` (JWT/Supabase)
  - Generic patterns: `api_key\s*=\s*['"][^'"]+['"]`
  - **DEFERRED**: Can add later for extra security

- [ ] 6.3 Create setup script:
  - `scripts/setup-git-hooks.ps1`
  - Configures git to use `.githooks` directory
  - **DEFERRED**: Can add later for extra security

## 7. Documentation

- [x] 7.1 Update .env.example with inline documentation:
  - Security notice at top
  - URLs for generating each API key
  - Cost information
  - Required vs optional indicators

- [ ] 7.2 Create `docs/ENVIRONMENT_SETUP.md`:
  - Step-by-step for new developers
  - How to get each API key
  - Troubleshooting common issues
  - **DEFERRED**: Can add when onboarding new developers

- [x] 7.3 Add inline documentation:
  - XML docs on all EnvConfig methods
  - XML docs on all EnvLoader methods
  - Comments in .env.example

## Completion Checklist

- [x] `.env.example` updated with all required variables
- [x] `EnvConfig.cs` created with full API
- [x] `EnvLoader.cs` created for editor with `[InitializeOnLoad]`
- [x] Icon Generator migrated to EnvConfig (with legacy fallback)
- [x] .gitignore updated with comprehensive security patterns
- [x] Inline documentation complete
- [x] No secrets in repository

## Summary of Changes

### Files Created
1. `Assets/Editor/Config/EnvLoader.cs` - Editor-only environment loader
2. `Assets/Plugins/GameCreator_Multiplayer/Runtime/Config/EnvConfig.cs` - Runtime config API

### Files Modified
1. `.env.example` - Added AI service keys, security docs, cost info
2. `.gitignore` - Added comprehensive secret patterns
3. `Assets/Editor/Tools/IconGenerator/IconGeneratorWindow.cs` - Migrated to EnvLoader

### Deferred Items (Not Essential for MVP)
- Editor window for key management (task 2.4)
- Secure build settings with encryption (task 3.x)
- AI Toolbox bridge (task 5.x)
- Pre-commit hooks (task 6.x)
- Standalone documentation file (task 7.2)
