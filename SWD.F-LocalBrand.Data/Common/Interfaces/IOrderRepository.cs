using SWD.F_LocalBrand.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Data.Common.Interfaces
{
    public interface IOrderRepository : IRepositoryBaseAsync<Order>
    {
        Task<Order?> GetCartByCustomerId(int customerId);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>?> FindOrderAsync(Expression<Func<Order, bool>> predicate);


    }
}
