using System.Net;
using RESR.MAUI.Services;
using RESR.Models.Resources;

namespace RESR.MAUI.Tests.Services;

public sealed class EventsApiClientTests
{
    [Fact]
    public async Task GetOwnByIdAsync_UsesOwnRoute_AndBearerHeader()
    {
        var session = new StubApiSession
        {
            Token = "jwt-own-event"
        };

        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/events/me/14", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-own-event", request.Headers.Authorization.Parameter);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = Json("""
                {
                  "idResource": 14,
                  "idEvent": 7,
                  "title": "Mon evenement",
                  "description": "Description evenement",
                  "type": "event",
                  "visibility": "PRIVATE",
                  "createdAt": "2026-03-13T09:00:00Z",
                  "modifiedAt": null,
                  "idUser": 7,
                  "author": { "idUser": 7, "username": "owner", "firstName": "Owner" },
                  "idCategory": 1,
                  "subtitle": "Sous titre",
                  "startDate": "2026-03-20T10:00:00Z",
                  "endDate": "2026-03-20T18:00:00Z",
                  "address": "Paris",
                  "department": {
                    "idDepartment": 1,
                    "name": "Paris",
                    "code": 75
                  },
                  "isApproved": false,
                  "files": [],
                  "deletedAt": null,
                  "defaultImageId": null
                }
                """)
            });
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new EventsApiClient(httpClient, session);

        var @event = await sut.GetOwnByIdAsync(14, CancellationToken.None);

        Assert.Equal("PRIVATE", @event.Visibility);
    }

    [Fact]
    public async Task UpdateAsync_SendsPatchMultipartRequest_WithBearerHeader()
    {
        var session = new StubApiSession
        {
            Token = "jwt-update-event"
        };

        var handler = new StubHttpMessageHandler(async request =>
        {
            Assert.Equal(HttpMethod.Patch, request.Method);
            Assert.Equal("/api/events/14", request.RequestUri?.PathAndQuery);
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("jwt-update-event", request.Headers.Authorization.Parameter);

            var body = await request.Content!.ReadAsStringAsync();
            Assert.Contains("name=Title", body);
            Assert.Contains("Evenement modifie", body);
            Assert.Contains("name=StartDate", body);
            Assert.Contains("2026-03-20T10:00:00.0000000", body);
            Assert.Contains("name=EndDate", body);
            Assert.Contains("2026-03-20T18:00:00.0000000", body);
            Assert.Contains("name=Address", body);
            Assert.Contains("Paris", body);
            Assert.Contains("name=ReplaceImages", body);
            Assert.Contains("False", body);

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new EventsApiClient(httpClient, session);

        await sut.UpdateAsync(
            14,
            new UpdateEventRequest(
                "Evenement modifie",
                "Description mise a jour",
                "PRIVATE",
                2,
                "Sous titre",
                new DateTime(2026, 3, 20, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 3, 20, 18, 0, 0, DateTimeKind.Utc),
                "Paris",
                75),
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
