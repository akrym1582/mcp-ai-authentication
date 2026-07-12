using System.Net.Http;

namespace Microsoft.Extensions.AI.Mcp.Authentication;

/// <summary>
/// Determines if an authentication retry should occur.
/// </summary>
public interface IMcpAuthenticationRetryPolicy
{
    /// <summary>
    /// Returns <see langword="true"/> when the request should be retried.
    /// </summary>
    bool ShouldRetry(HttpResponseMessage response, int retryAttempt, McpAuthenticationOptions options);
}
