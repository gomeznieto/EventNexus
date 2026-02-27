using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.Settings;

public sealed class TicketSettings{
    [Range(100000, 999999)]
    public int MinCode { get; set; }
    [Required]
    public int MaxCode { get; set; }
}
