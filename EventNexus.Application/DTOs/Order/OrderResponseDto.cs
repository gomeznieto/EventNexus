using EventNexus.Domain.Enums;

namespace EventNexus.Application.DTOs;

public class OrderResponseDto{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
    public DateTime ExpiresAt { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.PendingPayment;
    public decimal TotalAmountPaid { get; set; }
    public DateTime? PaidAt { get; set; }

    public int EventId { get; set; }
    public string EventTitle  { get; set; } = string.Empty;
    public DateTime EventStartDate { get; set; }

    public Guid UserId { get; set; }
}
