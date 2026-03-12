using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RESR.Core.Controllers.Comments.Ports;
using RESR.Core.Security.Token;

namespace RESR.WebAPI.Security;

public sealed class CommentOwnerOrPermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    public const string CanDeleteOtherUsersCommentsItemKey = "CanDeleteOtherUsersComments";

    private readonly ITokenService _tokenService;
    private readonly ICommentRepository _commentRepository;
    private readonly string _routeCommentIdParamName;
    private readonly string[] _requiredPermissions;

    public CommentOwnerOrPermissionAuthorizationFilter(
        ITokenService tokenService,
        ICommentRepository commentRepository,
        string routeCommentIdParamName,
        string[] requiredPermissions)
    {
        _tokenService = tokenService;
        _commentRepository = commentRepository;
        _routeCommentIdParamName = routeCommentIdParamName;
        _requiredPermissions = requiredPermissions ?? Array.Empty<string>();
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new UnauthorizedObjectResult("Missing or invalid Authorization header.");
            return;
        }

        var jwtToken = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(jwtToken) || !_tokenService.ValidateToken(jwtToken))
        {
            context.Result = new UnauthorizedObjectResult("Invalid token or unauthorized access.");
            return;
        }

        var tokenSubject = _tokenService.GetArgumentFromToken(jwtToken, JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(tokenSubject, out var subjectUserId))
        {
            context.Result = new UnauthorizedObjectResult("Invalid token or missing subject claim.");
            return;
        }

        var userPermissions = context.HttpContext.User.Claims
            .Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var hasRequiredPermissions = _requiredPermissions.Length > 0 && _requiredPermissions.All(userPermissions.Contains);
        context.HttpContext.Items[CanDeleteOtherUsersCommentsItemKey] = hasRequiredPermissions;
        if (hasRequiredPermissions)
            return;

        var routeCommentIdValue = context.RouteData.Values[_routeCommentIdParamName]?.ToString();
        if (!int.TryParse(routeCommentIdValue, out var commentId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var comment = await _commentRepository.GetByIdAsync(commentId, context.HttpContext.RequestAborted);
        if (comment is null)
            return;

        if (comment.IdUser != subjectUserId)
            context.Result = new ForbidResult();
    }
}
