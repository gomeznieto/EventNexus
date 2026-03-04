using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.DTOs;

public class AuthorizeEmailChangeDto{
    [Required(ErrorMessage = "El campo {0} es obligatorio")]
    public string VerificationCode { get; set; } = string.Empty;
    [Required(ErrorMessage = "El campo {0} es obligatorio")]
    public string NewEmail { get; set; } = string.Empty;
}
