using EventNexus.Domain.Enums;

namespace EventNexus.Application.Interfaces;

public interface IVerificationCodeService{
    Task<string> GenerateCodeAsync(Guid user, ActionType purpose);
    Task ValidateCodeAsync(Guid user, string code, ActionType purpose);
}
