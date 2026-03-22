using System.Net;
using RESR.MAUI.Services;
using RESR.Models.Resources;

namespace RESR.MAUI.Tests.Services;

public sealed class ArticlesApiClientTests
{
    [Fact]
    public async Task GetOwnByIdAsync_UsesOwnRoute_AndBearerHeader()
    {
        var session = new StubApiSession
        {
            Token = "jwt-own-article"
        };

        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/articles/me/12", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-own-article", request.Headers.Authorization.Parameter);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = Json("""
                {
                  "idResource": 12,
                  "idArticle": 4,
                  "title": "Mon article",
                  "description": "Description",
                  "type": "article",
                  "visibility": "PRIVATE",
                  "createdAt": "2026-03-13T08:00:00Z",
                  "modifiedAt": null,
                  "deletedAt": null,
                  "idUser": 7,
                  "author": { "idUser": 7, "username": "owner", "firstName": "Owner" },
                  "idCategory": 1,
                  "content": "Corps complet",
                  "isApproved": false,
                  "defaultImageId": null,
                  "files": []
                }
                """)
            });
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new ArticlesApiClient(httpClient, session);

        var article = await sut.GetOwnByIdAsync(12, CancellationToken.None);

        Assert.Equal("PRIVATE", article.Visibility);
    }

    [Fact]
    public async Task UpdateAsync_SendsPatchMultipartRequest_WithBearerHeader()
    {
        var session = new StubApiSession
        {
            Token = "jwt-update-article"
        };

        var handler = new StubHttpMessageHandler(async request =>
        {
            Assert.Equal(HttpMethod.Patch, request.Method);
            Assert.Equal("/api/articles/12", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-update-article", request.Headers.Authorization.Parameter);

            var body = await request.Content!.ReadAsStringAsync();
            Assert.Contains("name=Title", body);
            Assert.Contains("Article modifie", body);
            Assert.Contains("name=Visibility", body);
            Assert.Contains("PRIVATE", body);
            Assert.Contains("name=IdCategory", body);
            Assert.Contains("name=Content", body);
            Assert.Contains("Contenu mis a jour", body);
            Assert.Contains("name=ReplaceImages", body);
            Assert.Contains("False", body);

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new ArticlesApiClient(httpClient, session);

        await sut.UpdateAsync(
            12,
            new UpdateArticleRequest("Article modifie", "Nouvelle description", "PRIVATE", 3, "Contenu mis a jour"),
            [],
            defaultImageIndex: null,
            CancellationToken.None);
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
