using EventNexus.Application.DTOs;
using EventNexus.Domain.Entities;

namespace EventNexus.Application.Mapping;

public static class VenueMappingExtensions{
    public static Venue ToEntity(this CreateVenueRequestDto dto, Guid organizerId){
        return new Venue{
            Address = dto.Address,
            Name = dto.Name,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            OrganizerId = organizerId
        };
    }

    public static DTOs.VenueResponseDto toResponseVenue(this Venue venue){
        return new VenueResponseDto{
            Id = venue.Id,
            Name = venue.Name,
            Latitude = venue.Latitude,
            Longitude = venue.Longitude
        };
    }
}
