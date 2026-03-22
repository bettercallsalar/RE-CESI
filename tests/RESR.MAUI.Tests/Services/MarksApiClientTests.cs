using System.Net;
using System.Text;
using RESR.MAUI.Services;

namespace RESR.MAUI.Tests.Services;

public sealed class MarksApiClientTests
{
    [Fact]
    public async Task GetFavoritesAsync_SendsAuthorizedRequest()
    {
        var session = new StubApiSession { Token = "jwt-mark-token" };
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/marks/favorite?page=1&pageSize=20", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = Json("""
                {
                  "items": [
                    { "idMark": 5, "isFavorite": true, "isReadLater": false, "idRessource": 12, "idUser": 2 }
                  ],
                  "page": 1,
                  "pageSize": 20,
                  "totalCount": 1,
                  "totalPages": 1
                }
                """)
            });
        });

        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:8080/") };
        var sut = new MarksApiClient(httpClient, session);

        var page = await sut.GetFavoritesAsync(1, 20, CancellationToken.None);

        Assert.Single(page.Items);
        Assert.True(page.Items[0].IsFavorite);
    }

    [Fact]
    public async Task GetFavoriteAsync_ReturnsNull_OnNotFound()
    {
        var session = new StubApiSession { Token = "jwt-mark-token" };
        var handler = new StubHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));

        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:8080/") };
        var sut = new MarksApiClient(httpClient, session);

        var mark = await sut.GetFavoriteAsync(12, CancellationToken.None);

        Assert.Null(mark);
    }

    [Fact]
    public async Task MarkAsReadLaterAsync_PostsToExpectedEndpoint()
    {
        var session = new StubApiSession { Token = "jwt-mark-token" };
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/marks/readLater/12", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = Json("""
                { "idMark": 7, "isFavorite": false, "isReadLater": true, "idRessource": 12, "idUser": 2 }
                """)
            });
        });

        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:8080/") };
        var sut = new MarksApiClient(httpClient, session);

        var mark = await sut.MarkAsReadLaterAsync(12, CancellationToken.None);

        Assert.True(mark.IsReadLater);
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
        public void Clear() => Token = null;
    }

    private static StringContent Json(string value) =>
        new(value, Encoding.UTF8, "application/json");
}
