using SWD.F_LocalBrand.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Data.Common.Interfaces
{
    public interface ICampaignRepository :IRepositoryBaseAsync<Campaign>
    {
        Task<bool> AnyAsync(Expression<Func<Campaign, bool>> predicate);
    }
}
