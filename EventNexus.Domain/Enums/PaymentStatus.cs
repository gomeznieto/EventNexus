using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace EventNexus.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    [EnumMember(Value = "pending")]
    Pending,

    [EnumMember(Value = "approved")]
    Approved,

    [EnumMember(Value = "authorized")]
    Authorized,

    [EnumMember(Value = "in_process")]
    InProcess,

    [EnumMember(Value = "in_mediation")]
    InMediation,

    [EnumMember(Value = "rejected")]
    Rejected,

    [EnumMember(Value = "cancelled")]
    Cancelled,

    [EnumMember(Value = "refunded")]
    Refunded,

    [EnumMember(Value = "charged_back")]
    ChargedBack
}
