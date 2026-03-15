using System.Net;
using System.Text;
using RESR.MAUI.Services;
using RESR.Models.Reactions;

namespace RESR.MAUI.Tests.Services;

public sealed class ReactionsApiClientTests
{
    [Fact]
    public async Task GetByResourceIdAsync_UsesPublicEndpoint_WithoutAuthorization()
    {
        var session = new StubApiSession();
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/resources/12/reactions", request.RequestUri?.PathAndQuery);
            Assert.Null(request.Headers.Authorization);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = Json("""
                [
                  {
                    "idReaction": 7,
                    "name": "love",
                    "idResource": 12,
                    "idUser": 3,
                    "user": {
                      "idUser": 3,
                      "username": "alice",
                      "firstName": "Alice"
                    }
                  }
                ]
                """)
            });
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new ReactionsApiClient(httpClient, session);

        var reactions = await sut.GetByResourceIdAsync(12, CancellationToken.None);

        Assert.Single(reactions);
        Assert.Equal(ReactionNames.Love, reactions[0].Name);
    }

    [Fact]
    public async Task CreateAsync_SendsBearerHeader_AndPayload()
    {
        var session = new StubApiSession { Token = "jwt-reaction-token" };
        var handler = new StubHttpMessageHandler(async request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/resources/12/reactions", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-reaction-token", request.Headers.Authorization.Parameter);

            var payload = await request.Content!.ReadAsStringAsync();
            Assert.Contains("\"name\":\"like\"", payload);

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = Json("""
                {
                  "idReaction": 9,
                  "name": "like",
                  "idResource": 12,
                  "idUser": 4,
                  "user": {
                    "idUser": 4,
                    "username": "camille",
                    "firstName": "Camille"
                  }
                }
                """)
            };
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new ReactionsApiClient(httpClient, session);

        var reaction = await sut.CreateAsync(12, new CreateReactionRequest(ReactionNames.Like), CancellationToken.None);

        Assert.Equal(9, reaction.IdReaction);
        Assert.Equal(ReactionNames.Like, reaction.Name);
    }

    [Fact]
    public async Task DeleteAsync_UsesApiRoute_WithBearerHeader()
    {
        var session = new StubApiSession { Token = "jwt-delete-token" };
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal("/api/reactions/11", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-delete-token", request.Headers.Authorization.Parameter);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new ReactionsApiClient(httpClient, session);

        await sut.DeleteAsync(11, CancellationToken.None);
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
        new(value, Encoding.UTF8, "application/json");
}
