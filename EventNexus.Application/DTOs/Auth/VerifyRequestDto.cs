using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.DTOs;

public class VerifyRequestDto{

    [Required(ErrorMessage = "El campo {0} es obligatorio")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "El campo {0} es obligatorio")]
    public string Code { get; set; } = string.Empty;
}
