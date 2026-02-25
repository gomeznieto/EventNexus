using EventNexus.Application.DTOs;

namespace EventNexus.Application.Interfaces;

public interface IOrderService{
    public Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto dto, Guid userId);
}
