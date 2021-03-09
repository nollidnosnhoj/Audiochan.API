using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models.Responses;

namespace Audiochan.Core.Interfaces.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> expression,
            bool isTracking = true,
            CancellationToken cancellationToken = default);

        Task<TDto> SingleOrDefaultAsync<TDto>(
            Expression<Func<TEntity, bool>> expression,
            bool isTracking = true,
            object mappingParameters = null,
            CancellationToken cancellationToken = default);

        Task<List<TEntity>> ListAsync(
            Expression<Func<TEntity, bool>> expression,
            Expression<Func<TEntity, object>> orderBy = null,
            bool isDescending = true,
            bool isTracking = true,
            CancellationToken cancellationToken = default);
        
        Task<List<TDto>> ListAsync<TDto>(
            Expression<Func<TEntity, bool>> expression,
            Expression<Func<TEntity, object>> orderBy = null,
            bool isDescending = true,
            bool isTracking = true,
            object mappingParameters = null,
            CancellationToken cancellationToken = default);
        
        Task<PagedList<TEntity>> PagedListAsync(
            int page,
            int size,
            Expression<Func<TEntity, bool>> expression,
            Expression<Func<TEntity, object>> orderBy = null,
            bool isDescending = true,
            bool isTracking = true,
            CancellationToken cancellationToken = default);
        
        Task<PagedList<TDto>> PagedListAsync<TDto>(
            int page,
            int size,
            Expression<Func<TEntity, bool>> expression,
            Expression<Func<TEntity, object>> orderBy = null,
            bool isDescending = true,
            bool isTracking = true,
            object mappingParameters = null,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression,
            CancellationToken cancellationToken = default);

        Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}