using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.DTOs;

public class RegisterOrganizerRequestDto{
    [Required(ErrorMessage ="Field {0} is required")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage ="Field {0} is required")]
    public string FirstName { get; set; } = string.Empty;
    [Required(ErrorMessage ="Field {0} is required")]
    public string LastName { get; set; } = string.Empty;
    [Required(ErrorMessage ="Field {0} is required")]
    public string CompanyName { get; set; } = string.Empty;
    public string? BussinessPhone { get; set; }
}
