
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Entities;
using EventNexus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class TokenService : ITokenService {
    private readonly SymmetricSecurityKey _key;
    private readonly IConfiguration _config;
    private readonly AppDbContext _dbContext;
    public TokenService(
            IConfiguration configuration,
            AppDbContext dbContext
            )
    {
        _config = configuration;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        _dbContext = dbContext;
    }

    // -- CREATE TOKEN -- //
    public string CreateToken(User user, string securityStamp, IList<string>roles, string jti)
    {
        var claims = new List<Claim>{
            new Claim( JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
            new Claim( JwtRegisteredClaimNames.Email, user.Email!),
            new Claim("FullName", $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim("SecurityStamp", securityStamp)
        };

        foreach(var role in roles){
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = creds,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public async Task<string> CreateTokenRefreshAsync(Guid userId, string jti)
    {
        var refreshTokenString = GenerateRefreshToken(); 
        var refreshTokenEntity = new RefreshToken {
            Token = refreshTokenString,
            UserId = userId,
            JwtId = jti,
            CreationDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false, 
            IsUsed = false,
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);

        return refreshTokenString;
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token){
        var tokenValidationParameters = new TokenValidationParameters{
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        var jwtSecurityToken = securityToken as JwtSecurityToken;

        if(jwtSecurityToken is null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)){
            throw new SecurityTokenException("Invalid token format or algorithm.");
        }

        return principal;
    }

    public async Task RevokeRefreshTokenAsync(string currentRefreshToken)
    {
        var storedRefreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync( r => r.Token == currentRefreshToken);

        if(storedRefreshToken is null ||
                storedRefreshToken.IsUsed || 
                storedRefreshToken.IsRevoked
          ) return; 

        storedRefreshToken.IsUsed =  true;
        storedRefreshToken.IsRevoked = true;
    }

    private string GenerateRefreshToken(){
        var randomNumber = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}

