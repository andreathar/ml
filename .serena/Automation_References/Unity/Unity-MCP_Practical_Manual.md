# Unity-MCP Practical Manual

## Table of Contents
1. [Overview](#overview)
2. [Installation and Setup](#installation-and-setup)
3. [Available Tools](#available-tools)
4. [How to Use Unity-MCP](#how-to-use-unity-mcp)
5. [Common Use Cases and Examples](#common-use-cases-and-examples)
6. [Creating Custom Tools](#creating-custom-tools)
7. [Troubleshooting](#troubleshooting)
8. [Best Practices](#best-practices)
9. [Architecture Overview](#architecture-overview)

---

## Overview

### What is Unity-MCP?

Unity-MCP is an AI helper that bridges Large Language Models (LLMs) with the Unity Editor and Unity games. It enables AI to perform a wide range of tasks within the Unity environment by acting as a communication layer that exposes Unity's tools and API to LLMs.

### Main Features

**For Humans:**
- Chat with AI as you would with a human
- Supports both local and remote usage
- Supports `STDIO` and `HTTP` protocols for communication
- Wide range of default AI tools
- Customizable reflection converters for custom project data
- Remote AI unit setups using Docker containers for team collaboration

**For LLMs:**
- Agent-ready tools for efficient tool discovery
- Instant C# code compilation and execution using Roslyn
- Access (read/write) to assets and C# scripts
- Detailed positive and negative feedback for issue understanding
- Full access to project data in readable format
- Modification of granular data using Reflection
- Ability to find and call any method in the codebase (including DLLs)
- Access to Unity API with human-readable descriptions

---

## Installation and Setup

### Requirements

- Unity project path **must not contain spaces** (critical limitation)
- MCP Client that supports dynamic tool updates (Claude Code, Claude Desktop, GitHub Copilot in VS Code, Cursor, or Windsurf)
- Unity Editor

### Step 1: Install Unity Plugin

Choose one of the following methods:

#### Option A: Unity Package (Recommended)
1. Download the installer from [GitHub releases](https://github.com/IvanMurzak/Unity-MCP/releases)
2. Import `AI-Game-Dev-Installer.unitypackage` into your Unity project:
   - Double-click the file, OR
   - In Unity: `Assets → Import Package → Custom Package` → select the file

#### Option B: OpenUPM
1. Install OpenUPM-CLI
2. Open command line in the Unity project folder
3. Run: `openupm add com.ivanmurzak.unity.mcp`

### Step 2: Configure MCP Client

#### Automatic Configuration (Recommended)
1. Open your Unity project
2. Go to `Window → AI Connector (Unity-MCP)`
3. Click `Configure` for your preferred MCP client

#### Manual Configuration
If automatic configuration fails, use the JSON provided in the AI Connector window.

**Example for Claude Code (Windows):**
```bash
claude mcp add Unity-MCP "<unityProjectPath>/Library/mcp-server/win-x64/unity-mcp-server.exe" client-transport=stdio
```

Replace `<unityProjectPath>` with your actual project path.

### Step 3: Enable Agent Mode

**Important:** Ensure "Agent" mode is turned on in your MCP client for proper tool utilization.

---

## Available Tools

Unity-MCP provides a comprehensive set of tools organized by functionality:

### 1. Script Operations
- Read and write C# code
- Compile code using Roslyn
- Execute C# scripts

### 2. Asset Management
- Create, modify, and organize Unity assets
- Material creation and manipulation
- Asset import and export

### 3. Scene Manipulation
- GameObject creation
- Component modification
- Hierarchy management

### 4. Test Integration
- Remote execution of Unity Test Runner tests
- Test result collection
- Filter tests by mode, assembly, namespace, class, and method

### 5. Reflection System
- Dynamic access to Unity APIs
- Custom type converters

### Example Tools

**Editor_Selection_Get**
- Retrieves information about current selection in Unity Editor
- Returns data about selected assets or GameObjects

**Editor_Selection_Set**
- Sets the selection in Unity Editor using instance IDs

**Assets_Prefab_Save**
- Saves changes to a prefab when in prefab editing mode

**TestRunner_Run**
- Initiates Unity tests with parameters:
  - `testMode`: PlayMode or EditMode
  - `testAssembly`: Specific assembly to test
  - `testNamespace`: Namespace filter
  - `testClass`: Class filter
  - `testMethod`: Method filter

---

## How to Use Unity-MCP

### Basic Workflow

1. **Open Unity Editor** with your project
2. **Open AI Connector Window**: `Window → AI Connector (Unity-MCP)`
3. **Verify Connection Status**: Check that it shows "Connected"
4. **Open Your MCP Client** (Claude Code, Cursor, etc.)
5. **Ensure Agent Mode is Enabled**
6. **Give Natural Language Commands** to the AI

### Natural Language Examples

You can interact with Unity-MCP using natural language commands:

```
"Explain my scene hierarchy"
"Create 3 cubes in a circle with radius 2"
"Create metallic golden material and attach it to a sphere gameObject"
"Run all PlayMode tests in the PlayerController namespace"
"List all GameObjects with Rigidbody components"
"Save the current prefab"
```

The MCP client translates these commands into tool calls that Unity-MCP executes.

---

## Common Use Cases and Examples

### 1. AI-Assisted Content Creation

**Use Case:** Generate and modify game assets using AI.

**Examples:**
```
"Create 5 spheres arranged in a pentagon with radius 3"
"Generate a simple platform prefab with a BoxCollider"
"Create a particle system for a fire effect"
```

### 2. Automated Scene Management

**Use Case:** Manipulate scene hierarchies and game objects.

**Examples:**
```
"Explain the hierarchy of my current scene"
"Find all inactive GameObjects in the scene"
"Organize all UI elements under a Canvas parent"
"Save the current GameObject as a prefab named 'EnemySpawner'"
```

### 3. Material and Visual Customization

**Use Case:** Create and assign materials with specific properties.

**Examples:**
```
"Create metallic golden material and attach it to a sphere GameObject"
"Apply a red transparent material to all cubes in the scene"
"Create a PBR material with roughness 0.3 and metallic 0.8"
```

### 4. Automated Testing

**Use Case:** Run Unity tests and analyze results.

**Examples:**
```
"Run all PlayMode tests"
"Execute tests in the Combat.Tests namespace"
"Run the PlayerMovementTest class tests"
"Filter and run only tests containing 'Jump' in their name"
```

### 5. Rapid Prototyping

**Use Case:** Quickly test ideas and automate repetitive tasks.

**Examples:**
```
"Create a simple player controller script"
"Set up a basic scene with ground, player, and lighting"
"Generate 10 random obstacles in the scene"
```

### 6. Code Analysis and Refactoring

**Use Case:** Analyze and improve existing code.

**Examples:**
```
"Explain what the PlayerController script does"
"Find all scripts that use the old InputManager"
"List all public methods in the GameManager class"
```

---

## Creating Custom Tools

You can extend Unity-MCP by creating custom tools tailored to your project's needs.

### Step-by-Step Guide

#### 1. Create a Tool Class

Add the `[McpPluginToolType]` attribute to your class:

```csharp
using IvanMurzak.Unity.MCP.Plugin;
using System.ComponentModel;

[McpPluginToolType]
public class Tool_CustomOperations
{
    // Your custom tools will go here
}
```

#### 2. Define a Tool Method

Add a method with the `[McpPluginTool]` attribute:

```csharp
[McpPluginTool
(
    "CreateCustomLevel",
    Title = "Create a custom level with specific parameters"
)]
[Description("Creates a procedurally generated level based on provided parameters.")]
public string CreateCustomLevel
(
    [Description("Width of the level in units")]
    int width,

    [Description("Height of the level in units")]
    int height,

    [Description("Optional difficulty setting (1-10). Default is 5.")]
    int? difficulty = null
)
{
    // Background thread work (if any)
    int actualDifficulty = difficulty ?? 5;

    // Main thread work (Unity API calls)
    return MainThread.Instance.Run(() =>
    {
        // Create GameObjects, modify scene, etc.
        GameObject levelParent = new GameObject($"Level_{width}x{height}");

        // Your level generation logic here...

        return $"[Success] Created level with dimensions {width}x{height} and difficulty {actualDifficulty}.";
    });
}
```

#### 3. Key Points for Custom Tools

**Attributes:**
- `[McpPluginToolType]`: Marks the class as containing MCP tools
- `[McpPluginTool]`: Marks the method as an MCP tool
- `[Description]`: Provides context for the LLM (on methods and parameters)

**Optional Parameters:**
- Use nullable types like `string? optional = null` to mark parameters as optional

**Threading:**
- Non-Unity API code can run on background threads
- Unity API calls must run on the main thread using `MainThread.Instance.Run(() => { ... })`

**Return Values:**
- Return descriptive strings indicating success or failure
- Include relevant data in the response for the AI to understand

### Example: Advanced Custom Tool

```csharp
[McpPluginToolType]
public class Tool_GameObject
{
    [McpPluginTool
    (
        "FindObjectsByTag",
        Title = "Find all GameObjects with a specific tag"
    )]
    [Description("Searches the scene for all GameObjects with the specified tag and returns their names and positions.")]
    public string FindObjectsByTag
    (
        [Description("The tag to search for (e.g., 'Player', 'Enemy', 'Collectible')")]
        string tag
    )
    {
        return MainThread.Instance.Run(() =>
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

            if (objects.Length == 0)
            {
                return $"[Info] No GameObjects found with tag '{tag}'.";
            }

            StringBuilder result = new StringBuilder();
            result.AppendLine($"[Success] Found {objects.Length} GameObject(s) with tag '{tag}':");

            foreach (GameObject obj in objects)
            {
                Vector3 pos = obj.transform.position;
                result.AppendLine($"- {obj.name} at position ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
            }

            return result.ToString();
        });
    }
}
```

### Limitations

- **In-Game Tools**: Adding custom in-game tools is not yet supported (work in progress)
- Custom tools work in the Unity Editor context

---

## Troubleshooting

### Connection Issues

#### Problem: Server not found
**Solutions:**
- Ensure `unity-mcp-server` dotnet tool is installed globally
- Verify the tool is accessible in your system's PATH
- Restart Unity to trigger server download to `Library/mcp-server/{platform}/`

#### Problem: Connection timeout
**Solutions:**
- Verify Unity Editor is running
- Check Unity-MCP Plugin is correctly installed
- Adjust timeout in AI Connector window or via environment variable:
  - Environment: `UNITY_MCP_PLUGIN_TIMEOUT` (in milliseconds)
  - Command-line: `--plugin-timeout` argument
- Default timeout can be changed in the AI Connector window

#### Problem: Port conflicts (Port 8080 already in use)
**Solutions:**
- Change port using command-line: `--port`
- Or use environment variable: `UNITY_MCP_PORT`

#### Problem: Project path contains spaces
**Solution:**
- This is a critical limitation - Unity-MCP does not support project paths with spaces
- Move your project to a path without spaces

### Configuration Issues

#### Problem: Client not configured
**Solution:**
- Use AI Connector window: `Window → AI Connector (Unity-MCP)`
- Click `Configure` for automatic setup
- If automatic fails, use manual configuration with JSON from the window

#### Problem: Tools not available to AI
**Solution:**
- Ensure "Agent" mode is enabled in your MCP client
- Verify connection status shows "Connected" in AI Connector window

### Debugging

#### Enable Debug Output
Run `unity-mcp-server` manually with specific arguments to see debug output.

#### Configure Log Levels
In AI Connector window, adjust log levels:
- `Error`: Only critical errors
- `Warning`: Errors and warnings
- `Info`: General information
- `Debug`: Detailed debugging info
- `Trace`: Maximum detail

#### Use MCP Inspector Tool
For debugging MCP protocol communication:
```bash
npx @modelcontextprotocol/inspector
```

---

## Best Practices

### Installation and Setup

1. **Always use AI Connector Window** for configuration (automatic is better than manual)
2. **Verify connection status** before starting work
3. **Keep Unity Editor running** while using MCP clients
4. **Ensure project paths have no spaces** to avoid issues

### Development Workflow

1. **Enable Agent Mode** in your MCP client before issuing commands
2. **Be specific in your requests** - the more details you provide, the better the AI can help
3. **Test incrementally** - start with simple commands before complex operations
4. **Monitor connection status** in the AI Connector window

### Version Management (For Developers)

1. **Use `bump-version.ps1` script** for version updates
2. **Preview changes** with `-WhatIf` before applying
3. **Clean working directory** before version bump (commit or stash changes)
4. **Follow semantic versioning** (`major.minor.patch`)
5. **Test after bump** - verify project builds and functions correctly
6. **Commit immediately** after version bump

### Performance

1. **Keep Unity Editor responsive** - don't overload with too many simultaneous requests
2. **Use specific tool filters** (e.g., for tests) to avoid unnecessary processing
3. **Close unused scenes** to reduce memory usage

### Security

1. **Be cautious with code execution tools** - review generated code before execution
2. **Don't expose sensitive data** through custom tools
3. **Test custom tools thoroughly** before deploying to team

---

## Architecture Overview

Understanding the architecture helps with troubleshooting and advanced usage.

### Three-Tier Architecture

```
AI Clients (Tier 1)
    ↓ MCP Protocol (HTTP/STDIO)
Unity-MCP-Server (Tier 2)
    ↓ SignalR (Port 8080)
Unity-MCP-Plugin (Tier 3)
```

#### Tier 1: AI Clients
- External applications like Claude Code, Cursor, VS Code
- Communicate using Model Context Protocol (MCP)
- Send tool call requests to the server

#### Tier 2: Unity-MCP-Server
- ASP.NET Core application
- Central orchestration layer
- Routes tool calls between AI clients and Unity plugin
- Manages request tracking
- Components:
  - `Program.Main`: Entry point
  - `IMcpServer`: MCP protocol handler
  - `RemoteApp Hub`: SignalR hub for Unity communication
  - `ToolRouter`: Routes tool calls
  - `RequestTrackingService`: Tracks async requests

#### Tier 3: Unity-MCP-Plugin
- Unity Editor integration
- Executes AI-requested operations within Unity
- Components:
  - `McpPluginUnity`: Main plugin component
  - `RpcRouter`: RPC communication handler
  - `McpRunner`: Tool execution engine
  - `ToolRunnerCollection`: Collection of available tools

### Communication Flow

1. **AI Client** sends `CallTool` request via MCP
2. **MCP Server** receives and routes to `ToolRouter`
3. **RequestTrackingService** creates a pending request
4. **RemoteApp Hub** forwards via SignalR to Unity plugin
5. **RpcRouter** in plugin receives the request
6. **McpRunner** executes the tool on Unity Main Thread
7. Response flows back through the same chain
8. **AI Client** receives the result

### Key Technologies

- **Model Context Protocol (MCP)**: Communication protocol between AI and tools
- **SignalR**: Real-time communication between server and Unity plugin
- **Roslyn**: C# code compilation and execution
- **Reflection**: Dynamic access to Unity APIs and project code
- **ASP.NET Core**: Server framework

---

## Quick Reference

### Essential Commands (in AI Client)

```
"Explain my scene hierarchy"
"Create [object] at position [x, y, z]"
"Run tests in [namespace/class]"
"Save prefab"
"List all [components] in scene"
"Create material with [properties]"
```

### Important Paths

- **Server Binary**: `<ProjectPath>/Library/mcp-server/{platform}/unity-mcp-server.exe`
- **Plugin Location**: Installed via Unity Package Manager
- **AI Connector Window**: `Window → AI Connector (Unity-MCP)`

### Environment Variables

- `UNITY_MCP_PLUGIN_TIMEOUT`: Connection timeout in milliseconds
- `UNITY_MCP_PORT`: SignalR communication port (default: 8080)

### Common Attributes for Custom Tools

```csharp
[McpPluginToolType]           // On class
[McpPluginTool("ToolName")]   // On method
[Description("...")]          // On methods and parameters
```

---

## Additional Resources

- **GitHub Repository**: https://github.com/IvanMurzak/Unity-MCP
- **Releases**: https://github.com/IvanMurzak/Unity-MCP/releases
- **Issues and Support**: https://github.com/IvanMurzak/Unity-MCP/issues

---

## Glossary

- **MCP**: Model Context Protocol - standard for AI-tool communication
- **LLM**: Large Language Model - AI system that processes natural language
- **SignalR**: Library for real-time web communication
- **Roslyn**: .NET compiler platform for C#
- **Reflection**: Runtime inspection and manipulation of code
- **Agent Mode**: MCP client mode that enables tool usage
- **Main Thread**: Unity's primary execution thread (required for Unity API calls)

---

**Document Version**: 1.0
**Last Updated**: October 2025
**Based on**: Unity-MCP Repository Documentation via DeepWiki
