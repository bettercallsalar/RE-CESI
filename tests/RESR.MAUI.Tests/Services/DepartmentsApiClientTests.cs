using System.Net;
using System.Net.Http.Json;
using RESR.MAUI.Services;
using RESR.Models.Departments;

namespace RESR.MAUI.Tests.Services;

public sealed class DepartmentsApiClientTests
{
    [Fact]
    public async Task GetDepartmentsAsync_WhenSuccessful_ReturnsDepartments()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/departments", request.RequestUri?.PathAndQuery);

            var payload = new[]
            {
                new DepartmentResponse(1, "Maintenance", 42),
                new DepartmentResponse(2, "Delivery", 51)
            };

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new DepartmentsApiClient(httpClient);

        var departments = await sut.GetDepartmentsAsync(CancellationToken.None);

        Assert.Collection(
            departments,
            department =>
            {
                Assert.Equal(1, department.IdDepartment);
                Assert.Equal("Maintenance", department.Name);
                Assert.Equal(42, department.Code);
            },
            department =>
            {
                Assert.Equal(2, department.IdDepartment);
                Assert.Equal("Delivery", department.Name);
                Assert.Equal(51, department.Code);
            });
    }

    [Fact]
    public async Task GetDepartmentsAsync_WhenApiReturnsError_ThrowsApiExceptionWithMessage()
    {
        var handler = new StubHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Invalid request")
            }));

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new DepartmentsApiClient(httpClient);

        var exception = await Assert.ThrowsAsync<ApiException>(() => sut.GetDepartmentsAsync(CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("Invalid request", exception.Message);
    }

    [Fact]
    public async Task GetDepartmentsAsync_WhenRequestTimesOut_ThrowsTimeoutException()
    {
        var handler = new StubHttpMessageHandler(_ => throw new TaskCanceledException("timeout"));

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        var sut = new DepartmentsApiClient(httpClient);

        var exception = await Assert.ThrowsAsync<TimeoutException>(() => sut.GetDepartmentsAsync(CancellationToken.None));

        Assert.Equal("Le serveur ne repond pas. Reessayez plus tard.", exception.Message);
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
