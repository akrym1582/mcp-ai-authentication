namespace Microsoft.Extensions.AI.Mcp.Authentication.Extensions;

/// <summary>
/// Describes an MCP tool that can be exposed as an AI function.
/// </summary>
/// <param name="Name">Tool name.</param>
/// <param name="Description">Tool description for model consumption.</param>
/// <param name="InvokeAsync">Tool invocation delegate.</param>
public sealed record McpToolDescriptor(
    string Name,
    string? Description,
    Func<CancellationToken, ValueTask<object?>> InvokeAsync);
