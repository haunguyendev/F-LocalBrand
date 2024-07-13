using Microsoft.Extensions.DependencyInjection;
using SWD.F_LocalBrand.Business.DTO;
using SWD.F_LocalBrand.Business.Services;
using System.Collections.Concurrent;

public class RedisQueueService
{
    private readonly ConcurrentQueue<OrderQueueItem> _queue;
    private readonly IServiceScopeFactory _scopeFactory;

    public RedisQueueService(IServiceScopeFactory scopeFactory)
    {
        _queue = new ConcurrentQueue<OrderQueueItem>();
        _scopeFactory = scopeFactory;
    }

    public Task EnqueueOrderAsync(OrderQueueItem order)
    {
        _queue.Enqueue(order);
        return Task.CompletedTask;
    }

    public Task<OrderQueueItem> DequeueOrderAsync()
    {
        _queue.TryDequeue(out var order);
        return Task.FromResult(order);
    }

    public async Task<(bool Success, string ErrorMessage, string PaymentUrl)> ProcessOrderPaymentAsync(OrderQueueItem orderQueueItem)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var processingWorker = scope.ServiceProvider.GetRequiredService<ProcessingWorker>();
            return await processingWorker.ProcessOrderPaymentAsync(orderQueueItem);
        }
    }
}
