namespace EventNexus.Application.DTOs;

public class AuthResponseDto{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}
