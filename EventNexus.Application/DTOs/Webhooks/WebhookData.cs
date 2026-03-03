using System.Text.Json.Serialization;

namespace EventNexus.Application.DTOs;

public class WebhookData {
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}
