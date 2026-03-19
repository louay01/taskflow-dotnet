using TaskFlow.Core.Entities;

namespace TaskFlow.Core.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
}
