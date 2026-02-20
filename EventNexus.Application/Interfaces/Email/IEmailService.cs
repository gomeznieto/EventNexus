using EventNexus.Application.DTOs;

namespace EventNexus.Application.Interfaces;

public interface IEmailService{
    public Task SendEmailAsync(EmailDetailsDto dto);
}
