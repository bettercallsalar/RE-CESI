using Microsoft.AspNetCore.Mvc;

namespace RESR.WebAPI.Security;

public sealed class AuthorizePermissionOrSelfAttribute : TypeFilterAttribute
{
    public AuthorizePermissionOrSelfAttribute(string routeIdParamName, params string[] permissions)
        : base(typeof(PermissionOrSelfAuthorizationFilter))
    {
        Arguments = new object[] { routeIdParamName, permissions };
    }
}
