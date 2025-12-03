# MCP Server Inventory

**Priority:** INTEGRATION
**Last Updated:** 2025-12-02
**Applies To:** All AI assistants (Claude Code, Serena, Antigravity)

## Active MCP Servers

### 1. Unity-MCP (Primary Unity Integration)

| Property | Value |
|----------|-------|
| **Name** | Unity-MCP |
| **Type** | stdio |
| **Executable** | `Library/mcp-server/win-x64/unity-mcp-server.exe` |
| **Port** | 8081 |
| **Plugin Timeout** | 20000ms |
| **Status** | Active |

**Capabilities:**
- Unity Editor integration
- Scene management
- Asset operations
- Script compilation status
- Play mode control

**Configuration Location:**
- `.mcp.json` (Claude Code)
- `.serena/config.yaml` (Serena)

### 2. filesystem-ml (File System Access)

| Property | Value |
|----------|-------|
| **Name** | filesystem-ml |
| **Type** | node |
| **Package** | `@modelcontextprotocol/server-filesystem` |
| **Scope** | `D:\UnityWorkspaces\ml` |
| **Status** | Active |

**Capabilities:**
- Read/write files
- List directories
- Create directories
- Search files
- File metadata

**Configuration Location:**
- `.mcp.json` (Claude Code)

### 3. Unity Knowledge Base (Qdrant)

| Property | Value |
|----------|-------|
| **Name** | unity-knowledge-base |
| **Type** | python |
| **Script** | `scripts/unity-kb/direct_mcp_server.py` |
| **Backend** | Qdrant (localhost:6333) |
| **Collection** | `unity_project_kb` |
| **Symbols** | 77,914+ |
| **Status** | Available (requires Qdrant running) |

**Capabilities:**
- Semantic code search
- Symbol lookup
- Pattern detection
- Conflict identification
- Documentation retrieval

**Configuration Location:**
- `.serena/config.yaml` (Serena)

**Health Check:**
```bash
curl -s http://localhost:6333/healthz
curl -s http://localhost:6333/collections/unity_project_kb
```

## Configuration Files

### Primary: `.mcp.json` (Claude Code)

```json
{
  "mcpServers": {
    "filesystem-ml": { ... },
    "Unity-MCP": { ... }
  }
}
```

### Secondary: `.serena/config.yaml` (Serena)

```yaml
mcp:
  enabled: true
  servers:
    unity: { ... }
    unity-knowledge-base: { ... }
    filesystem-mlcreator: { ... }
    memory: { ... }
    sequential-thinking: { ... }
    serena: { ... }
```

### Permissions: `.claude/settings.local.json`

```json
{
  "permissions": {
    "allow": [
      "mcp__filesystem-ml__*",
      "mcp__Unity-MCP__*"
    ]
  }
}
```

## Startup Order

1. **Qdrant** (external) - Must be running for KB queries
2. **Unity Editor** - Required for Unity-MCP
3. **Unity-MCP** - Auto-started by Claude Code
4. **filesystem-ml** - Auto-started by Claude Code
5. **Serena MCP servers** - Started on Serena activation

## Health Monitoring

### Quick Health Check Script

```powershell
# Check Qdrant
$qdrantHealth = Invoke-RestMethod -Uri "http://localhost:6333/healthz" -ErrorAction SilentlyContinue
if ($qdrantHealth) { Write-Host "Qdrant: OK" -ForegroundColor Green }
else { Write-Host "Qdrant: OFFLINE" -ForegroundColor Red }

# Check Unity MCP (via file existence)
$unityMcp = Test-Path "Library/mcp-server/win-x64/unity-mcp-server.exe"
if ($unityMcp) { Write-Host "Unity-MCP: Available" -ForegroundColor Green }
else { Write-Host "Unity-MCP: NOT FOUND" -ForegroundColor Red }

# Check KB collection
$kbStats = Invoke-RestMethod -Uri "http://localhost:6333/collections/unity_project_kb" -ErrorAction SilentlyContinue
if ($kbStats.result.points_count -gt 70000) { 
    Write-Host "KB Symbols: $($kbStats.result.points_count)" -ForegroundColor Green 
}
else { Write-Host "KB: Incomplete or missing" -ForegroundColor Yellow }
```

## Troubleshooting

### Unity-MCP Not Responding

1. Check Unity Editor is running
2. Verify port 8081 is not blocked
3. Restart Unity Editor
4. Check `Library/mcp-server/` exists

### Qdrant Not Available

1. Start Qdrant: `docker start qdrant` or run local binary
2. Verify: `curl http://localhost:6333/healthz`
3. Check collection: `curl http://localhost:6333/collections/unity_project_kb`

### KB Queries Failing

1. Verify Qdrant is running
2. Check symbol count (should be 77,914+)
3. Review `.serena/How_to_make_queries_to_KB_and_take_decisions.md`

## Related Documentation

- `.serena/config.yaml` - Full Serena MCP configuration
- `.serena/How_to_make_queries_to_KB_and_take_decisions.md` - KB query guide
- `openspec/unity-kb/` - KB utility scripts
