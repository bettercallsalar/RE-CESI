using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RESR.Core.Security.Token;
public class GenericAuthorizationFilter : IAuthorizationFilter
{
    private readonly ITokenService _tokenService;
    private readonly TokenRole[] _expectedRoles;

    public GenericAuthorizationFilter(ITokenService tokenService, TokenRole[] expectedRoles)
    {
        _tokenService = tokenService;
        _expectedRoles = expectedRoles ?? Array.Empty<TokenRole>();
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var jwtToken = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);

        if (string.IsNullOrEmpty(jwtToken) || !_tokenService.ValidateToken(jwtToken))
        {
            context.Result = new UnauthorizedObjectResult("Invalid token or unauthorized access.");
            return;
        }

        var clientIs = _tokenService.GetArgumentFromToken(jwtToken, "User");
        var isAdmin = _tokenService.GetArgumentFromToken(jwtToken, "IsAdmin");

        bool isAuthorized = _expectedRoles.Any(role =>
        {
            if (role == TokenRole.Admin)
                return clientIs == "User" && isAdmin == "True";
            if (role == TokenRole.User)
                return clientIs == "User";
            if (role == TokenRole.Customer)
                return clientIs == "Customer";
            return false;
        });

        // if (!isAuthorized)
        // {
        //     context.Result = new UnauthorizedObjectResult("Access restricted to specific roles.");
        // }
    }
}
