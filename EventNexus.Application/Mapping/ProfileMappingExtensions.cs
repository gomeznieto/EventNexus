using EventNexus.Application.DTOs;
using EventNexus.Domain.Entities;

namespace EventNexus.Application.Mapping;

public static class ProfileMappingExetensions{

    public static CurrentUserDto ToResponse(this User user){
        return new CurrentUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            ComapanyName = user.OrganizerProfile?.CompanyName,
            BusinessPhone = user.OrganizerProfile?.BusinessPhone
        };
    }
}
