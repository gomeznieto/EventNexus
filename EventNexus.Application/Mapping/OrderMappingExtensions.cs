using EventNexus.Application.DTOs;
using EventNexus.Domain.Entities;

namespace EventNexus.Application.Mapping;

public static class OrderMappingExtensions {

    public static OrderResponseDto ToResponseDto(this Order entity){
        return new OrderResponseDto{
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            ExpiresAt = entity.ExpiresAt,
            Status = entity.Status,
            TotalAmountPaid = entity.TotalAmount,
            PaidAt = entity.PaidAt,
            UserId = entity.UserId,

            EventId = entity.EventId,
            EventTitle = entity.Event?.Title ?? string.Empty, 
            EventStartDate = entity.Event?.StartDate ?? DateTime.MinValue
        };
    }
}
