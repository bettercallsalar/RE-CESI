using System.Net;
using System.Net.Http.Json;
using RESR.MAUI.Services;

namespace RESR.MAUI.Tests.Services;

public sealed class FollowsApiClientTests
{
    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenEndpointReturnsNoContent()
    {
        var session = new StubApiSession();
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/follows/3/9", request.RequestUri?.PathAndQuery);
            Assert.Null(request.Headers.Authorization);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
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
    public async Task FollowAsync_SendsBearerHeader_AndBody()
    {
        var session = new StubApiSession
        {
            Token = "jwt-token-for-follow"
        };

        var handler = new StubHttpMessageHandler(async request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/follows", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-token-for-follow", request.Headers.Authorization.Parameter);

            var payload = await request.Content!.ReadFromJsonAsync<RESR.Models.Follows.FollowRequest>();
            Assert.NotNull(payload);
            Assert.Equal(3, payload!.IdFollower);
            Assert.Equal(9, payload.IdFollowing);

            return new HttpResponseMessage(HttpStatusCode.NoContent);
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
            Assert.Equal("/api/follows/3/9", request.RequestUri?.PathAndQuery);
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
}
