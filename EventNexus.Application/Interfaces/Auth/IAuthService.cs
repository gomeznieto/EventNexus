using EventNexus.Application.DTOs;

namespace EventNexus.Application.Interfaces;

public interface IAuthService {
    public Task<Guid> RegisterCustomerAsync(RegisterCustomerRequestDto dto);
    public Task<Guid> RegisterOrganizerAsync(RegisterOrganizerRequestDto dto);
    public Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
    public Task<AuthResponseDto> VerifyLoginCodeAsync(VerifyRequestDto dto);
    public Task RevokeTokenAsync(string id);
    public Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
}
