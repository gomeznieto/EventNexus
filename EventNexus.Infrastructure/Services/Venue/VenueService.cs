using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EventNexus.Application.Mapping;

namespace EventNexus.Infrastructure.Services;

public class VenueService : IVenueService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _dbContext;

    public VenueService(
            UserManager<IdentityUser> userManager,
            AppDbContext appDbContext
            )
    {
        _userManager = userManager; 
        _dbContext = appDbContext;
    }

    // --- CREATE --- //
    public async Task<VenueResponseDto> CreateAsync(CreateVenueRequestDto dto, string userId)
    {
        // Validate organizer
        var organizer = await _dbContext.Organizers.FirstOrDefaultAsync(o => o.UserId == Guid.Parse(userId));
        if(organizer is null) throw new ArgumentException("The Organizer dows not exist");
        
        var venueExists = await _dbContext.Venues.AnyAsync( v => v.OrganizerId == organizer.Id && v.Name.ToLower() == dto.Name.ToLower());

        if(venueExists) throw new ArgumentException("You already have a venue with this name");

        var newVenue = dto.ToEntity(organizer.Id);

        _dbContext.Venues.Add(newVenue);

        await _dbContext.SaveChangesAsync();

        return newVenue.toResponseVenue();
    }

    // -- GET BY ID -- //
    public async Task<VenueResponseDto> GetByIdAsync(int id)
    {
        var searchedVenue = await _dbContext.Venues.Include(v => v.Organizer).FirstOrDefaultAsync(v => v.Id == id);

        if(searchedVenue is null) throw new KeyNotFoundException("The Venue you are looking for does not exist");

        return searchedVenue.toResponseVenue();
    }
}
