using EventNexus.Application.Interfaces;
using EventNexus.Application.Settings;
using EventNexus.Domain.Entities;
using EventNexus.Domain.Enums;
using EventNexus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventNexus.Infrastructure.Services;

public class VerificationCodeService : IVerificationCodeService
{
    private readonly AppDbContext _dbContext;
    private readonly TicketSettings _ticketSetting;

    public VerificationCodeService(
            AppDbContext appDbContext,
            IOptions<TicketSettings> options
            )
    {
        _dbContext = appDbContext;
        _ticketSetting = options.Value;
    }

    // -- GENERATE CODE -- //
    public async Task<string> GenerateCodeAsync(Guid userId, ActionType purpose)
    {
        // Verify others valid codes
        var activeCode = await _dbContext.VerificationCodes
            .FirstOrDefaultAsync( u => 
                    u.UserId == userId &&
                    DateTime.UtcNow < u.Expiration &&
                    u.Purpose == purpose 
                    );

        if (activeCode is not null){
            activeCode.UsedAt = DateTime.UtcNow;
        }
        
        // Generate a login code
        var code = GenerateCodeValue();
        int expiresInMinutes = 15;

        var newLoginCode = new VerificationCode {
            UserId = userId,
            Code = code,
            Purpose = purpose,
            Expiration = DateTime.UtcNow.AddMinutes(expiresInMinutes),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.VerificationCodes.Add(newLoginCode);

        return code;
    }

    // -- VERIFICATION CODE SENDING -- //
    public async Task ValidateCodeAsync(Guid userId, string code, ActionType purpose)
    {
        var activeCode = await _dbContext.VerificationCodes
            .FirstOrDefaultAsync(c => 
                    c.UserId == userId &&
                    c.Code == code && 
                    c.UsedAt == null && 
                    c.Purpose == purpose 
                    );

        if(activeCode is null) throw new ArgumentException("The code is not valid");
        if(activeCode.Expiration < DateTime.UtcNow) throw new ArgumentException("Code has expired");

        // Update code to revoke
        activeCode.UsedAt = DateTime.UtcNow;
    }

    // Generate six numbers code
    private string GenerateCodeValue(){
        var MIN_CODE = _ticketSetting.MinCode; 
        var MAX_CODE = _ticketSetting.MaxCode;
        return Random.Shared.Next(MIN_CODE, MAX_CODE).ToString(); 
    }

}
