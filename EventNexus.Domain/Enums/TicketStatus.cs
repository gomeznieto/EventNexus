namespace EventNexus.Domain.Enums;

public enum TicketStatus
{
    PendingPayment, // Reservado (bloquea stock temporalmente)
    Paid,           // Confirmado (tiene QR)
    Used,           // Ya entró al evento
    Refunded,       // Devolución
    Cancelled       // Expiró la reserva
}
