using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.DTOs;

public class VerificationCodeDto {
    [Required(ErrorMessage = "El campo {0} es obligatorio")]
    public string Code { get; set; } = string.Empty;
}
