using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Entities;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EventNexus.Application.Mapping;
using EventNexus.Domain.Enums;

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
    
    // -- CREATE ORDER -- //
    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto dto, Guid userId)
    {
        // Valid Event exist
        var eventToAttend = await _dbContext.Events.FirstOrDefaultAsync( e => e.Id == dto.EventId);

        if(eventToAttend is null )
            throw new KeyNotFoundException("The event you are looking for does not exist!");

        // Validate the event is active
        if (eventToAttend.Status != EventStatus.Active)
            throw new InvalidOperationException($"This event is currently {eventToAttend.Status} and is not accepting ticket sales.");


        // Validate User does already have open tickets
        if(await _dbContext.Orders.AnyAsync(o => o.UserId == userId && o.Status == TicketStatus.PendingPayment))
            throw new ArgumentException("You already have an order pending payment. Please complete or cancel it first.");

        // Start transaction
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try{
            // Validate quantity
            var rowsUpdated = await _dbContext.Events
                .Where(e => e.Id == dto.EventId && e.AvailableTickets >= dto.Quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(
                            e => e.AvailableTickets, 
                            e => e.AvailableTickets - dto.Quantity));

            if (rowsUpdated == 0)
                throw new InvalidOperationException("Tickets are sold out, or there are not enough tickets for your request.");

            // Create the Order & Tickets
            var newOrder = new Order {
                TotalAmount = eventToAttend.Price * dto.Quantity,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                UserId = userId,
                EventId = dto.EventId,
                Tickets = new List<Ticket>()
            };

            for(int i = 0; i < dto.Quantity; i++){
                newOrder.Tickets.Add(new Ticket());
            }

            _dbContext.Orders.Add(newOrder);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return newOrder.ToResponseDto();

        } catch(Exception){
            await transaction.RollbackAsync();
            throw;
        }
    }
}
