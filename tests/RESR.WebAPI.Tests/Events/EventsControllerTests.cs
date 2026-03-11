using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Events;
using RESR.Models.Resources;
using RESR.WebAPI.Routes.Events;

namespace RESR.WebAPI.Tests.Events;

public sealed class EventsControllerTests
{
    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IEventService>();
        service.Setup(s => s.GetByResourceIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);
        var controller = new EventsController(service.Object);

        var result = await controller.GetByResourceId(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenValid()
    {
        var service = new Mock<IEventService>();
        service.Setup(s => s.CreateAsync(It.IsAny<CreateEventCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(21);
        var controller = new EventsController(service.Object);

        var result = await controller.Create(
            new CreateEventRequest(
                "Title",
                null,
                "private",
                1,
                2,
                null,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2),
                "Paris",
                75),
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetByResourceId), created.ActionName);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IEventService>();
        service.Setup(s => s.SoftDeleteAsync(6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var controller = new EventsController(service.Object);

        var result = await controller.Delete(6, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
