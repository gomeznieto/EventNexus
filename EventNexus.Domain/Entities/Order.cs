using EventNexus.Domain.Enums;

namespace EventNexus.Domain.Entities;

public class Order{
    public Guid Id { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.PendingPayment;
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
    public DateTime ExpiresAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = [];
}
