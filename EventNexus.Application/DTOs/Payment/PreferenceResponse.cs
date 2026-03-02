using System.Text.Json.Serialization;

namespace EventNexus.Application.DTOs;

public class PreferenceResponse
{
    [JsonPropertyName("init_point")]
    public string InitPoint { get; set; } = string.Empty; // Production

    [JsonPropertyName("sandbox_init_point")]
    public string SandboxInitPoint { get; set; } = string.Empty; // Development
}
