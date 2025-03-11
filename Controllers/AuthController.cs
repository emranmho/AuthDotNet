using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthDotNet.Entities;
using AuthDotNet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthDotNet.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IConfiguration configuration) : ControllerBase
{
    public static User user = new();
    
    [HttpPost("register")]
    public ActionResult<User> Register([FromBody] UserDto request)
    {
        var hashedPassword = new PasswordHasher<User>()
            .HashPassword(user, request.Password);
        
        user.PasswordHash = hashedPassword;
        user.UserName = request.UserName;
        
        return Ok(user);
    }
    
    [HttpPost("login")]
    public ActionResult<string> Login([FromBody] UserDto request)
    {
        if (user.UserName != request.UserName)
        {
            return BadRequest("User not found");
        }

        if (new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
        {
            return BadRequest("Wrong password");
        }

        string token = CreateToken(user);
        
        return Ok(token);
    }

    private string CreateToken(User request)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, request.UserName),
            new Claim(ClaimTypes.Email, "mhoemran@gmail.com"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim("Project", "AuthDotNet")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration.GetValue<string>("JwtSettings:Token")!));
        
        var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("JwtSettings:Issuer")!,
            audience: configuration.GetValue<string>("JwtSettings:Audience")!,
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credential
        );
        
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}