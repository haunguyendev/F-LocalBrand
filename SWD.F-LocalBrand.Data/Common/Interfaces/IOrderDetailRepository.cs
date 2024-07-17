using SWD.F_LocalBrand.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Data.Common.Interfaces
{
    public interface IOrderDetailRepository : IRepositoryBaseAsync<OrderDetail>
    {
        Task<List<OrderDetail>?> FindOrderDetailAsync(Expression<Func<OrderDetail, bool>> predicate);
    }
}
