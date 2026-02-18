namespace EventNexus.Domain.Enums;

public enum EventStatus
{
    Draft,      // Borrador (Nadie lo ve)
    Active,     // Publicado y vendiendo
    SoldOut,    // Agotado
    Cancelled,  // Cancelado por el organizador
    Completed   // Ya pasó
}
