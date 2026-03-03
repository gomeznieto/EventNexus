using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Entities;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EventNexus.Application.Mapping;
using EventNexus.Domain.Enums;
using System.Runtime.InteropServices;

namespace EventNexus.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly IMercadoPagoService _mercadoPagoService;

    public OrderService(
            UserManager<IdentityUser> userManager,
            AppDbContext appDbContext,
            IMercadoPagoService mercadoPagoService
            )
    {
        _userManager = userManager;
        _dbContext = appDbContext;
        _mercadoPagoService = mercadoPagoService;
    }
    
    // -- CREATE ORDER -- //
    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto dto, Guid userId, string userEmail)
    {
        // Valid Event exist
        var eventToAttend = await _dbContext.Events.
            FirstOrDefaultAsync( e => e.Id == dto.EventId);

        if(eventToAttend is null )
            throw new KeyNotFoundException("The event you are looking for does not exist!");

        // Validate the event is active
        if (eventToAttend.Status != EventStatus.Active)
            throw new InvalidOperationException($"This event is currently {eventToAttend.Status} and is not accepting ticket sales.");


        // Validate User does already have open tickets to the same Event
        if(await _dbContext.Orders
                .AnyAsync(o => 
                    o.UserId == userId && 
                    o.EventId == dto.EventId &&
                    o.Status == TicketStatus.PendingPayment)
                )
            throw new ArgumentException("You already have an order pending payment. Please complete or cancel it first.");

        // Create the Order & Tickets in memory
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

        // Start transaction
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try{
            // Validate quantity
            var rowsUpdated = await _dbContext.Events
                .Where(e => 
                        e.Id == dto.EventId &&
                        e.AvailableTickets >= dto.Quantity
                      )
                .ExecuteUpdateAsync(s => s.SetProperty(
                            e => e.AvailableTickets, 
                            e => e.AvailableTickets - dto.Quantity));

            if (rowsUpdated == 0)
                throw new InvalidOperationException("Tickets are sold out, or there are not enough tickets for your request.");

            _dbContext.Orders.Add(newOrder);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

        } catch(Exception){
            await transaction.RollbackAsync();
            throw;
        }

        // Send Order to external API
        string checkoutUrl;

        try{
            checkoutUrl = await _mercadoPagoService.CreatePaymentPreferenceAsync(newOrder, userEmail);
        } catch (Exception){
            throw new ExternalException("Payment gateway is down. Prase try again later");
        }

        var responseOrder =  newOrder.ToResponseDto();
        responseOrder.PaymentUrl = checkoutUrl;

        return responseOrder;
    }

    public async Task FullFillOrderAync(string orderId)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == Guid.Parse(orderId));

        if(order is null) 
            throw new KeyNotFoundException("The Order you are looking for does not exist");

        order.Status = TicketStatus.Paid;
        order.PaidAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

}
