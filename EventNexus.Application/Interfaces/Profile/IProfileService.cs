using EventNexus.Application.DTOs;

namespace EventNexus.Application.Interfaces;

public interface IProfileService{
    Task<UserPublicProfileDto> GetByIdAsync(Guid id);
    Task<CurrentUserDto> GetCurrentAsync(string id, IList<string> roles);
    Task<CurrentUserDto> UpdateProfileAsync (string id, UpdateProfileDto dto);
    Task<MessageResponseDto> RequestChangeEmailAsync(RequestUpdateDto dto);
    Task<MessageResponseDto> AuthorizeEmailChangeAsync(Guid userId, string oldEmail, AuthorizeEmailChangeDto dto);
    Task<AuthResponseDto> ConfirmEmailChangeAsync(Guid userId, VerificationCodeDto dto);
}
