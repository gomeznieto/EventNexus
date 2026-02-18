namespace EventNexus.Domain.Entities;

public class User{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }

    public ICollection<UserLoginCode> LoginCodes { get; set; } = [];
    public ICollection<Ticket> Tickets { get; set; } = [];
    public ICollection<Event> OrganizedEvents { get; set; } = [];
}

