using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Mcp.Authentication;
using Microsoft.Extensions.AI.Mcp.Authentication.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.AI.Mcp.Authentication.Tests;

public sealed class AuthenticatedHttpMessageHandlerTests
{
    [Fact]
    public async Task SendAsync_AttachesBearerToken()
    {
        var authHandler = new TestAuthenticationHandler(_ => new ValueTask<string?>("token-123"));
        HttpRequestMessage? capturedRequest = null;
        using var handler = new AuthenticatedHttpMessageHandler(authHandler)
        {
            InnerHandler = new StubHttpMessageHandler((request, _, _) =>
            {
                capturedRequest = request;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }),
        };

        using var client = new HttpClient(handler);
        using var response = await client.GetAsync("https://example.test/mcp");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(capturedRequest);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization?.Scheme);
        Assert.Equal("token-123", capturedRequest.Headers.Authorization?.Parameter);
        Assert.Equal(1, authHandler.TokenRequestCount);
    }

    [Fact]
    public async Task SendAsync_OnUnauthorized_NotifiesAndRetries()
    {
        var authHandler = new TestAuthenticationHandler(context =>
            new ValueTask<string?>(context.Attempt == 0 ? "stale" : "fresh"));

        var statuses = new[] { HttpStatusCode.Unauthorized, HttpStatusCode.OK };
        var tokens = new List<string?>();

        using var handler = new AuthenticatedHttpMessageHandler(authHandler, options: new McpAuthenticationOptions { MaxRetryCount = 1 })
        {
            InnerHandler = new StubHttpMessageHandler((request, attempt, _) =>
            {
                tokens.Add(request.Headers.Authorization?.Parameter);
                var response = new HttpResponseMessage(statuses[attempt]);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer", "error=\"invalid_token\""));
                }

                return Task.FromResult(response);
            }),
        };

        using var client = new HttpClient(handler);
        using var response = await client.GetAsync("https://example.test/mcp");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new[] { "stale", "fresh" }, tokens);
        Assert.Equal(1, authHandler.ChallengeCount);
        Assert.Contains("invalid_token", authHandler.LastChallenge?.WwwAuthenticate);
    }

    [Fact]
    public async Task SendAsync_StopsRetryWhenBudgetExhausted()
    {
        var authHandler = new TestAuthenticationHandler(_ => new ValueTask<string?>("token"));

        using var handler = new AuthenticatedHttpMessageHandler(authHandler, options: new McpAuthenticationOptions { MaxRetryCount = 1 })
        {
            InnerHandler = new StubHttpMessageHandler((_, _, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized))),
        };

        using var client = new HttpClient(handler);
        using var response = await client.GetAsync("https://example.test/mcp");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(2, authHandler.TokenRequestCount);
        Assert.Equal(1, authHandler.ChallengeCount);
    }

    [Fact]
    public async Task AddMcpTools_RegistersAIFunctions()
    {
        var services = new ServiceCollection();
        services.AddMcpTools(builder => builder.AddTool("ping", "returns pong", _ => new ValueTask<object?>("pong")));

        using var provider = services.BuildServiceProvider();
        var functions = provider.GetRequiredService<IEnumerable<AIFunction>>().ToList();

        Assert.Single(functions);
        Assert.Equal("ping", functions[0].Name);

        var result = await functions[0].InvokeAsync(new AIFunctionArguments());
        Assert.Equal("pong", result);
    }

    [Fact]
    public async Task AddMcpAuthenticationHandler_UsesConfiguredServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMcpAuthenticationHandler>(new TestAuthenticationHandler(_ => new ValueTask<string?>("di-token")));
        services.AddMcpAuthentication(options => options.MaxRetryCount = 0);

        HttpRequestMessage? capturedRequest = null;
        services.AddHttpClient("mcp")
            .AddMcpAuthenticationHandler()
            .ConfigurePrimaryHttpMessageHandler(() => new StubHttpMessageHandler((request, _, _) =>
            {
                capturedRequest = request;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }));

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        using var response = await factory.CreateClient("mcp").GetAsync("https://example.test/mcp");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("di-token", capturedRequest?.Headers.Authorization?.Parameter);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, int, CancellationToken, Task<HttpResponseMessage>> _handler;
        private int _attempt;

        public StubHttpMessageHandler(Func<HttpRequestMessage, int, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var current = _attempt;
            _attempt++;
            return _handler(request, current, cancellationToken);
        }
    }

    private sealed class TestAuthenticationHandler : IMcpAuthenticationHandler
    {
        private readonly Func<McpAuthenticationContext, ValueTask<string?>> _tokenFactory;

        public TestAuthenticationHandler(Func<McpAuthenticationContext, ValueTask<string?>> tokenFactory)
        {
            _tokenFactory = tokenFactory;
        }

        public int TokenRequestCount { get; private set; }

        public int ChallengeCount { get; private set; }

        public McpAuthenticationChallengeContext? LastChallenge { get; private set; }

        public ValueTask<string?> GetAccessTokenAsync(McpAuthenticationContext context)
        {
            TokenRequestCount++;
            return _tokenFactory(context);
        }

        public ValueTask HandleAuthenticationChallengeAsync(McpAuthenticationChallengeContext context)
        {
            ChallengeCount++;
            LastChallenge = context;
            return ValueTask.CompletedTask;
        }
    }
}
