using EventNexus.Application.DTOs;
using EventNexus.Domain.Entities;
using EventNexus.Domain.Enums;

namespace EventNexus.Application.Mapping;

public static class EventMappingExtensions
{
    // Dto to Entity
    public static Event ToEntity(this CreateEventRequestDto dto, Guid organizerId)
    {
        return new Event
        {
            Title = dto.Title,
            Description = dto.Description,
            UrlImage = dto.UrlImage,
            StartDate = dto.StartDate.ToUniversalTime(),
            EndDate = dto.EndDate.ToUniversalTime(),
            Capacity = dto.Capacity,
            Price = dto.Price,
            Modality = dto.Modality,
            VenueId = dto.VenueId,
            OrganizerId = organizerId,
            Status = EventStatus.Draft // Default business rule!
        };
    }

    // Entity to Dto
    public static EventResponseDto ToResponseDto(this Event entity)
        {
            return new EventResponseDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                UrlImage = entity.UrlImage,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Capacity = entity.Capacity,
                Price = entity.Price,
                Modality = entity.Modality,
                Status = entity.Status,
                OrganizerId = entity.OrganizerId,
                VenueId = entity.VenueId,
               
                OrganizerName = entity.Organizer?.CompanyName ?? string.Empty, 
                VenueName = entity.Venue?.Name
            };
        }
}
