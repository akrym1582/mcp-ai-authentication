# Copilot Instructions

## Repository overview

- This repository provides MCP authentication integration libraries for .NET.
- Main projects:
  - `Microsoft.Extensions.AI.Mcp.Authentication`
  - `Microsoft.Extensions.AI.Mcp.Authentication.Extensions`
  - `Microsoft.Extensions.AI.Mcp.Authentication.Tests`

## Development rules

- Keep changes small and focused.
- Avoid breaking public APIs unless explicitly requested.
- Follow existing naming and style in nearby files.
- Prefer dependency injection patterns already used in the repository.

## Validation

- Build:
  - `dotnet build McpAiAuthentication.slnx`
- Tests:
  - `dotnet run --project Microsoft.Extensions.AI.Mcp.Authentication.Tests/Microsoft.Extensions.AI.Mcp.Authentication.Tests.csproj`

## Notes for agent-generated changes

- Do not add new dependencies unless required.
- Do not modify unrelated tests or files.
- Update `README.md` when public-facing behavior or usage changes.
