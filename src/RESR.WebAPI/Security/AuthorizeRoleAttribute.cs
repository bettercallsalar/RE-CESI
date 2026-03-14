using Microsoft.AspNetCore.Mvc;

namespace RESR.WebAPI.Security;

public sealed class AuthorizeRoleAttribute : TypeFilterAttribute
{
    public AuthorizeRoleAttribute(params int[] roleIds) : base(typeof(RoleAuthorizationFilter))
    {
        Arguments = new object[] { roleIds };
    }
}
