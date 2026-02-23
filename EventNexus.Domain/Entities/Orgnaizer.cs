namespace EventNexus.Domain.Entities;

public class Organizer {
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string BussinessPhone { get; set; } = string.Empty;
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
}
