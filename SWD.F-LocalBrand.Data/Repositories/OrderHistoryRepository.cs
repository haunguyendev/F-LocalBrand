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
    public class OrderHistoryRepository : RepositoryBaseAsync<OrderHistory>, IOrderHistoryRepository
    {
        public OrderHistoryRepository(SwdFlocalBrandContext dbContext) : base(dbContext)
        {
        }
    }
}
