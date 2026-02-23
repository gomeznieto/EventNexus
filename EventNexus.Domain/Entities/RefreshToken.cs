namespace EventNexus.Domain.Entities;

public class RefreshToken{
    public Guid Id { get; set; }
    public string Token { get; set; } = null!;
    public string JwtId { get; set; } = null!;
    public DateTime CreationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    
    // Foreign Key to the User
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
