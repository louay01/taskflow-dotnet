namespace TaskFlow.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RefreshAsync(string refreshToken);
    Task RevokeAsync(string refreshToken);
}

public record RegisterRequest(string FirstName, string LastName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResult(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserInfo User);
public record UserInfo(string Id, string Email, string FirstName, string LastName, IList<string> Roles);
