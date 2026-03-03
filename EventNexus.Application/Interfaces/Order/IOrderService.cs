using EventNexus.Application.DTOs;

namespace EventNexus.Application.Interfaces;

public interface IOrderService{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto dto, Guid userId, string userEmail);
    Task FullFillOrderAync(string orderId);
}
