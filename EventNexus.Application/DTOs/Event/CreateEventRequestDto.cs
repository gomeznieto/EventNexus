using System.ComponentModel.DataAnnotations;
using EventNexus.Domain.Enums;

namespace EventNexus.Application.DTOs;

public class CreateEventRequestDto{
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    public string? UrlImage { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    [Required]
    public int Capacity { get; set; }
    [Required]
    public decimal Price { get; set; }

    [Required]
    public EventModality Modality { get; set; }

    public int? VenueId { get; set; }
}
