namespace Microsoft.Extensions.AI.Mcp.Authentication;

/// <summary>
/// Options for MCP authentication behavior.
/// </summary>
public sealed class McpAuthenticationOptions
{
    /// <summary>
    /// Gets or sets the number of retry attempts after the initial request when 401 is returned.
    /// </summary>
    public int MaxRetryCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether retry-on-401 behavior is enabled.
    /// </summary>
    public bool RetryOnUnauthorized { get; set; } = true;
}
