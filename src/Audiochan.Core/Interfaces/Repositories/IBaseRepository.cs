using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;
using Microsoft.EntityFrameworkCore.Query;

namespace Audiochan.Core.Interfaces.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TDto> SingleOrDefaultAsync<TDto>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            Expression<Func<TEntity, bool>> where = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Expression<Func<TEntity, TDto>> select = null,
            bool asNoTracking = true,
            CancellationToken cancellationToken = default);
        
        Task<TDto> FirstOrDefaultAsync<TDto>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            Expression<Func<TEntity, bool>> where = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Expression<Func<TEntity, TDto>> select = null,
            bool asNoTracking = true,
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
        
        Task<List<TDto>> ToListAsync<TDto>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            Expression<Func<TEntity, bool>> where = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Expression<Func<TEntity, TDto>> select = null,
            bool asNoTracking = true,
            CancellationToken cancellationToken = default);
        
        Task<PagedList<TDto>> ToPaginatedListAsync<TDto>(
            PaginationQuery<TDto> paginationQuery,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            Expression<Func<TEntity, bool>> where = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Expression<Func<TEntity, TDto>> select = null,
            bool asNoTracking = true,
            CancellationToken cancellationToken = default);

        Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
    }
}