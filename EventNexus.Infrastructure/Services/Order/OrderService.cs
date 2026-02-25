using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace EventNexus.Infrastructure.Services;

public class OrderService : IOrderService
{
        private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _dbContext;

    public OrderService(
            UserManager<IdentityUser> userManager,
            AppDbContext appDbContext
            )
    {
        _userManager = userManager;
        _dbContext = appDbContext;
    }

    public Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto dto, Guid userId)
    {
       throw new NotImplementedException(); 
    }
}
