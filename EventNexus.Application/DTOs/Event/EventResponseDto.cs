using EventNexus.Domain.Enums;

namespace EventNexus.Application.DTOs;

public class EventResponseDto{
    public int Id { get; set; } 
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? UrlImage { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Duration => (int)(EndDate - StartDate).TotalMinutes; 
    public int Capacity { get; set; }
    public decimal Price { get; set; }

    public EventModality Modality { get; set; }
    public EventStatus Status { get; set; }

    public Guid OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;

    public int? VenueId { get; set; }
    public string? VenueName { get; set; }
}
