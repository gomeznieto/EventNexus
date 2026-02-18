namespace EventNexus.Domain.Entities;

public class UserLoginCode
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime Expiration { get; set; }
    public DateTime? UsedAt { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
}
