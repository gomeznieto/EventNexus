namespace EventNexus.Domain.Entities;

public class Ticket{
    public int Id { get; set; }

    public Guid OrderId{ get; set; }
    public Order? Order { get; set; }

    public bool IsScanned { get; set; } = false;
    public Guid QrCode { get; set; } = Guid.NewGuid();
}
