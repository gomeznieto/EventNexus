using System.Text.Json.Serialization;
using EventNexus.Domain.Enums;

namespace EventNexus.Application.DTOs;

public class PaymentDetailDto{
    [JsonPropertyName("status")]
    public PaymentStatus Status { get; set; }

    [JsonPropertyName("external_reference")]
    public string ExternalReference { get; set; } = string.Empty;
}
