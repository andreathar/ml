# MLcreator Project Structure

## Key Directories
- `Assets/Plugins/GameCreator_Multiplayer/Runtime/` - Runtime multiplayer code
- `Assets/Plugins/GameCreator_Multiplayer/Editor/` - Editor tooling
- `Assets/Tests/Runtime/` - Runtime tests
- `.serena/memories/` - AI knowledge base (tiered: CRITICAL, CONTEXT, TOOLS, INCIDENTS)
- `.serena/Automation_References/` - Reference documentation
- `openspec/` - Change proposals
- `claudedocs/` - AI-generated documentation

## Tech Stack
- Unity 6000.2.13f1
- C# 9.0+
- Netcode 2.7.0
- GameCreator 2.0+

## Reference Files
- `.serena/ai/critical.llm.txt` - Quick critical rules (~1000 tokens)
- `.serena/symbols/assemblies.json` - Unity assemblies
- `.serena/symbols/gamecreator_modules.json` - GameCreator modules
