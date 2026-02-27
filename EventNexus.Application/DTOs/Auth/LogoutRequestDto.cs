using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.DTOs;

public class LogoutRequestDto{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
