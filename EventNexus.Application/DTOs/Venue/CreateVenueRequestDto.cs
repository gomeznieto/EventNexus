using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.DTOs;

public class CreateVenueRequestDto{

    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Address { get; set; } = string.Empty;
    [Required]
    public double Latitude { get; set; }
    [Required]
    public double Longitude { get; set; }
}
