
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class TokenService : ITokenService {
    private readonly SymmetricSecurityKey _key;
    private readonly IConfiguration _config;

    public TokenService(IConfiguration configuration)
    {
        _config = configuration;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    }

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

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token){
        var tokenValidationParameters = new TokenValidationParameters{
            ValidateAudience = true,
            ValidateIssuer = true,
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
}

