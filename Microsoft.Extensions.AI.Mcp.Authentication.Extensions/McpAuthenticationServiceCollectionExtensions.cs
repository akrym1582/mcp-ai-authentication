using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Mcp.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.AI.Mcp.Authentication.Extensions;

/// <summary>
/// Dependency injection extensions for MCP OAuth authentication integration.
/// </summary>
public static class McpAuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Registers MCP authentication core services.
    /// </summary>
    public static IServiceCollection AddMcpAuthentication(this IServiceCollection services, Action<McpAuthenticationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IMcpAuthenticationRetryPolicy, UnauthorizedMcpAuthenticationRetryPolicy>();
        services.TryAddSingleton(_ =>
        {
            var options = new McpAuthenticationOptions();
            configure?.Invoke(options);
            return options;
        });

        return services;
    }

    /// <summary>
    /// Adds the <see cref="AuthenticatedHttpMessageHandler"/> to an HTTP client pipeline.
    /// </summary>
    public static IHttpClientBuilder AddMcpAuthenticationHandler(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddHttpMessageHandler(sp =>
            new AuthenticatedHttpMessageHandler(
                sp.GetRequiredService<IMcpAuthenticationHandler>(),
                sp.GetRequiredService<IMcpAuthenticationRetryPolicy>(),
                sp.GetRequiredService<McpAuthenticationOptions>()));
    }

    /// <summary>
    /// Registers MCP tools and exposes them as <see cref="AIFunction"/> instances.
    /// </summary>
    public static IServiceCollection AddMcpTools(this IServiceCollection services, Action<McpToolCollectionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new McpToolCollectionBuilder(services);
        configure(builder);

        services.TryAddSingleton<IReadOnlyList<AIFunction>>(sp =>
            sp.GetServices<McpToolDescriptor>()
              .Select(McpAIFunctionWrapper.Create)
              .ToArray());

        services.TryAddSingleton<IEnumerable<AIFunction>>(sp =>
            sp.GetRequiredService<IReadOnlyList<AIFunction>>());

        return services;
    }
}
