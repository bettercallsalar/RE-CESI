using System.Net;
using System.Text;
using RESR.MAUI.Services;

namespace RESR.MAUI.Tests.Services;

public sealed class ResourcesApiClientTests
{
    [Fact]
    public async Task GetArticlesAsync_ReturnsPayload_FromPublicEndpoint()
    {
        var session = new StubApiSession();
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/articles?page=1&pageSize=5", request.RequestUri?.PathAndQuery);
            Assert.Null(request.Headers.Authorization);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = Json("""
                {
                  "items": [
                    {
                      "idResource": 12,
                      "idArticle": 4,
                      "title": "Article test",
                      "description": "Description",
                      "type": "article",
                      "visibility": "PUBLIC",
                      "createdAt": "2026-03-13T08:00:00Z",
                      "modifiedAt": null,
                      "deletedAt": null,
                      "idUser": 2,
                      "idCategory": 1,
                      "content": "Contenu",
                      "isApproved": true,
                      "idDepartment": null,
                      "files": []
                    }
                  ],
                  "page": 1,
                  "pageSize": 5,
                  "totalCount": 1,
                  "totalPages": 1
                }
                """)
            });
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new ResourcesApiClient(httpClient, session);

        var page = await sut.GetArticlesAsync(1, 5, CancellationToken.None);

        Assert.Single(page.Items);
        Assert.Equal("Article test", page.Items[0].Title);
    }

    [Fact]
    public async Task GetArticlesAsync_AddsKeywordQueryString_WhenProvided()
    {
        var session = new StubApiSession();
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/articles?page=2&pageSize=10&keyword=charge%20mentale", request.RequestUri?.PathAndQuery);
            Assert.Null(request.Headers.Authorization);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = Json("""
                {
                  "items": [],
                  "page": 2,
                  "pageSize": 10,
                  "totalCount": 0,
                  "totalPages": 0
                }
                """)
            });
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new ResourcesApiClient(httpClient, session);

        await sut.GetArticlesAsync(2, 10, "charge mentale", CancellationToken.None);
    }

    [Fact]
    public async Task GetEventsAsync_SendsBearerHeader_WhenSessionIsAuthenticated()
    {
        var session = new StubApiSession
        {
            Token = "jwt-token-for-events"
        };

        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/events?page=1&pageSize=3&keyword=forum", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-token-for-events", request.Headers.Authorization.Parameter);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = Json("""
                {
                  "items": [
                    {
                      "idResource": 14,
                      "idEvent": 7,
                      "title": "Forum test",
                      "description": "Description evenement",
                      "type": "event",
                      "visibility": "PUBLIC",
                      "createdAt": "2026-03-13T09:00:00Z",
                      "modifiedAt": null,
                      "deletedAt": null,
                      "idUser": 3,
                      "idCategory": 1,
                      "subtitle": "Sous titre",
                      "startDate": "2026-03-20T10:00:00Z",
                      "endDate": "2026-03-20T18:00:00Z",
                      "address": "Paris",
                      "idDepartment": 1,
                      "department": {
                        "idDepartment": 1,
                        "name": "Paris",
                        "code": 75
                      },
                      "isApproved": true,
                      "files": []
                    }
                  ],
                  "page": 1,
                  "pageSize": 3,
                  "totalCount": 1,
                  "totalPages": 1
                }
                """)
            });
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new ResourcesApiClient(httpClient, session);

        var page = await sut.GetEventsAsync(1, 3, "forum", CancellationToken.None);

        Assert.Single(page.Items);
        Assert.Equal("Forum test", page.Items[0].Title);
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
