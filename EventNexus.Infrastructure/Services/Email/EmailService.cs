using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;

namespace EventNexus.Infrastructure.Data;

public class EmailService : IEmailService
{
    public Task SendEmailAsync(EmailDetailsDto dto)
    {
        throw new NotImplementedException();
    }
}
