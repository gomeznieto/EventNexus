using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EventNexus.Application.Mapping;
using EventNexus.Domain.Enums;

namespace EventNexus.Infrastructure.Services;

public class EventService : IEventService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _dbContext;
    public EventService(
            UserManager<IdentityUser> userManager,
            AppDbContext appDbContext
            )
    {
        _userManager = userManager; 
        _dbContext = appDbContext;
    }

    public async Task<EventResponseDto> CreateAsync(CreateEventRequestDto dto, string userId)
    {
        ArgumentNullException.ThrowIfNull(dto); 

        // Validate Organizer
        var organizer = await _dbContext.Organizers.FirstOrDefaultAsync(o => o.UserId == Guid.Parse(userId));
        if(organizer is null) throw new ArgumentException("The Organizer does not exists!");

        // Validate venue Id
        if(dto.VenueId.HasValue){
            // Validate Venue
            if(! await _dbContext.Venues.AnyAsync(v => v.Id == dto.VenueId)){
                throw new ArgumentException("The Venue does not exist. You must create it first!");
            }
        } else if(dto.Modality == EventModality.InPerson){
            throw new ArgumentException("In-Person events must have a valid Venue.");
        }

        // Create new event, save and map
        var newEvent = dto.ToEntity(organizer.Id);

        _dbContext.Events.Add(newEvent);

        await _dbContext.SaveChangesAsync();

        return newEvent.ToResponseDto();
    }
}
