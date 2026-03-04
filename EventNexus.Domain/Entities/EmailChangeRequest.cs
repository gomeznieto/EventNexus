using EventNexus.Domain.Enums;

namespace EventNexus.Domain.Entities;

public class EmailChangeRequest{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public string CurrentEmail { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;

    public EmailChangeStatus Status { get; set; } = EmailChangeStatus.PendingConfirmation;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime CompletedAt { get; set; } 

    public User User { get; set; } = null!;
}
