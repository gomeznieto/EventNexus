using EventNexus.Application.DTOs;

namespace EventNexus.Application.Interfaces;

public interface IEventService{
    public Task<DTOs.EventResponseDto> CreateAsync(CreateEventRequestDto dto, string userId);
}
