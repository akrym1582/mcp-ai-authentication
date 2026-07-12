using System.Net.Http;

namespace Microsoft.Extensions.AI.Mcp.Authentication;

/// <summary>
/// Context for handling a 401 authentication challenge.
/// </summary>
/// <param name="Request">The failed request.</param>
/// <param name="Response">The 401 response.</param>
/// <param name="Attempt">The zero-based authentication attempt number.</param>
/// <param name="WwwAuthenticate">The raw WWW-Authenticate header value.</param>
/// <param name="CancellationToken">The cancellation token.</param>
public sealed record McpAuthenticationChallengeContext(
    HttpRequestMessage Request,
    HttpResponseMessage Response,
    int Attempt,
    string? WwwAuthenticate,
    CancellationToken CancellationToken);
