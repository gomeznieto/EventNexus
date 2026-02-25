using EventNexus.Application.DTOs;

namespace EventNexus.Application.Interfaces;

public interface IVenueService{
    Task<VenueResponseDto> CreateAsync(CreateVenueRequestDto dto, string userId);
    Task<VenueResponseDto> GetByIdAsync(int id);
}
