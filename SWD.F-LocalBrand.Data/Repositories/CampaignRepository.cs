using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using SWD.F_LocalBrand.Data.DataAccess;
using SWD.F_LocalBrand.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Data.Repositories
{
    public class CampaignRepository : RepositoryBaseAsync<Campaign>,ICampaignRepository
    {
        public CampaignRepository(SwdFlocalBrandContext dbContext) : base(dbContext)
        {
        }

        public async Task<bool> AnyAsync(Expression<Func<Campaign, bool>> predicate)
        {
            return await _dbContext.Campaigns.AnyAsync(predicate);
        }
    }
}
