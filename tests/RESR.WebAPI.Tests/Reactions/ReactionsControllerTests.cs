using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Reactions;
using RESR.Core.Errors;
using RESR.Models.Reactions;
using RESR.WebAPI.Routes.Reactions;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Tests.Reactions;

public sealed class ReactionsControllerTests
{
    [Fact]
    public async Task GetByResourceId_ReturnsOk_WithResponses()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetByResourceIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reaction> { BuildReaction(), BuildReaction(idReaction: 6, name: ReactionNames.Love) });

        var result = await controller.GetByResourceId(4, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var responses = Assert.IsType<List<ReactionResponse>>(ok.Value);
        Assert.Equal(2, responses.Count);
        Assert.Equal("user_2", responses[0].User.Username);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync((Reaction?)null);

        var result = await controller.GetById(5, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetByUser_ReturnsCurrentUserReactions_WhenQueryIsMissing()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetByUserIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reaction> { BuildReaction(idReaction: 5, idUser: 2) });

        var result = await controller.GetByUser(null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserReactionsResponse>(ok.Value);
        Assert.Equal(2, response.IdUser);
        Assert.Equal(1, response.TotalCount);
        Assert.Equal("user_2", response.Items[0].User.Username);
    }

    [Fact]
    public async Task GetByUser_ReturnsForbid_WhenQueryTargetsAnotherUserWithoutPermission()
    {
        var controller = CreateController(out _, userId: 2);

        var result = await controller.GetByUser(4, CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task GetByUser_ReturnsOtherUserReactions_WhenPermissionIsPresent()
    {
        var controller = CreateController(
            out var service,
            userId: 2,
            permissions: new[] { PermissionNames.ViewOtherUserReactions });
        service.Setup(s => s.GetByUserIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reaction> { BuildReaction(idReaction: 8, idUser: 4) });

        var result = await controller.GetByUser(4, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserReactionsResponse>(ok.Value);
        Assert.Equal(4, response.IdUser);
        Assert.Single(response.Items);
        Assert.Equal("user_4", response.Items[0].User.Username);
    }

    [Fact]
    public async Task GetByUser_ReturnsUnauthorized_WhenTokenSubjectMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.GetByUser(null, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenSuccess()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.CreateAsync(It.IsAny<CreateReactionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildReaction());

        var result = await controller.Create(4, new CreateReactionRequest(ReactionNames.Like), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ReactionsController.GetById), created.ActionName);
    }

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenTokenSubjectMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.Create(4, new CreateReactionRequest(ReactionNames.Like), CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_ReturnsForbid_WhenServiceRejectsActor()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateReactionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var result = await controller.Update(5, new UpdateReactionRequest(ReactionNames.Dislike), CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.DeleteAsync(5, 2, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await controller.Delete(5, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    private static ReactionsController CreateController(
        out Mock<IReactionService> service,
        int? userId = null,
        IReadOnlyList<string>? permissions = null)
    {
        service = new Mock<IReactionService>();
        var controller = new ReactionsController(service.Object);

        if (userId.HasValue)
        {
            var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, userId.Value.ToString()) };
            if (permissions is not null)
            {
                claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));
            }

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

    private static Reaction BuildReaction(int idReaction = 5, string name = ReactionNames.Like, int idUser = 2)
    {
        return new Reaction
        {
            IdReaction = idReaction,
            Name = name,
            IdResource = 4,
            IdUser = idUser,
            Username = $"user_{idUser}",
            FirstName = $"User{idUser}"
        };
    }
}
