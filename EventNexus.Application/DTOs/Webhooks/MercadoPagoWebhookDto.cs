using System.Text.Json.Serialization;

namespace EventNexus.Application.DTOs;

public class MercadoPagoWebhookDto{
[JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public WebhookData Data { get; set; } = new();
}
