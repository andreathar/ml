# Change: Add Inventory Icon Generator Tool

## Why
Creating unique icons for hundreds of inventory items is a bottleneck. Manually drawing or searching for assets takes significant time. An automated tool that generates icons from item names using AI would provide a 10x efficiency improvement in asset creation.

## What Changes
- New Unity Editor Window: `Inventory Icon Generator`
- Integration with AI Image Generation API (e.g., DALL-E 3 or Stable Diffusion via MCP/API)
- Batch processing capabilities for list-based generation
- Automated asset saving and optional GameCreator Item creation

## Impact
- Affected specs: `tools/icon-generator`
- Affected code: `Assets/Editor/Tools/IconGenerator/`
- Dependencies: Requires API Key for image generation service

