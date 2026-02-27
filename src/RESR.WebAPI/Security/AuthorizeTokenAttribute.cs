using Microsoft.AspNetCore.Mvc;

namespace RESR.WebAPI.Security;

public class AuthorizeTokenAttribute : TypeFilterAttribute
{
    public AuthorizeTokenAttribute(params TokenRole[] roles) : base(typeof(GenericAuthorizationFilter))
    {
        Arguments = new object[] { roles };
    }
}