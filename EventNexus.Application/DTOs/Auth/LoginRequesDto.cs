using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.DTOs;

public class LoginRequestDto{
    [Required(ErrorMessage = "El campo {0} es obligatorio")]
    public string Email { get; set; } = string.Empty;
}
