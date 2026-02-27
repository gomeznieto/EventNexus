using EventNexus.Application.DTOs;

namespace EventNexus.Application.Interfaces;

public interface IEventService{
    public Task<EventResponseDto> CreateAsync(CreateEventRequestDto dto, string userId);
    public Task<EventResponseDto> GetByIdAsync(int id);
    
    public Task<LoginResponseDto> RequestUpdateEventAsync(RequestUpdateEventDto dto);
    public Task<EventResponseDto> PublishEventAsync(VerificationEventCodeDto dto );
    public Task<EventResponseDto> CancelEventAsync(VerificationEventCodeDto dto );
}
