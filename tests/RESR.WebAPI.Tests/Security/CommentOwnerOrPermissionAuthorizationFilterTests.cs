using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using RESR.Core.Controllers.Comments.Ports;
using RESR.Core.Security.Token;
using RESR.Models.Comments;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Tests.Security;

public sealed class CommentOwnerOrPermissionAuthorizationFilterTests
{
    [Fact]
    public async Task OnAuthorizationAsync_ReturnsUnauthorized_WhenAuthorizationHeaderMissing()
    {
        var tokenService = new Mock<ITokenService>();
        var commentRepository = new Mock<ICommentRepository>();
        var filter = new CommentOwnerOrPermissionAuthorizationFilter(
            tokenService.Object,
            commentRepository.Object,
            "idComment",
            new[] { PermissionNames.DeleteComment });
        var context = CreateContext(commentId: 5);
        context.HttpContext.Request.Headers.Authorization = string.Empty;

        await filter.OnAuthorizationAsync(context);

        Assert.IsType<UnauthorizedObjectResult>(context.Result);
    }

    [Fact]
    public async Task OnAuthorizationAsync_AllowsRequest_WhenTokenUserOwnsComment()
    {
        var tokenService = new Mock<ITokenService>();
        var commentRepository = new Mock<ICommentRepository>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("7");
        commentRepository.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment { IdComment = 5, IdUser = 7 });
        var filter = new CommentOwnerOrPermissionAuthorizationFilter(
            tokenService.Object,
            commentRepository.Object,
            "idComment",
            new[] { PermissionNames.DeleteComment });
        var context = CreateContext(commentId: 5);

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
        Assert.Equal(false, context.HttpContext.Items[CommentOwnerOrPermissionAuthorizationFilter.CanDeleteOtherUsersCommentsItemKey]);
    }

    [Fact]
    public async Task OnAuthorizationAsync_AllowsRequest_WhenPermissionClaimExists()
    {
        var tokenService = new Mock<ITokenService>();
        var commentRepository = new Mock<ICommentRepository>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("7");
        var filter = new CommentOwnerOrPermissionAuthorizationFilter(
            tokenService.Object,
            commentRepository.Object,
            "idComment",
            new[] { PermissionNames.DeleteComment });
        var context = CreateContext(commentId: 5, permissions: new[] { PermissionNames.DeleteComment });

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
        Assert.Equal(true, context.HttpContext.Items[CommentOwnerOrPermissionAuthorizationFilter.CanDeleteOtherUsersCommentsItemKey]);
        commentRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OnAuthorizationAsync_ReturnsForbid_WhenUserDoesNotOwnComment_AndPermissionMissing()
    {
        var tokenService = new Mock<ITokenService>();
        var commentRepository = new Mock<ICommentRepository>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("7");
        commentRepository.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comment { IdComment = 5, IdUser = 8 });
        var filter = new CommentOwnerOrPermissionAuthorizationFilter(
            tokenService.Object,
            commentRepository.Object,
            "idComment",
            new[] { PermissionNames.DeleteComment });
        var context = CreateContext(commentId: 5);

        await filter.OnAuthorizationAsync(context);

        Assert.IsType<ForbidResult>(context.Result);
    }

    private static AuthorizationFilterContext CreateContext(int commentId, IEnumerable<string>? permissions = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer jwt-token";

        if (permissions is not null)
        {
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    permissions.Select(permission => new System.Security.Claims.Claim("permission", permission)),
                    "test"));
        }

        var routeData = new RouteData();
        routeData.Values["idComment"] = commentId.ToString();

        return new AuthorizationFilterContext(
            new ActionContext(httpContext, routeData, new ActionDescriptor()),
            new List<IFilterMetadata>());
    }
}
