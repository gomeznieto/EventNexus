using System.ComponentModel.DataAnnotations;

namespace EventNexus.Application.DTOs;

public class ChangeEventPriceDto
{
    [Required(ErrorMessage = "El código de verificación es obligatorio.")]
    public string VerificationCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0, 10000000.00, ErrorMessage = "El precio debe ser igual a 0 y tener un límite razonable.")]
    public decimal Price { get; set; }
}
