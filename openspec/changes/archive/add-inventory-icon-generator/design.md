# Design: Inventory Icon Generator

## Context
The project aims for 10x developer efficiency. Asset creation, particularly UI icons for inventory items, is a labor-intensive process. We need a pipeline to convert "Item Name" -> "Game Ready Icon" automatically.

## Goals
- Input: List of strings (e.g., "Iron Sword", "Health Potion").
- Output: 512x512 PNG icons saved to `Assets/Textures/Icons/`.
- Style consistency: Allow defining a global style prompt (e.g., "Fantasy RPG style, flat vector icon, transparent background").
- Automation: One-click batch generation.

## Architecture
1.  **GeneratorWindow**: EditorWindow for input/output configuration.
2.  **ImageGenService**: Service layer handling API calls to external AI (OpenAI DALL-E 3 or similar).
3.  **IconProcessor**: Handles Texture2D manipulation (compression, resizing) and file I/O.
4.  **GameCreatorBridge**: Optional module to creating `Item` assets and assigning the sprite.

## Decisions
- **API**: Use OpenAI DALL-E 3 initially for high quality, or Local Stable Diffusion if configured. (Will make the service swappable).
- **Storage**: Save raw images to `Assets/Art/Generated/Icons` first, then process to `Assets/Resources/Icons`.
- **Async**: Generation must be asynchronous to avoid freezing the Editor. Use `async/await` with `EditorApplication.delayCall` for main thread Unity API access.

## Risks
- **Cost**: API costs for large batches. Mitigation: Add estimated cost calculator before run.
- **Consistency**: AI might generate varying styles. Mitigation: Robust prompt engineering templates.
- **Rate Limiting**: API limits. Mitigation: Implement queue with delays.

