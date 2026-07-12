using Microsoft.Extensions.AI;

namespace Microsoft.Extensions.AI.Mcp.Authentication.Extensions;

/// <summary>
/// Wraps MCP tool delegates as <see cref="AIFunction"/>.
/// </summary>
public static class McpAIFunctionWrapper
{
    /// <summary>
    /// Converts an <see cref="McpToolDescriptor"/> into an <see cref="AIFunction"/>.
    /// </summary>
    public static AIFunction Create(McpToolDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return AIFunctionFactory.Create(
            (Func<CancellationToken, ValueTask<object?>>)(cancellationToken => descriptor.InvokeAsync(cancellationToken)),
            descriptor.Name,
            descriptor.Description);
    }
}
