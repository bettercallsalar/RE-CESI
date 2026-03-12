using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Comments;
using RESR.Core.Errors;
using RESR.Core.Security.Token;
using RESR.Models.Comments;
using RESR.WebAPI.Routes.Comments;

namespace RESR.WebAPI.Tests.Comments;

public sealed class CommentsControllerTests
{
    [Fact]
    public async Task GetByResourceId_ReturnsOk_WithResponses()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetByResourceIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>
            {
                BuildComment(),
                BuildComment(idComment: 6, idParentComment: 5, deletedAt: new DateTime(2026, 3, 11, 13, 0, 0, DateTimeKind.Utc))
            });

        var result = await controller.GetByResourceId(4, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var responses = Assert.IsType<List<CommentResponse>>(ok.Value);
        Assert.Equal(2, responses.Count);
        Assert.NotNull(responses[1].DeletedAt);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync((Comment?)null);

        var result = await controller.GetById(5, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenSuccess()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.CreateAsync(It.IsAny<CreateCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildComment());

        var result = await controller.Create(4, new CreateCommentRequest("Hello", null), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(CommentsController.GetById), created.ActionName);
        service.Verify(s => s.CreateAsync(
            It.Is<CreateCommentCommand>(cmd => cmd.IdResource == 4 && cmd.IdUser == 2 && cmd.Content == "Hello"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenTokenSubjectMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.Create(4, new CreateCommentRequest("Hello", null), CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_ReturnsForbid_WhenServiceRejectsActor()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateCommentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var result = await controller.Update(5, new UpdateCommentRequest("Updated"), CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
        service.Verify(s => s.UpdateAsync(
            It.Is<UpdateCommentCommand>(cmd => cmd.IdComment == 5 && cmd.ActorUserId == 2 && cmd.Content == "Updated"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.DeleteAsync(5, 2, false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await controller.Delete(5, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenValidationFails()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.DeleteAsync(0, 2, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Bad"));

        var result = await controller.Delete(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private static CommentsController CreateController(
        out Mock<ICommentService> service,
        int? userId = null,
        IEnumerable<string>? permissions = null)
    {
        service = new Mock<ICommentService>();
        var tokenService = new Mock<ITokenService>();
        var controller = new CommentsController(service.Object, tokenService.Object);

        if (userId.HasValue || permissions is not null)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.HttpContext.Request.Headers.Authorization = "Bearer jwt-token";

            if (userId.HasValue)
            {
                tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
                tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub"))
                    .Returns(userId.Value.ToString());
            }

            if (permissions is not null)
            {
                controller.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        permissions.Select(permission => new System.Security.Claims.Claim("permission", permission)),
                        "test"));
            }
        }

        return controller;
    }

    private static Comment BuildComment(int idComment = 5, int? idParentComment = null, DateTime? deletedAt = null)
    {
        return new Comment
        {
            IdComment = idComment,
            Content = "Hello",
            CreatedAt = new DateTime(2026, 3, 11, 12, 0, 0, DateTimeKind.Utc),
            DeletedAt = deletedAt,
            IdResource = 4,
            IdUser = 2,
            IdParentComment = idParentComment
        };
    }
}
