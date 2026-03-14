using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Events;
using RESR.Core.Controllers.Users.Ports;
using RESR.Core.Errors;
using RESR.Core.Security.Token;
using RESR.Models.Departments;
using RESR.Models.Resources;
using RESR.Models.Users;
using RESR.WebAPI.Routes.Events;

namespace RESR.WebAPI.Tests.Events;

public sealed class EventsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsPaginatedResponse()
    {
        var service = new Mock<IEventService>();
        var users = CreateUserRepository();
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
        var controller = new EventsController(service.Object, users.Object, tokenService.Object);

        var result = await controller.GetAll(ct: CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedEventsResponse>(ok.Value);
        Assert.Single(response.Items);
        Assert.Equal(1, response.TotalCount);
        Assert.Equal("user1", response.Items[0].Author.Username);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IEventService>();
        var users = CreateUserRepository();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.GetByResourceIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);
        var controller = new EventsController(service.Object, users.Object, tokenService.Object);

        var result = await controller.GetByResourceId(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetOwnByResourceId_ReturnsForbid_WhenTokenUserDoesNotOwnEvent()
    {
        var service = new Mock<IEventService>();
        var users = CreateUserRepository();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.GetByResourceIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { IdResource = 1, IdUser = 9, Title = "Forum", Visibility = ResourceVisibility.PRIVATE, CreatedAt = DateTime.UtcNow, IdCategory = 2 });
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("1");
        var controller = new EventsController(service.Object, users.Object, tokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers.Authorization = "Bearer jwt-token";

        var result = await controller.GetOwnByResourceId(1, CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task GetPendingApprovalEvents_ReturnsOnlyPendingFilters()
    {
        var service = new Mock<IEventService>();
        var users = CreateUserRepository();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.GetPaginatedAsync(1, 20, It.IsAny<EventListingFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Event>(), 0));
        var controller = new EventsController(service.Object, users.Object, tokenService.Object);

        var result = await controller.GetPendingApprovalEvents(ct: CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        service.Verify(s => s.GetPaginatedAsync(
            1,
            20,
            It.Is<EventListingFilters>(filters =>
                filters.Visibility == null &&
                filters.IsApproved == false &&
                filters.IncludeDeleted == false),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByResourceIdForApproval_ReturnsOk_ForUnapprovedPrivateEvent()
    {
        var service = new Mock<IEventService>();
        var users = CreateUserRepository();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.GetByResourceIdAsync(6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event
            {
                IdResource = 6,
                IdEvent = 2,
                Title = "Forum",
                Visibility = ResourceVisibility.PRIVATE,
                CreatedAt = DateTime.UtcNow,
                IdUser = 1,
                IdCategory = 2,
                IsApproved = false,
                StartDate = new DateTime(2026, 4, 1),
                Department = new Department { IdDepartment = 75, Name = "Department 75", Code = 750 }
            });
        var controller = new EventsController(service.Object, users.Object, tokenService.Object);

        var result = await controller.GetByResourceIdForApproval(6, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<EventResponse>(ok.Value);
        Assert.False(response.IsApproved);
        Assert.Equal("PRIVATE", response.Visibility);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenValid()
    {
        var service = new Mock<IEventService>();
        var users = CreateUserRepository();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.CreateAsync(It.IsAny<CreateEventCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(21);
        tokenService.Setup(s => s.ValidateToken("jwt-token"))
            .Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub"))
            .Returns("8");
        var controller = new EventsController(service.Object, users.Object, tokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers.Authorization = "Bearer jwt-token";

        var result = await controller.Create(
            new CreateEventFormRequest
            {
                Title = "Title",
                Description = null,
                Visibility = "private",
                IdCategory = 2,
                Subtitle = null,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 2),
                Address = "Paris",
                IdDepartment = 75
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetOwnByResourceId), created.ActionName);
        service.Verify(s => s.CreateAsync(
            It.Is<CreateEventCommand>(cmd => cmd.IdUser == 8 && cmd.IdCategory == 2),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IEventService>();
        var users = CreateUserRepository();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.SoftDeleteAsync(6, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        tokenService.Setup(s => s.ValidateToken("jwt-token"))
            .Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub"))
            .Returns("1");
        var controller = new EventsController(service.Object, users.Object, tokenService.Object)
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
        var users = CreateUserRepository();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenException("Forbidden"));
        tokenService.Setup(s => s.ValidateToken("jwt-token"))
            .Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub"))
            .Returns("1");
        var controller = new EventsController(service.Object, users.Object, tokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers.Authorization = "Bearer jwt-token";

        var result = await controller.Update(6, new UpdateEventFormRequest { Title = "Updated" }, CancellationToken.None);

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
        var users = CreateUserRepository();
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

        var controller = new EventsController(service.Object, users.Object, tokenService.Object);
        var result = await controller.SetApproval(6, new SetResourceApprovalRequest(true), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<EventResponse>(ok.Value);
        Assert.True(response.IsApproved);
        Assert.Equal(75, response.Department!.IdDepartment);
        Assert.Equal("User 1", response.Author.FirstName);
    }

    [Fact]
    public async Task GetByResourceIdForApproval_ReturnsNotFound_WhenEventIsDeleted()
    {
        var service = new Mock<IEventService>();
        var users = CreateUserRepository();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.GetByResourceIdAsync(6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event
            {
                IdResource = 6,
                IdEvent = 2,
                Title = "Forum",
                Visibility = ResourceVisibility.PRIVATE,
                CreatedAt = DateTime.UtcNow,
                DeletedAt = DateTime.UtcNow,
                IdUser = 1,
                IdCategory = 2,
                IsApproved = false,
                StartDate = new DateTime(2026, 4, 1),
                Department = new Department { IdDepartment = 75, Name = "Department 75", Code = 750 }
            });
        var controller = new EventsController(service.Object, users.Object, tokenService.Object);

        var result = await controller.GetByResourceIdForApproval(6, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    private static Mock<IUserRepository> CreateUserRepository()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int idUser, CancellationToken _) => new User
            {
                IdUser = idUser,
                Username = $"user{idUser}",
                FirstName = $"User {idUser}",
                Email = $"user{idUser}@example.com",
                HashedPassword = "hash",
                Department = new Department { IdDepartment = 1, Name = "Dept", Code = 1 },
                IdRole = 1
            });

        return users;
    }
}
