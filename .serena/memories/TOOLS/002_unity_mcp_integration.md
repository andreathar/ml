# Unity MCP Integration Workflow

**Category:** TOOLS - Unity Automation
**Last Updated:** 2025-11-23
**MCP Server:** unity-mcp (configured in `.mcp.json`)

## Unity MCP Overview

Unity MCP Server enables AI assistants to interact with Unity Editor programmatically:
- Scene manipulation
- Asset operations
- GameObject creation and modification
- Component management

## Configuration

**Location:** `.mcp.json`

```json
{
  "mcpServers": {
    "unity": {
      "command": "D:\\UnityWorkspaces\\MLcreator\\Library\\mcp-server\\win-x64\\unity-mcp-server.exe",
      "args": ["--client-transport", "stdio", "--port", "8081"]
    }
  }
}
```

## Common Unity MCP Operations

### Scene Operations

**Create GameObject:**
```
Unity MCP: Create new GameObject "PlayerSpawnPoint" with Transform component
```

**Add Component:**
```
Unity MCP: Add NetworkObject component to GameObject "Player_Network"
```

**Modify Transform:**
```
Unity MCP: Set position of "PlayerSpawnPoint" to (0, 0, 0)
```

### Prefab Operations

**Create Prefab:**
```
Unity MCP: Save GameObject "Player_Network" as prefab at Assets/Prefabs/Player_Network.prefab
```

**Instantiate Prefab:**
```
Unity MCP: Instantiate prefab Assets/Prefabs/Enemy.prefab in current scene
```

### Asset Management

**Create ScriptableObject:**
```
Unity MCP: Create ScriptableObject of type NetworkConfig at Assets/Config/NetworkSettings.asset
```

**Find Assets:**
```
Unity MCP: Find all prefabs in Assets/Prefabs/Characters/ directory
```

## Unity MCP + GameCreator Patterns

### Creating Character Prefab

```
1. Unity MCP: Create GameObject "NetworkCharacter"
2. Unity MCP: Add Character component
3. Unity MCP: Add CharacterController component
4. Unity MCP: Add NetworkObject component
5. Unity MCP: Add NetworkCharacterAdapter component
6. Unity MCP: Set IsNetworkSpawned to true (requires code)
7. Unity MCP: Save as prefab Assets/Prefabs/NetworkCharacter.prefab
```

### Setting Up NetworkManager

```
Unity MCP: Create GameObject "NetworkManager"
Unity MCP: Add NetworkManager component
Unity MCP: Set NetworkManager prefabs list to include Player_Network.prefab
```

## Workflow Integration

### With OpenSpec Changes

When implementing OpenSpec proposals that require Unity assets:

1. Read the OpenSpec proposal
2. Identify required Unity assets/prefabs
3. Use Unity MCP to create/modify assets
4. Validate changes in Unity Editor
5. Update OpenSpec tasks.md with completion status

**Example:**
```
OpenSpec task: "Create network player prefab"

→ Unity MCP: Create GameObject "Player_Network"
→ Unity MCP: Add components (Character, NetworkObject, etc.)
→ Unity MCP: Configure network settings
→ Unity MCP: Save prefab
→ Mark task complete in tasks.md
```

### With Slash Commands

Slash commands generate code, Unity MCP creates assets:

```
1. /gc:multiplayer-component PlayerSync
   → Generates PlayerSync.cs code

2. Unity MCP: Create GameObject "NetworkPlayer"
   → Creates GameObject in scene

3. Unity MCP: Add PlayerSync component to NetworkPlayer
   → Attaches generated script

4. Unity MCP: Configure component properties
   → Sets up initial values
```

## Troubleshooting

### Unity MCP Connection Failed

**Status from user:** "Failed to reconnect to unity-mcp"

**Possible causes:**
- Unity Editor not running
- MCP server not started
- Port 8081 in use
- Incorrect path in `.mcp.json`

**Solutions:**
```powershell
# Check if Unity Editor is running
Get-Process -Name "Unity" -ErrorAction SilentlyContinue

# Check if port 8081 is available
netstat -ano | findstr ":8081"

# Restart Unity MCP server
# (Usually automatic when Unity Editor starts)
```

### Component Not Found

If Unity MCP can't find a component:
- Verify component exists in project
- Check namespace and assembly references
- Ensure component is in correct assembly definition

### Prefab Modifications Not Saving

```
Unity MCP: Apply changes to prefab
Unity MCP: Force save assets
```

## Best Practices

### 1. Validate Before Operations

```
Before: Unity MCP: Delete GameObject "Important"
Do: Unity MCP: Check if GameObject "Important" has dependencies
Then: Proceed or abort based on validation
```

### 2. Batch Operations

```
# ❌ Inefficient
Unity MCP: Add NetworkObject to Player1
Unity MCP: Add NetworkObject to Player2
Unity MCP: Add NetworkObject to Player3

# ✅ Efficient
Unity MCP: Add NetworkObject component to all GameObjects with tag "Player"
```

### 3. Preserve Manual Changes

Unity MCP operations should not overwrite manual Editor changes:
- Check if prefab has manual overrides before modifying
- Use prefab variants for customization
- Document which assets are MCP-managed vs manually managed

## Limitations

**Unity MCP cannot:**
- Execute Play mode operations (runtime testing)
- Modify Editor preferences/settings extensively
- Handle complex visual/material editing
- Replace manual level design work

**For these, use:**
- Manual Unity Editor interaction
- UnityTest framework for runtime testing
- Visual editing tools in Editor

## Related Documentation

- `.mcp.json` - MCP server configuration
- `Library/mcp-server/` - Unity MCP server binaries
- `.serena/memories/TOOLS/003_openspec_workflow.md` - OpenSpec integration

## Quick Reference

| Task | Unity MCP Command Pattern |
|------|--------------------------|
| Create GameObject | `Create GameObject "Name"` |
| Add component | `Add [Component] to "GameObject"` |
| Save prefab | `Save "GameObject" as prefab at path` |
| Find assets | `Find [asset type] in [directory]` |
| Modify property | `Set [property] of "GameObject" to [value]` |
