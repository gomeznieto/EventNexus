using EventNexus.Application.Interfaces;
using EventNexus.Domain.Enums;
using EventNexus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventNexus.Infrastructure.Services;

public class OrderCleanupService : IOrderCleanupService
{
    private readonly AppDbContext _dbContext;

    public OrderCleanupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CancelExpiresOrderAsync(CancellationToken cancellationToken)
    {
        var expiredOrders = await _dbContext.Orders
            .Include(o => o.Tickets)
            .Where(o => o.Status == TicketStatus.PendingPayment && o.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (expiredOrders.Any())
        {
            foreach (var order in expiredOrders)
            {
                var quantityToRestore = order.Tickets.Count;

                await _dbContext.Events
                    .Where(e => e.Id == order.EventId)
                    .ExecuteUpdateAsync(s => s.SetProperty(
                                e => e.AvailableTickets, 
                                e => e.AvailableTickets + quantityToRestore), cancellationToken);

                order.Status = TicketStatus.Cancelled;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
