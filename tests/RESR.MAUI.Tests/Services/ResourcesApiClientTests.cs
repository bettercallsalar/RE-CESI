using System.Net;
using System.Net.Http.Json;
using RESR.MAUI.Services;
using RESR.Models.Departments;
using RESR.Models.Resources;

namespace RESR.MAUI.Tests.Services;

public sealed class ResourcesApiClientTests
{
    [Fact]
    public async Task GetArticlesAsync_ReturnsPayload_FromPublicEndpoint()
    {
        var session = new ApiSession();
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/articles?page=1&pageSize=5", request.RequestUri?.PathAndQuery);
            Assert.Null(request.Headers.Authorization);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new PaginatedArticlesResponse(
                    [
                        new ArticleResponse(12, 4, "Article test", "Description", "article", "PUBLIC", new DateTime(2026, 3, 13, 8, 0, 0, DateTimeKind.Utc), null, 2, 1, "Contenu", true)
                    ],
                    1,
                    5,
                    1,
                    1))
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
    public async Task GetEventsAsync_SendsBearerHeader_WhenSessionIsAuthenticated()
    {
        var session = new ApiSession
        {
            Token = "jwt-token-for-events"
        };

        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/events?page=1&pageSize=3", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-token-for-events", request.Headers.Authorization.Parameter);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new PaginatedEventsResponse(
                    [
                        new EventResponse(
                            14,
                            7,
                            "Forum test",
                            "Description evenement",
                            "event",
                            "PUBLIC",
                            new DateTime(2026, 3, 13, 9, 0, 0, DateTimeKind.Utc),
                            null,
                            3,
                            1,
                            "Sous titre",
                            new DateTime(2026, 3, 20, 10, 0, 0, DateTimeKind.Utc),
                            new DateTime(2026, 3, 20, 18, 0, 0, DateTimeKind.Utc),
                            "Paris",
                            new Department { IdDepartment = 1, Name = "Paris", Code = 75 },
                            true)
                    ],
                    1,
                    3,
                    1,
                    1))
            });
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new ResourcesApiClient(httpClient, session);

        var page = await sut.GetEventsAsync(1, 3, CancellationToken.None);

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
}
