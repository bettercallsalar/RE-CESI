using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Events;
using RESR.Core.Errors;
using RESR.Core.Security.Token;
using RESR.Models.Departments;
using RESR.Models.Resources;
using RESR.WebAPI.Routes.Events;

namespace RESR.WebAPI.Tests.Events;

public sealed class EventsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsPaginatedResponse()
    {
        var service = new Mock<IEventService>();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.GetPaginatedAsync(1, 20, It.IsAny<EventListingFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Event>
            {
                new()
                {
                    IdResource = 1,
                    IdEvent = 2,
                    Title = "Forum",
                    Description = null,
                    Visibility = ResourceVisibility.PUBLIC,
                    CreatedAt = DateTime.UtcNow,
                    IdUser = 1,
                    IdCategory = 2,
                    IsApproved = false,
                    Subtitle = null,
                    StartDate = new DateTime(2026, 4, 1),
                    EndDate = null,
                    Address = null,
                    Department = new Department { IdDepartment = 67, Name = "Department 67", Code = 670 }
                }
            }, 1));
        var controller = new EventsController(service.Object, tokenService.Object);

        var result = await controller.GetAll(ct: CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedEventsResponse>(ok.Value);
        Assert.Single(response.Items);
        Assert.Equal(1, response.TotalCount);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IEventService>();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.GetByResourceIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);
        var controller = new EventsController(service.Object, tokenService.Object);

        var result = await controller.GetByResourceId(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenValid()
    {
        var service = new Mock<IEventService>();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.CreateAsync(It.IsAny<CreateEventCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(21);
        tokenService.Setup(s => s.ValidateToken("jwt-token"))
            .Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub"))
            .Returns("8");
        var controller = new EventsController(service.Object, tokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers.Authorization = "Bearer jwt-token";

        var result = await controller.Create(
            new CreateEventRequest(
                "Title",
                null,
                "private",
                2,
                null,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2),
                "Paris",
                75),
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetByResourceId), created.ActionName);
        service.Verify(s => s.CreateAsync(
            It.Is<CreateEventCommand>(cmd => cmd.IdUser == 8 && cmd.IdCategory == 2),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IEventService>();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.SoftDeleteAsync(6, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        tokenService.Setup(s => s.ValidateToken("jwt-token"))
            .Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub"))
            .Returns("1");
        var controller = new EventsController(service.Object, tokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers.Authorization = "Bearer jwt-token";

        var result = await controller.Delete(6, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsForbid_WhenTokenUserDoesNotOwnEvent()
    {
        var service = new Mock<IEventService>();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenException("Forbidden"));
        tokenService.Setup(s => s.ValidateToken("jwt-token"))
            .Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub"))
            .Returns("1");
        var controller = new EventsController(service.Object, tokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers.Authorization = "Bearer jwt-token";

        var result = await controller.Update(6, new UpdateEventRequest(Title: "Updated"), CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
        service.Verify(s => s.UpdateAsync(
            It.Is<UpdateEventCommand>(cmd => cmd.IdResource == 6 && cmd.IdUser == 1),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetApproval_ReturnsOk_WhenValid()
    {
        var service = new Mock<IEventService>();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.SetApprovalAsync(It.IsAny<SetEventApprovalCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event
            {
                IdResource = 6,
                IdEvent = 3,
                Title = "Forum",
                Description = null,
                Visibility = ResourceVisibility.PUBLIC,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = null,
                DeletedAt = null,
                IdUser = 1,
                IdCategory = 2,
                IsApproved = true,
                Subtitle = "Sub",
                StartDate = new DateTime(2026, 4, 1),
                EndDate = null,
                Address = "Paris",
                Department = new Department { IdDepartment = 75, Name = "Department 75", Code = 750 }
            });

        var controller = new EventsController(service.Object, tokenService.Object);
        var result = await controller.SetApproval(6, new SetResourceApprovalRequest(true), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<EventResponse>(ok.Value);
        Assert.True(response.IsApproved);
        Assert.Equal(75, response.Department!.IdDepartment);
    }
}
