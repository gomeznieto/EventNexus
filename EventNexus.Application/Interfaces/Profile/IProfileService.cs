using EventNexus.Application.DTOs;

namespace EventNexus.Application.Interfaces;

public interface IProfileService{
    public Task<UserPublicProfileDto> GetByIdAsync(Guid id);
    public Task<CurrentUserDto> GetCurrentAsync(string id, IList<string> roles);
    Task<MessageResponseDto> RequestChangeEmailAsync(RequestUpdateDto dto);
    Task<MessageResponseDto> AuthorizeEmailChangeAsync(Guid userId, string oldEmail, AuthorizeEmailChangeDto dto);
    Task<AuthResponseDto> ConfirmEmailChangeAsync(Guid userId, VerificationCodeDto dto);
}
