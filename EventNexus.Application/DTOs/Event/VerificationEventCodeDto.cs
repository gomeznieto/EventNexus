using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.DTOs;

public class VerificationEventCodeDto{
    [Required]
    public string Code { get; set; } = string.Empty;
    [Required]
    public int EventId { get; set; }
    [Required]
    public Guid UserId { get; set; }
}
