using System.Net;
using System.Net.Http;

namespace Microsoft.Extensions.AI.Mcp.Authentication;

/// <summary>
/// Retries only when the server returns 401 and retry budget remains.
/// </summary>
public sealed class UnauthorizedMcpAuthenticationRetryPolicy : IMcpAuthenticationRetryPolicy
{
    /// <inheritdoc />
    public bool ShouldRetry(HttpResponseMessage response, int retryAttempt, McpAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(options);

        return options.RetryOnUnauthorized
            && response.StatusCode == HttpStatusCode.Unauthorized
            && retryAttempt < options.MaxRetryCount;
    }
}
