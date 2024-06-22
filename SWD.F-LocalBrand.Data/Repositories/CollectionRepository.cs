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
    public class CollectionRepository : RepositoryBaseAsync<Collection>, ICollectionRepository
    {
        public CollectionRepository(SwdFlocalBrandContext dbContext) : base(dbContext)
        {
        }

        public async Task<bool> CollectionNameExistsAsync(string collectionName)
        {
            return await _dbContext.Collections.AnyAsync(c => c.CollectionName == collectionName);
        }
    }
}
