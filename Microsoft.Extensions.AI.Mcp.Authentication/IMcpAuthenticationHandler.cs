namespace Microsoft.Extensions.AI.Mcp.Authentication;

/// <summary>
/// Defines callbacks for retrieving tokens and reacting to authentication challenges.
/// </summary>
public interface IMcpAuthenticationHandler
{
    /// <summary>
    /// Gets an access token for the outgoing request.
    /// </summary>
    ValueTask<string?> GetAccessTokenAsync(McpAuthenticationContext context);

    /// <summary>
    /// Handles an authentication challenge response.
    /// </summary>
    ValueTask HandleAuthenticationChallengeAsync(McpAuthenticationChallengeContext context);
}
