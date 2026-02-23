namespace EventNexus.Application.DTOs;

public class RefreshTokenRequestDto{
    public string ExpiredToken { get; set; } = null!;
    public string RefressToken { get; set; } = null!;
}
