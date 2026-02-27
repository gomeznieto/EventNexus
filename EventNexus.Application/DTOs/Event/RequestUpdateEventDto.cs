using System.ComponentModel.DataAnnotations;
using EventNexus.Domain.Enums;

namespace EventNexus.Application.DTOs;

public class RequestUpdateEventDto{
    [Required]
    public int EventId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public ActionType Action { get; set; }
}
