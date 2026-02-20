using EventNexus.Application.DTOs;

namespace EventNexus.Application.Interfaces;

public interface IAuthService {
    public Task<Guid> RegisterAsync(RegisterRequestDtos dto);
    public Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
    public Task<AuthResponseDto> VerifyLoginCodeAsync(VerifyRequestDto dto);
}
