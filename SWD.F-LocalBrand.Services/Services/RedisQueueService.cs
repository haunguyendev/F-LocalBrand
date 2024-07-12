using Newtonsoft.Json;
using SWD.F_LocalBrand.Business.DTO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Services
{
    public class RedisQueueService
    {
        private readonly ConcurrentQueue<OrderQueueItem> _queue;

        public RedisQueueService()
        {
            _queue = new ConcurrentQueue<OrderQueueItem>();
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
    }
}
