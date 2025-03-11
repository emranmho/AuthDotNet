using AuthDotNet.Models;
using AuthDotNet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthDotNet.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDto request)
    {
        var user = await authService.RegisterAsync(request);
        if (user is null)
        {
            return BadRequest("Username already exist!");
        }
        return Ok(user);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserDto request)
    {
        var result = await authService.LoginAsync(request);
        if (result is null)
        {
            return BadRequest("Invalid username or password");
        }
        return Ok(result);
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto request)
    {
        var result = await authService.RefreshAsync(request);
        if (result is null || result.AccessToken is null || result.RefreshToken is null)
        {
            return Unauthorized("Invalid refresh token");
        }
        return Ok(result);
    }

    [HttpGet("auth-check/admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult AuthenticatedAdminOnly()
    {
        return Ok("You are authenticated and Admin");
    }
    
    [HttpGet("auth-check")]
    [Authorize(Roles = "Customer")]
    public IActionResult Authenticated()
    {
        return Ok("You are authenticated");
    }
}