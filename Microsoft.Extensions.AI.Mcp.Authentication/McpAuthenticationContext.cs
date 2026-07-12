using System.Net.Http;

namespace Microsoft.Extensions.AI.Mcp.Authentication;

/// <summary>
/// Context for token acquisition.
/// </summary>
/// <param name="Request">The outgoing request.</param>
/// <param name="Attempt">The zero-based authentication attempt number.</param>
/// <param name="CancellationToken">The cancellation token.</param>
public sealed record McpAuthenticationContext(
    HttpRequestMessage Request,
    int Attempt,
    CancellationToken CancellationToken);
