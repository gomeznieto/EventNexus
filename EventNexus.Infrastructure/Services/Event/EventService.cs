using EventNexus.Application.DTOs;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EventNexus.Application.Mapping;
using EventNexus.Domain.Enums;
using EventNexus.Application.Interfaces;

namespace EventNexus.Infrastructure.Services;

public class EventService : IEventService 
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly IVerificationCodeService _codeService;
    private readonly IEmailService _emailService;

    public EventService(
            UserManager<IdentityUser> userManager,
            AppDbContext appDbContext,
            IVerificationCodeService verificationCodeService,
            IEmailService emailService
            )
    {
        _userManager = userManager; 
        _dbContext = appDbContext;
        _codeService = verificationCodeService;
        _emailService = emailService;
    }

    // -- CREATE -- //
    public async Task<EventResponseDto> CreateAsync(CreateEventRequestDto dto, string userId)
    {
        ArgumentNullException.ThrowIfNull(dto); 

        // Validate Organizer
        var organizer = await _dbContext.Organizers.FirstOrDefaultAsync(o => o.UserId == Guid.Parse(userId));
        if(organizer is null) throw new KeyNotFoundException("The Organizer does not exists!");

        // Validate venue Id
        if(dto.VenueId.HasValue){
            // Validate Venue
            if(! await _dbContext.Venues.AnyAsync(v => v.Id == dto.VenueId)){
                throw new KeyNotFoundException("The Venue does not exist. You must create it first!");
            }
        } else if(dto.Modality == EventModality.InPerson){
            throw new ArgumentException("In-Person events must have a valid Venue.");
        }

        // Create
        var newEvent = dto.ToEntity(organizer.Id);

        _dbContext.Events.Add(newEvent);

        await _dbContext.SaveChangesAsync();

        return newEvent.ToResponseDto();
    }

    // -- GET BY ID -- //
    public async Task<EventResponseDto> GetByIdAsync(int id)
    {
       var searchedEvent = await _dbContext.Events
           .Include(e => e.Organizer)
           .Include(e => e.Venue)
           .FirstOrDefaultAsync(e => e.Id == id);

       if(searchedEvent is null) throw new KeyNotFoundException("The Event you are looking for does not exist.");
        
        return searchedEvent.ToResponseDto();
    }

    // -- PUBLISH EVENT -- //
    public async Task<EventResponseDto> PublishEventAsync(VerificationEventCodeDto dto)
    {
        //Cancel Event
        await _codeService.ValidateCodeAsync(dto.UserId, dto.Code, ActionType.PublishEvent);

        // Validate Organizer
        var organizer = await _dbContext.Organizers.FirstOrDefaultAsync(o => o.UserId == dto.UserId);
        if(organizer is null)
            throw new KeyNotFoundException("The organizer you are looking for to activate dows not exist");

        var eventToActivate = await _dbContext.Events
            .FirstOrDefaultAsync(e => e.OrganizerId == organizer.Id && e.Id == dto.EventId);

        if(eventToActivate is null) 
            throw new KeyNotFoundException("The event you are looking for to activate dows not exist");

        eventToActivate.Status = EventStatus.Active;

        await _dbContext.SaveChangesAsync();

        return eventToActivate.ToResponseDto();
    }

    // -- CANCEL EVENT -- //
    public async Task<EventResponseDto> CancelEventAsync(VerificationEventCodeDto dto)
    {
        //Cancel Event
        await _codeService.ValidateCodeAsync(dto.UserId, dto.Code, ActionType.CancelEvent);

        // Validate Organizer
        var organizer = await _dbContext.Organizers.FirstOrDefaultAsync(o => o.UserId == dto.UserId);
        if(organizer is null)
            throw new KeyNotFoundException("The organizer you are looking for to activate dows not exist");

        var eventToActivate = await _dbContext.Events
            .FirstOrDefaultAsync(e => e.OrganizerId == organizer.Id && e.Id == dto.EventId);

        if(eventToActivate is null) 
            throw new KeyNotFoundException("The event you are looking for to activate dows not exist");

        eventToActivate.Status = EventStatus.Active;

        await _dbContext.SaveChangesAsync();

        return eventToActivate.ToResponseDto();
    }

    // -- GENERIC REQUEST EVENT GENERATE CODE -- //
    public async Task<LoginResponseDto> RequestUpdateEventAsync(RequestUpdateEventDto dto)
    {
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

        return new LoginResponseDto {Message = "If that email exists, a code was sent"};
    }

}
