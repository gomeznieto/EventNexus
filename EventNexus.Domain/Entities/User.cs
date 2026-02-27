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

    public ICollection<VerificationCode> LoginCodes { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public Organizer? OrganizerProfile { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

