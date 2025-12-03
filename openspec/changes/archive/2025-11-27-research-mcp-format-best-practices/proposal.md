# Change: Research MCP Format Best Practices

## Why

MCP (Model Context Protocol) servers have different configuration formats and parameter requirements across different LLM clients (Claude Code, Cursor, Cline, Gemini/Antigravity, etc.) and editors (VSCode, Cursor IDE, etc.). This creates confusion and trial-and-error when setting up MCP servers for different environments.

Currently, there is no consolidated reference documenting:
- Format differences between clients (stdio vs SSE transport, parameter naming conventions)
- Required vs optional parameters per client
- Common pitfalls and troubleshooting patterns
- Best practices for multi-client compatibility

## What Changes

- **NEW**: Comprehensive research document analyzing MCP configuration formats across major LLM clients and editors
- **NEW**: Best practices manual with short, actionable guidelines for each common use case
- **NEW**: Reference examples showing correct configuration for each client type
- **NEW**: Troubleshooting guide for common MCP setup issues

This change does NOT modify any code. It is purely documentation and research to inform future MCP integrations.

## Impact

### Affected Specs
- **NEW capability**: `mcp-documentation` - Documentation and best practices for MCP server configuration

### Affected Documentation
- New documentation in `docs/_knowledge-base/03-api-reference/MCP/`
- Reference material for future MCP server integrations
- Reduces setup friction for AI assistants working with MCP servers

### Benefits
- Faster MCP server setup and troubleshooting
- Reduced configuration errors and compatibility issues
- Single source of truth for MCP format requirements
- Improved multi-client compatibility when configuring MCP servers

### Non-Goals
- Not implementing any MCP servers
- Not modifying existing MCP configurations (reference only)
- Not creating automated configuration generators (future work)
