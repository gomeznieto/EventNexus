using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;

namespace EventNexus.Infrastructure.Services;

public class MockEmailService : IEmailService
{
    public async Task SendEmailAsync(EmailDetailsDto dto)
    {
        Console.WriteLine("-----------------------------------");
        Console.WriteLine($"Enviando email a: {dto.Destination}"); 
        Console.WriteLine($"Asunto: {dto.Subject}"); 
        Console.WriteLine($"Mensaje: {dto.Body}"); 
        Console.WriteLine("-----------------------------------");
    }
}
