using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.DataAccess;
using SWD.F_LocalBrand.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Data.Repositories
{
    public class OrderRepository : RepositoryBaseAsync<Order>, IOrderRepository
    {
        public OrderRepository(SwdFlocalBrandContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Order>?> FindOrderAsync(System.Linq.Expressions.Expression<Func<Order, bool>> predicate)
        {
           return await _dbContext.Orders.Include(x=>x.OrderHistories).Include(x=>x.OrderDetails).ThenInclude(x=>x.Product).Where(predicate).ToListAsync();
        }

        public Task<Order?> GetCartByCustomerId(int customerId)
        {
            return _dbContext.Orders.Include(x => x.OrderDetails)
                .ThenInclude(x=>x.Product)
                .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.OrderStatus.Equals("Cart"));
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
             return await _dbContext.Orders.Include(x=>x.OrderHistories)
                .Include(x=>x.OrderDetails)
                .ThenInclude(x=>x.Product)
                .FirstOrDefaultAsync(x=>x.Id==orderId);
        }

        
    }
}
