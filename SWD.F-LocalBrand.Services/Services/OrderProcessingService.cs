using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Services
{
    public class OrderProcessingService : BackgroundService
    {
        private readonly RedisQueueService _queueService;
        private readonly IServiceProvider _serviceProvider;

        public OrderProcessingService(RedisQueueService queueService, IServiceProvider serviceProvider)
        {
            _queueService = queueService;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var order = await _queueService.DequeueOrderAsync();
                if (order != null)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
                        await orderService.ProcessOrderAsync(order);
                    }
                }

                await Task.Delay(1000, stoppingToken); // Thời gian chờ giữa các lần xử lý
            }
        }
    }
}
