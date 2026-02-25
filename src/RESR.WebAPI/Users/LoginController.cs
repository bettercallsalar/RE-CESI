using Microsoft.AspNetCore.Mvc;
using RESR.Core.Users;
using RESR.Models.Users;

namespace RESR.WebAPI.Users;

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
    [HttpPost("login-user")]
    public async Task<IActionResult> LoginUserAsync([FromBody] LoginDto loginDto)
    {
        var result = await _service.LoginUserAsync(loginDto);
        if (result != null)
        {
            return Ok(result);
        }
        else
        {
            return Unauthorized("Invalid credentials.");
        }
    }
}

