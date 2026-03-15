using System.Net;
using System.Text;
using RESR.MAUI.Services;

namespace RESR.MAUI.Tests.Services;

public sealed class CommentsApiClientTests
{
    [Fact]
    public async Task GetByResourceIdAsync_ReturnsComments_WithoutAuthorizationHeader()
    {
        var session = new StubApiSession();
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/comments/resources/12", request.RequestUri?.PathAndQuery);
            Assert.Null(request.Headers.Authorization);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = Json("""
                [
                  {
                    "idComment": 1,
                    "content": "Premier commentaire",
                    "createdAt": "2026-03-14T09:00:00Z",
                    "modifiedAt": null,
                    "deletedAt": null,
                    "idResource": 12,
                    "idUser": 2,
                    "idParentComment": null
                  }
                ]
                """)
            });
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new CommentsApiClient(httpClient, session);

        var comments = await sut.GetByResourceIdAsync(12, CancellationToken.None);

        Assert.Single(comments);
        Assert.Equal(1, comments[0].IdComment);
    }

    [Fact]
    public async Task CreateAsync_SendsBearerHeader_AndBody()
    {
        var session = new StubApiSession
        {
            Token = "jwt-comment-token"
        };

        var handler = new StubHttpMessageHandler(async request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/comments/resources/12", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-comment-token", request.Headers.Authorization.Parameter);

            var payload = await request.Content!.ReadAsStringAsync();
            Assert.Contains("\"content\":\"Reponse utile\"", payload);
            Assert.Contains("\"idParentComment\":4", payload);

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = Json("""
                {
                  "idComment": 6,
                  "content": "Reponse utile",
                  "createdAt": "2026-03-14T09:30:00Z",
                  "modifiedAt": null,
                  "deletedAt": null,
                  "idResource": 12,
                  "idUser": 7,
                  "idParentComment": 4
                }
                """)
            };
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new CommentsApiClient(httpClient, session);

        var comment = await sut.CreateAsync(12, new RESR.Models.Comments.CreateCommentRequest("Reponse utile", 4), CancellationToken.None);

        Assert.Equal(6, comment.IdComment);
        Assert.Equal(4, comment.IdParentComment);
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
