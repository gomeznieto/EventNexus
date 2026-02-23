using EventNexus.Domain.Entities;

namespace EventNexus.Application.Interfaces;

public interface ITokenService{
    public string CreateToken(User user, string securityStamp, IList<string> roles, string jti);
}
