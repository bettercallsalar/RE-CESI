using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Users;
using RESR.Models.Users;

namespace RESR.WebAPI.Routes.Users;

[ApiController]
[Route("api/login")]
public sealed class LoginController : ControllerBase
{
    private readonly IUserService _service;

    public LoginController(IUserService service) => _service = service;

    /// <summary>
    /// Logs in a user.
    /// </summary>
    /// <param name="loginDto">
    /// The login details.
    /// Email : Required
    /// Password : Required
    /// </param>
    /// <returns>
    /// The token if the login is successful. Null if the login is unsuccessful.
    /// </returns>
    /// 
    [HttpPost]
    public async Task<IActionResult> LoginUserAsync([FromBody] Login loginDto, CancellationToken ct)
    {
        try
        {
            var result = await _service.LoginUserAsync(loginDto, ct);
            return Ok(new { token = result });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
