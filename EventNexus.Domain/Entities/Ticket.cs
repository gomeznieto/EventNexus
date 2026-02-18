using EventNexus.Domain.Enums;

namespace EventNexus.Domain.Entities;

public class Ticket{
    public int Id { get; set; }

    public DateTime PurchasedDate { get; set; }
    public decimal PricePaid { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.PendingPayment;

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

}
