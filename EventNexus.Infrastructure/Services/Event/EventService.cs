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
    public EventService(
            UserManager<IdentityUser> userManager,
            AppDbContext appDbContext
            )
    {
        _userManager = userManager; 
        _dbContext = appDbContext;
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

    public async Task<EventResponseDto> PublishEventAsync(int eventId, Guid organizerId)
    {
        var eventToActivate = await _dbContext.Events
            .FirstOrDefaultAsync(e => e.OrganizerId == organizerId && e.Id == eventId);

        if(eventToActivate is null) 
            throw new KeyNotFoundException("The event does not exist, or you do not have permission to modify it.");

        if(eventToActivate.Status == EventStatus.Active) 
            throw new InvalidOperationException("This event is already active and published.");

        eventToActivate.Status = EventStatus.Active;

        await _dbContext.SaveChangesAsync();

        return eventToActivate.ToResponseDto();
    }
}
