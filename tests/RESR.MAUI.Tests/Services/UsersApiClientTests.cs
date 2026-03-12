using System.Net;
using System.Net.Http.Json;
using RESR.MAUI.Services;
using RESR.Models.Users;

namespace RESR.MAUI.Tests.Services;

public sealed class UsersApiClientTests
{
    [Fact]
    public async Task LoginAsync_WithKnownCredentials_StoresTokenForSubsequentCalls()
    {
        var session = new ApiSession();
        var handler = new StubHttpMessageHandler(async request =>
        {
            if (request.Method == HttpMethod.Post && request.RequestUri?.PathAndQuery == "/api/login")
            {
                var payload = await request.Content!.ReadFromJsonAsync<Login>();

                Assert.NotNull(payload);
                Assert.Equal("nouveau.user@test.com", payload.Email);
                Assert.Equal("test9326_", payload.Password);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new { token = "jwt-token-for-tests" })
                };
            }

            if (request.Method == HttpMethod.Get && request.RequestUri?.PathAndQuery == "/api/users")
            {
                Assert.NotNull(request.Headers.Authorization);
                Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
                Assert.Equal("jwt-token-for-tests", request.Headers.Authorization.Parameter);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new PaginatedUsersResponse([], 1, 20, 0, 0))
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new UsersApiClient(httpClient, session);

        await sut.LoginAsync(new Login("nouveau.user@test.com", "test9326_"), CancellationToken.None);

        Assert.Equal("jwt-token-for-tests", session.Token);

        var page = await sut.GetUsersAsync(CancellationToken.None);

        Assert.Empty(page.Items);
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
