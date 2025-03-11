using AuthDotNet.Entities;
using AuthDotNet.Models;

namespace AuthDotNet.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(UserDto request);
    Task<TokenResponseDto?> LoginAsync(UserDto request);
    Task<TokenResponseDto?> RefreshAsync(RefreshTokenRequestDto request);
}