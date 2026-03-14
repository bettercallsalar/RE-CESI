using Moq;
using RESR.Core.Controllers.Comments;
using RESR.Core.Controllers.Comments.Factories;
using RESR.Core.Controllers.Comments.Ports;
using RESR.Core.Errors;
using RESR.Models.Comments;

namespace RESR.Core.Tests.Comments;

public sealed class CommentServiceTests
{
    [Fact]
    public async Task GetByResourceIdAsync_Throws_WhenResourceMissing()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.ResourceExistsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByResourceIdAsync(4, CancellationToken.None));
    }

    [Fact]
    public async Task GetByResourceIdAsync_ReturnsComments_WhenResourceExists()
    {
        var service = CreateService(out var repo, out _);
        var expected = new List<Comment> { BuildComment() };
        repo.Setup(r => r.ResourceExistsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.GetByResourceIdAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await service.GetByResourceIdAsync(4, CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDeletedComment_WhenCommentDeleted()
    {
        var service = CreateService(out var repo, out _);
        var deletedAt = DateTime.UtcNow;
        repo.Setup(r => r.GetByIdAsync(8, It.IsAny<CancellationToken>())).ReturnsAsync(BuildComment(deletedAt: deletedAt));

        var result = await service.GetByIdAsync(8, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(deletedAt, result!.DeletedAt);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenParentBelongsToAnotherResource()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.ResourceExistsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildComment(idComment: 1, idResource: 9));

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateCommentCommand(4, "Hello", 2, 1), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_NormalizesContent_AndDelegatesToRepository()
    {
        var service = CreateService(out var repo, out var factory);
        repo.Setup(r => r.ResourceExistsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildComment(idComment: 1, idResource: 4));

        factory.Setup(f => f.CreateForCreation("Hello", 4, 2, 1))
            .Returns(BuildComment(content: "Hello", idResource: 4, idUser: 2, idParentComment: 1));

        repo.Setup(r => r.CreateAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildComment(idComment: 12, content: "Hello", idResource: 4, idUser: 2, idParentComment: 1));

        var result = await service.CreateAsync(new CreateCommentCommand(4, " Hello ", 2, 1), CancellationToken.None);

        Assert.Equal(12, result.IdComment);
        factory.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenActorIsNotAuthor()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildComment(idComment: 5, idUser: 4));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateAsync(new UpdateCommentCommand(5, "Updated", 2), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesContent_WhenActorIsAuthor()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildComment(idComment: 5, idUser: 2));
        repo.Setup(r => r.UpdateContentAsync(5, "Updated", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildComment(idComment: 5, content: "Updated", idUser: 2));

        var result = await service.UpdateAsync(new UpdateCommentCommand(5, " Updated ", 2), CancellationToken.None);

        Assert.Equal("Updated", result.Content);
    }

    [Fact]
    public async Task DeleteAsync_AllowsAuthorWithoutExtraPermission()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildComment(idComment: 5, idUser: 2));
        repo.Setup(r => r.SoftDeleteAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await service.DeleteAsync(5, 2, canDeleteOtherUsersComments: false, CancellationToken.None);

        repo.Verify(r => r.SoftDeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_AllowsModeratorPermission()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildComment(idComment: 5, idUser: 9));
        repo.Setup(r => r.SoftDeleteAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await service.DeleteAsync(5, 2, canDeleteOtherUsersComments: true, CancellationToken.None);

        repo.Verify(r => r.SoftDeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenActorCannotDelete()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildComment(idComment: 5, idUser: 9));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.DeleteAsync(5, 2, canDeleteOtherUsersComments: false, CancellationToken.None));
    }

    private static CommentService CreateService(out Mock<ICommentRepository> repo, out Mock<ICommentFactory> factory)
    {
        repo = new Mock<ICommentRepository>();
        factory = new Mock<ICommentFactory>();
        return new CommentService(repo.Object, factory.Object);
    }

    private static Comment BuildComment(
        int idComment = 5,
        string content = "Hello",
        int idResource = 4,
        int idUser = 2,
        int? idParentComment = null,
        DateTime? deletedAt = null)
    {
        return new Comment
        {
            IdComment = idComment,
            Content = content,
            CreatedAt = new DateTime(2026, 3, 11, 12, 0, 0, DateTimeKind.Utc),
            ModifiedAt = null,
            DeletedAt = deletedAt,
            IdResource = idResource,
            IdUser = idUser,
            IdParentComment = idParentComment
        };
    }
}
