using EventNexus.Domain.Enums;

namespace EventNexus.Domain.Entities;

public class Event{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? UrlImage { get; set; }

    public DateTime Date { get; set; }
    public int Duration { get; set; }
    public int Capacity { get; set; }
    public decimal Price { get; set; }

    public EventModality Modality { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    
    public Guid UserId  { get; set; }
    public User? User { get; set; }

    public int VenueId { get; set; }
    public Venue? Venue  { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = null!;
}
