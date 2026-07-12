using System.Net.Http;
using System.Net.Http.Headers;

namespace Microsoft.Extensions.AI.Mcp.Authentication;

/// <summary>
/// Delegating handler that appends bearer tokens and handles 401 challenges with retry.
/// </summary>
public sealed class AuthenticatedHttpMessageHandler : DelegatingHandler
{
    private readonly IMcpAuthenticationHandler _authenticationHandler;
    private readonly IMcpAuthenticationRetryPolicy _retryPolicy;
    private readonly McpAuthenticationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatedHttpMessageHandler"/> class.
    /// </summary>
    public AuthenticatedHttpMessageHandler(
        IMcpAuthenticationHandler authenticationHandler,
        IMcpAuthenticationRetryPolicy? retryPolicy = null,
        McpAuthenticationOptions? options = null)
    {
        _authenticationHandler = authenticationHandler ?? throw new ArgumentNullException(nameof(authenticationHandler));
        _retryPolicy = retryPolicy ?? new UnauthorizedMcpAuthenticationRetryPolicy();
        _options = options ?? new McpAuthenticationOptions();
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        for (var attempt = 0; ; attempt++)
        {
            using var requestClone = await CloneRequestAsync(request, cancellationToken).ConfigureAwait(false);
            var authContext = new McpAuthenticationContext(requestClone, attempt, cancellationToken);
            var token = await _authenticationHandler.GetAccessTokenAsync(authContext).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(token))
            {
                requestClone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(requestClone, cancellationToken).ConfigureAwait(false);
            if (!_retryPolicy.ShouldRetry(response, attempt, _options))
            {
                return response;
            }

            var challengeContext = new McpAuthenticationChallengeContext(
                requestClone,
                response,
                attempt,
                GetWwwAuthenticate(response),
                cancellationToken);

            await _authenticationHandler.HandleAuthenticationChallengeAsync(challengeContext).ConfigureAwait(false);
            response.Dispose();
        }
    }

    private static string? GetWwwAuthenticate(HttpResponseMessage response)
    {
        return response.Headers.TryGetValues("WWW-Authenticate", out var values)
            ? string.Join(", ", values)
            : null;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy,
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

#pragma warning disable CS0618
        foreach (var property in request.Properties)
        {
            clone.Properties[property.Key] = property.Value;
        }
#pragma warning restore CS0618

        if (request.Content is not null)
        {
            var memoryStream = new MemoryStream();
            await request.Content.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            memoryStream.Position = 0;

            clone.Content = new StreamContent(memoryStream);
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
