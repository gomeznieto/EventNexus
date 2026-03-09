using Resend;
using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
namespace EventNexus.Infrastructure.Services;

public class MockEmailService : IEmailService
{
    private readonly IResend _resend;
    public MockEmailService(IResend resend)
    {
        _resend = resend;
    }

    public async Task SendEmailAsync(EmailDetailsDto dto)
    {
        var message = new EmailMessage {
            From = "onboarding@resend.dev",
            To = dto.Destination,
            Subject = dto.Subject,
            TextBody = dto.Body
        };

        await _resend.EmailSendAsync(message);
    }
}
