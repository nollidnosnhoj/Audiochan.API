using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Audiochan.Core.Interfaces.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> expression,
            bool isTracking = true,
            CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression,
            CancellationToken cancellationToken = default);
        Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}