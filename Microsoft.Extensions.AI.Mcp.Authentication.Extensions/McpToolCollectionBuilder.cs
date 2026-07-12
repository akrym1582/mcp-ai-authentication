using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.AI.Mcp.Authentication.Extensions;

/// <summary>
/// Builds MCP tool registrations.
/// </summary>
public sealed class McpToolCollectionBuilder
{
    private readonly IServiceCollection _services;

    internal McpToolCollectionBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Registers a tool descriptor.
    /// </summary>
    public McpToolCollectionBuilder AddTool(McpToolDescriptor tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
        _services.AddSingleton(tool);
        return this;
    }

    /// <summary>
    /// Registers a tool invocation delegate.
    /// </summary>
    public McpToolCollectionBuilder AddTool(string name, string? description, Func<CancellationToken, ValueTask<object?>> invokeAsync)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(invokeAsync);

        return AddTool(new McpToolDescriptor(name, description, invokeAsync));
    }
}
