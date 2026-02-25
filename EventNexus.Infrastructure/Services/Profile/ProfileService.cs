using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventNexus.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _dbContext;

    public ProfileService(
            UserManager<IdentityUser> userManager,
            AppDbContext appDbContext
            )
    {
        _userManager = userManager;
        _dbContext = appDbContext;
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
}
