using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthDotNet.Data;
using AuthDotNet.Entities;
using AuthDotNet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthDotNet.Services;

public class AuthService(AuthDbContext context, IConfiguration configuration) : IAuthService
{
    public async Task<User?> RegisterAsync(UserDto request)
    {
        if (await context.Users.AnyAsync(d => d.UserName == request.UserName))
        {
            return null;
        }

        var user = new User();
        var hashedPassword = new PasswordHasher<User>()
            .HashPassword(user, request.Password);
        
        user.PasswordHash = hashedPassword;
        user.UserName = request.UserName;
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        return user;
    }

    public async Task<TokenResponseDto?> LoginAsync(UserDto request)
    {
        var user = await context
            .Users
            .SingleOrDefaultAsync(d => d.UserName == request.UserName);

        if (user is null)
        {
            return null;
        }
        
        if (new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return await CreateResponseToken(user);
    }

    private async Task<TokenResponseDto> CreateResponseToken(User user)
    {
        var response = new TokenResponseDto
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
        };
        return response;
    }

    public async Task<TokenResponseDto?> RefreshAsync(RefreshTokenRequestDto request)
    {
        var user = await ValidateRefreshToken(request.UserId, request.RefreshToken);
        if (user is null)
        {
            return null;
        }
        return await CreateResponseToken(user);
    }

    private string CreateToken(User request)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, request.Id.ToString()),
            new Claim(ClaimTypes.Name, request.UserName),
            new Claim(ClaimTypes.Email, "mhoemran@gmail.com"),
            new Claim(ClaimTypes.Role, request.Role),
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

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var range = RandomNumberGenerator.Create();
        range.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(4);
        
        await context.SaveChangesAsync();
        return refreshToken;
    }

    private async Task<User?> ValidateRefreshToken(Guid userId, string refreshToken)
    {
        var user = await context.Users.FindAsync(userId);
        if (user is null || user.RefreshToken != refreshToken
                         || user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            return null;
        }
        
        return user;
    }
}