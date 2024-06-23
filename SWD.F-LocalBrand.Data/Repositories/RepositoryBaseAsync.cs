using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Data.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SWD.F_LocalBrand.Data.DataAccess;
using SWD.F_LocalBrand.Data.Models;


namespace SWD.F_LocalBrand.Data.Repositories
{
    public class RepositoryBaseAsync<T> : IRepositoryBaseAsync<T> where T : EntityBase
    {
        protected readonly SwdFlocalBrandContext _dbContext;
        
        public RepositoryBaseAsync(SwdFlocalBrandContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
          

        }

        public IQueryable<T> FindAll(bool trackChanges = false)
        {
            return !trackChanges ? _dbContext.Set<T>().AsNoTracking() :
                _dbContext.Set<T>();
        }
        public IQueryable<T> FindAll(bool trackChanges, params Expression<Func<T, object>>[] includeProperties)
        {
            var items = FindAll(trackChanges);
            items = includeProperties.Aggregate(items, (current, includeProperty) => current.Include(includeProperty));
            return items;
        }
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false)
        {
            return !trackChanges ? _dbContext.Set<T>().Where(expression).AsNoTracking()
              : _dbContext.Set<T>().Where(expression);
        }
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties)
        {
            var items = FindByCondition(expression, trackChanges);
            items = includeProperties.Aggregate(items, (current, includeProperty) => current.Include(includeProperty));
            return items;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await FindByCondition(T => T.Id.Equals(id)).FirstOrDefaultAsync();
        }
        public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includeProperties)
        {
            return await FindByCondition(T => T.Id.Equals(id), trackChanges: false, includeProperties).FirstOrDefaultAsync();
        }
        public Task<IDbContextTransaction> BeginTransactionAsync()
         => _dbContext.Database.BeginTransactionAsync();

        public async Task EndTransactionAsync()
        {
            await _dbContext.SaveChangesAsync();
            await _dbContext.Database.CommitTransactionAsync();
        }
        public async Task RollbackTransactionAsync()
        {
            await _dbContext.Database.RollbackTransactionAsync();
        }

        public async Task<int> CreateAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            return entity.Id;
        }
        public async Task<IList<int>> CreateListAsync(IEnumerable<T> entities)
        {
            await _dbContext.Set<T>().AddRangeAsync(entities);
            return entities.Select(x => x.Id).ToList();
        }
        public Task UpdateAsync(T entity)
        {
            if (_dbContext.Entry(entity).State == EntityState.Unchanged) return Task.CompletedTask;
            T exist = _dbContext.Set<T>().Find(entity.Id);
            _dbContext.Entry(exist).CurrentValues.SetValues(entity);
            return Task.CompletedTask;
        }
        public async Task UpdateListAsync(IEnumerable<T> entities)
        => await _dbContext.Set<T>().AddRangeAsync(entities);



        public Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteListAsync(IEnumerable<T> entities)
        {
            _dbContext.Set<T>().RemoveRange(entities);
            return Task.CompletedTask;
        }

        
    }
}
