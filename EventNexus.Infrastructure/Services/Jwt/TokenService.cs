
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

    public string CreateToken(User user, IList<string>roles)
    {
        var claims = new List<Claim>{
            new Claim( JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
            new Claim( JwtRegisteredClaimNames.Email, user.Email!),
            new Claim("FullName", $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach(var role in roles){
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}

