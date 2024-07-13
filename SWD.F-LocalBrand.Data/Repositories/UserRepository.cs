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
    public class UserRepository : RepositoryBaseAsync<User>, IUserRepository
    {
        public UserRepository(SwdFlocalBrandContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _dbContext.Users.Include(x=>x.Role).ToListAsync();
        }

        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            return await _dbContext.Users.Include(x => x.Role).Where(x => x.Role.RoleName.Equals(role)).ToListAsync();
        }
    }
}
