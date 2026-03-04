using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Entities;
using EventNexus.Domain.Enums;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventNexus.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly IVerificationCodeService _codeService;
    private readonly IEmailService _emailService;

    public ProfileService(
            UserManager<IdentityUser> userManager,
            AppDbContext appDbContext,
            IVerificationCodeService codeService,
            IEmailService emailService
            )
    {
        _userManager = userManager;
        _dbContext = appDbContext;
        _codeService = codeService;
        _emailService = emailService;
    }

    // -- GET PUBLIC USER -- //
    public async Task<UserPublicProfileDto> GetByIdAsync(Guid searchId)
    {

        // Validate Admin Profile
        var searchUser = await _userManager.FindByIdAsync(searchId.ToString());
        if (searchUser is null) 
            throw new KeyNotFoundException("The profile you are looking for does not exist");

        var searchUserRole = await _userManager.GetRolesAsync(searchUser);
        if (searchUserRole.Contains("Admin")) 
            throw new ArgumentException("The profile you are looking for is not public");

        var profile = await _dbContext.Users
            .Where(u => u.Id == searchId)
            .Select(u => new UserPublicProfileDto
            {
                Id = u.Id,
                DisplayName = $"{u.FirstName} {u.LastName}",
                CompanyName = u.OrganizerProfile != null
                    ? u.OrganizerProfile.CompanyName
                    : null,
                HostedEvents = u.OrganizerProfile != null
                    ? u.OrganizerProfile.OrganizedEvents
                    .Where(o => o.StartDate > DateTime.UtcNow)
                    .Select(e => new EventSummaryDto
                    {
                        Title = e.Title,
                        UrlImage = e.UrlImage,
                        StartDate = e.StartDate
                    }).ToList()
                    : null
            })
            .FirstOrDefaultAsync();

        if (profile is null) 
            throw new KeyNotFoundException("The user that you are looking for does not exist");

        return profile;
    }

    // -- GET CURRENT -- //
    public async Task<CurrentUserDto> GetCurrentAsync(string id, IList<string> roles)
    {
        var currentUser = await _dbContext.Users
            .Where(u => u.Id == Guid.Parse(id))
            .Select(u => new CurrentUserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Roles = roles,
                ComapanyName = u.OrganizerProfile != null
                    ? u.OrganizerProfile.CompanyName
                    : null,
                BusinessPhone = u.OrganizerProfile != null
                    ? u.OrganizerProfile.BusinessPhone
                    : null
            })
        .FirstOrDefaultAsync();

        if (currentUser is null) 
            throw new KeyNotFoundException("The User you are looking for does not exist");

        return currentUser;
    }

    // -- REQUEST CHANGE EMAIL -- //
    public async Task<MessageResponseDto> RequestChangeEmailAsync(RequestUpdateDto dto) {

        // Create code
        var code = await _codeService.GenerateCodeAsync(dto.UserId, dto.Action);
        
        // Sending email with the login code
        var newMsg = new EmailDetailsDto {
           Destination = dto.Email!,
           Subject = "Verification Code",
           Body = $"Please enter the following code to complete {nameof(dto.Action)}: {code}.\nCode will expire in 15 minutes."
        };
        await _emailService.SendEmailAsync(newMsg);

        await _dbContext.SaveChangesAsync();

        return new MessageResponseDto {Message = "If that email exists, a code was sent"};
    }

    // -- AUTHORIZE EMAIL CANGE -- //
    public async Task<MessageResponseDto> AuthorizeEmailChangeAsync(Guid userId, string currentEmail, AuthorizeEmailChangeDto dto)
    {
        // Verify Code
        await _codeService.ValidateCodeAsync(userId, dto.VerificationCode, ActionType.UpdateEmail);
       
        // Verify old request
        var existingRequest = await _dbContext.EmailChangeRequests.Where(e => 
                    e.UserId == userId &&
                    e.CompletedAt == null &&
                    e.ExpiresAt > DateTime.UtcNow
                    ).ToListAsync();

        if(existingRequest.Any()){
            foreach(var request in existingRequest){
                request.ExpiresAt = DateTime.UtcNow;
                request.Status = EmailChangeStatus.Cancelled;
            }
        }
       
        // Verify new Email
        if(await _userManager.FindByEmailAsync(dto.NewEmail) != null)
            throw new ArgumentException("The email is already registry");

        var changeRequest = new  EmailChangeRequest{
            UserId = userId,
            CurrentEmail = currentEmail,
            NewEmail = dto.NewEmail,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        _dbContext.EmailChangeRequests.Add(changeRequest);

        // Create code
        var code = await _codeService.GenerateCodeAsync(userId, ActionType.UpdateEmail);

        // Sending email with the login code
        var newMsg = new EmailDetailsDto {
            Destination = dto.NewEmail,
            Subject = "Verification Code",
            Body = $"Please enter the following code to complete {nameof(ActionType.UpdateEmail)}: {code}.\nCode will expire in 15 minutes."
        };

        await _dbContext.SaveChangesAsync();

        await _emailService.SendEmailAsync(newMsg);

        return new MessageResponseDto {Message = "If that email exists, a code was sent"};
    }

    public async Task<AuthResponseDto> ConfirmEmailChangeAsync(Guid userId, VerificationCodeDto dto)
    {
        await _codeService.ValidateCodeAsync(userId, dto.Code, ActionType.UpdateEmail);

        var changeEmail = await _dbContext.EmailChangeRequests.FirstOrDefaultAsync( e => 
                e.UserId == userId &&
                e.ExpiresAt < DateTime.UtcNow &&
                e.CompletedAt == null &&
                e.Status == EmailChangeStatus.PendingConfirmation
                );

        if(changeEmail is null)
            throw new ArgumentException("The request you are looking for does not exist");

        var userIdentity = await _userManager.FindByIdAsync(userId.ToString());
        var userApp = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if(userIdentity is null || userApp is null)
            throw new ArgumentException("The User you are looking for does not exist");

        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try{
            
            await _userManager.SetEmailAsync(userIdentity, changeEmail.NewEmail);
            await _userManager.SetUserNameAsync(userIdentity, changeEmail.NewEmail);
            userApp.Email = changeEmail.NewEmail;

            changeEmail.Status = EmailChangeStatus.Completed;
            changeEmail.CompletedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

        } catch(Exception){
            await transaction.RollbackAsync();
            throw;
        }

        // Crer Tokens
    }
}
