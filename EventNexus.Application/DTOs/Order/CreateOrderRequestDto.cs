using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.DTOs;

public class CreateOrderRequestDto{
    [Required]
    public int EventId { get; set; }
    [Required]
    [Range(1, 5, ErrorMessage = "You must buy between 1 and 5 tickts")]
    public int Quantity { get; set; }
}
