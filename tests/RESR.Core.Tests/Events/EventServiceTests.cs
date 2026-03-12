using Moq;
using RESR.Core.Controllers.Events;
using RESR.Core.Controllers.Events.Ports;
using RESR.Core.Errors;
using RESR.Models.Resources;

namespace RESR.Core.Tests.Events;

public sealed class EventServiceTests
{
    [Fact]
    public async Task GetPaginatedAsync_NormalizesKeywordAndReturnsCount()
    {
        var repo = new Mock<IEventRepository>();
        EventListingFilters? captured = null;
        repo.Setup(r => r.GetPaginatedAsync(1, 10, It.IsAny<EventListingFilters>(), It.IsAny<CancellationToken>()))
            .Callback<int, int, EventListingFilters, CancellationToken>((_, _, filters, _) => captured = filters)
            .ReturnsAsync(new List<Event>());
        repo.Setup(r => r.CountAsync(It.IsAny<EventListingFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);
        var service = new EventService(repo.Object);

        var (_, totalCount) = await service.GetPaginatedAsync(
            1,
            10,
            new EventListingFilters("  forum  ", null, null, null, null, null, null, null),
            CancellationToken.None);

        Assert.Equal(4, totalCount);
        Assert.NotNull(captured);
        Assert.Equal("forum", captured!.Keyword);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenEndDateBeforeStartDate()
    {
        var repo = new Mock<IEventRepository>();
        var service = new EventService(repo.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(
                new CreateEventCommand(
                    "Event",
                    null,
                    ResourceVisibility.PUBLIC,
                    1,
                    1,
                    null,
                    new DateTime(2026, 1, 3),
                    new DateTime(2026, 1, 2),
                    null,
                    null),
                CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNotFound()
    {
        var repo = new Mock<IEventRepository>();
        repo.Setup(r => r.GetByResourceIdAsync(404, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);
        var service = new EventService(repo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(new UpdateEventCommand(404, Title: "x"), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenEffectiveDatesInvalid()
    {
        var repo = new Mock<IEventRepository>();
        repo.Setup(r => r.GetByResourceIdAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildEvent(start: new DateTime(2026, 1, 10), end: new DateTime(2026, 1, 12)));
        var service = new EventService(repo.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateAsync(new UpdateEventCommand(8, EndDate: new DateTime(2026, 1, 9)), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_ReturnsPatchedEvent()
    {
        var repo = new Mock<IEventRepository>();
        repo.Setup(r => r.GetByResourceIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildEvent());
        repo.Setup(r => r.PatchAsync(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildEvent(title: "Updated"));
        var service = new EventService(repo.Object);

        var result = await service.UpdateAsync(new UpdateEventCommand(10, Title: " Updated "), CancellationToken.None);

        Assert.Equal("Updated", result.Title);
    }

    [Fact]
    public async Task SoftDeleteAsync_DelegatesToRepository()
    {
        var repo = new Mock<IEventRepository>();
        repo.Setup(r => r.SoftDeleteAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = new EventService(repo.Object);

        var deleted = await service.SoftDeleteAsync(3, CancellationToken.None);

        Assert.True(deleted);
    }

    [Fact]
    public async Task SetApprovalAsync_DelegatesToRepository()
    {
        var repo = new Mock<IEventRepository>();
        var approved = BuildEvent();
        approved.IsApproved = true;
        repo.Setup(r => r.SetApprovalAsync(new SetEventApprovalCommand(9, true), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approved);
        var service = new EventService(repo.Object);

        var @event = await service.SetApprovalAsync(new SetEventApprovalCommand(9, true), CancellationToken.None);

        Assert.True(@event.IsApproved);
    }

    private static Event BuildEvent(string title = "Event", DateTime? start = null, DateTime? end = null) =>
        new()
        {
            IdResource = 10,
            IdEvent = 2,
            Title = title,
            Description = null,
            Visibility = ResourceVisibility.PUBLIC,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = null,
            DeletedAt = null,
            IdUser = 1,
            IdCategory = 2,
            IsApproved = false,
            Subtitle = null,
            StartDate = start ?? new DateTime(2026, 1, 10),
            EndDate = end ?? new DateTime(2026, 1, 11),
            Address = null,
            IdDepartment = null
        };
}
