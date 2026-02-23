using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.DTOs;

public class RegisterCustomerRequestDto{
    [Required(ErrorMessage = "El campo {0} es obligatorio")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "El campo {0} es obligatorio")]
    public string FirstName { get; set; } = string.Empty;
    [Required(ErrorMessage = "El campo {0} es obligatorio")]
    public string LastName { get; set; } = string.Empty;
}
