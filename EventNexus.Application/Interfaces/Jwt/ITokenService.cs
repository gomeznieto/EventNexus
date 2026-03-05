using System.Security.Claims;
using EventNexus.Domain.Entities;

namespace EventNexus.Application.Interfaces;

public interface ITokenService{
    public string CreateToken(User user, string securityStamp, IList<string> roles, string jti);
    public Task<string> CreateTokenRefreshAsync(Guid userId, string jti);
    public Task RevokeRefreshTokenAsync(string currentRefreshToken);
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
