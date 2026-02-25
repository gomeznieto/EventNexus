using EventNexus.Domain.Enums;

namespace EventNexus.Domain.Entities;

public class Event{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? UrlImage { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Duration { get; set; }
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public int AvailableTickets { get; set; }

    public EventModality Modality { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    
    public Guid OrganizerId  { get; set; }
    public Organizer? Organizer { get; set; }

    public int? VenueId { get; set; }
    public Venue? Venue  { get; set; }

    public ICollection<Order> Orders  { get; set; } = [];
}
