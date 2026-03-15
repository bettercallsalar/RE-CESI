using System.Net;
using RESR.MAUI.Services;
using RESR.Models.Follows;

namespace RESR.MAUI.Tests.Services;

public sealed class FollowsApiClientTests
{
    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenEndpointReturnsNoContent()
    {
        var session = new StubApiSession
        {
            Token = "jwt-token-for-follow-state"
        };
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/follows/me/following/9", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-token-for-follow-state", request.Headers.Authorization.Parameter);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = Json("""
                {
                  "idFollower": 3,
                  "idFollowing": 9,
                  "isFollowing": true
                }
                """)
            });
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new FollowsApiClient(httpClient, session);

        var exists = await sut.ExistsAsync(3, 9, CancellationToken.None);

        Assert.True(exists);
    }

    [Fact]
    public async Task FollowAsync_SendsBearerHeader()
    {
        var session = new StubApiSession
        {
            Token = "jwt-token-for-follow"
        };

        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/follows/9", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-token-for-follow", request.Headers.Authorization.Parameter);
            Assert.Null(request.Content);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new FollowsApiClient(httpClient, session);

        await sut.FollowAsync(3, 9, CancellationToken.None);
    }

    [Fact]
    public async Task UnfollowAsync_SendsBearerHeader()
    {
        var session = new StubApiSession
        {
            Token = "jwt-token-for-unfollow"
        };

        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal("/api/follows/9", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-token-for-unfollow", request.Headers.Authorization.Parameter);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new FollowsApiClient(httpClient, session);

        await sut.UnfollowAsync(3, 9, CancellationToken.None);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handler(request);
        }
    }

    private sealed class StubApiSession : IApiSession
    {
        public string? Token { get; set; }

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

        public void Clear()
        {
            Token = null;
        }
    }

    private static StringContent Json(string value) =>
        new(value, System.Text.Encoding.UTF8, "application/json");
}
