using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Comments;
using RESR.Core.Errors;
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
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenUserIdIsMappedToNameIdentifier()
    {
        var service = new Mock<ICommentService>();
        service.Setup(s => s.CreateAsync(It.IsAny<CreateCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildComment());

        var controller = new CommentsController(service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[] { new Claim(ClaimTypes.NameIdentifier, "2") },
                            "test"))
                }
            }
        };

        var result = await controller.Create(4, new CreateCommentRequest("Hello"), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(CommentsController.GetById), created.ActionName);
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
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        var controller = CreateController(out var service, userId: 2, permissions: new[] { "DeleteComment" });
        service.Setup(s => s.DeleteAsync(5, 2, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await controller.Delete(5, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenValidationFails()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.DeleteAsync(0, 2, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
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
        var controller = new CommentsController(service.Object);

        if (userId.HasValue || permissions is not null)
        {
            var claims = new List<Claim>();

            if (userId.HasValue)
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()));

            if (permissions is not null)
                claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
                }
            };
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
