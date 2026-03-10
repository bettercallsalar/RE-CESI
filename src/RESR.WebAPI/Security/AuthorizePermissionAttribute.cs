using Microsoft.AspNetCore.Mvc;

namespace RESR.WebAPI.Security;

public sealed class AuthorizePermissionAttribute : TypeFilterAttribute
{
    public AuthorizePermissionAttribute(params string[] permissions) : base(typeof(PermissionAuthorizationFilter))
    {
        Arguments = new object[] { permissions };
    }
}
