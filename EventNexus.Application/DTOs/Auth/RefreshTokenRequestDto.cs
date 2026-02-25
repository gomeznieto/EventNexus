using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.DTOs;

public class RefreshTokenRequestDto{
    [Required]
    public string ExpiredToken { get; set; } = null!;
    [Required]
    public string RefreshToken { get; set; } = null!;
}
