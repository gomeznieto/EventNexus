namespace EventNexus.Domain.Entities;

public class Organizer {
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string BusinessPhone { get; set; } = string.Empty;
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Venue> Venues { get; set; } = [];
        public ICollection<Event> OrganizedEvents { get; set; } = [];

}
