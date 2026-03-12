using Microsoft.AspNetCore.Mvc;

namespace RESR.WebAPI.Security;

public sealed class AuthorizeCommentOwnerOrPermissionAttribute : TypeFilterAttribute
{
    public AuthorizeCommentOwnerOrPermissionAttribute(string routeCommentIdParamName, params string[] permissions)
        : base(typeof(CommentOwnerOrPermissionAuthorizationFilter))
    {
        Arguments = new object[] { routeCommentIdParamName, permissions };
    }
}
