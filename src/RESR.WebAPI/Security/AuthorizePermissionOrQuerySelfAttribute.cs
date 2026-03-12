using Microsoft.AspNetCore.Mvc;

namespace RESR.WebAPI.Security;

public sealed class AuthorizePermissionOrQuerySelfAttribute : TypeFilterAttribute
{
    public AuthorizePermissionOrQuerySelfAttribute(string queryIdParamName, params string[] permissions)
        : base(typeof(PermissionOrQuerySelfAuthorizationFilter))
    {
        Arguments = new object[] { queryIdParamName, permissions };
    }
}
