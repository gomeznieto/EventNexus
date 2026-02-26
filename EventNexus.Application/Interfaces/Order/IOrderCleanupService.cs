namespace EventNexus.Application.Interfaces;

public interface IOrderCleanupService{
    public Task CancelExpiresOrderAsync(CancellationToken cancellationToken);
}
