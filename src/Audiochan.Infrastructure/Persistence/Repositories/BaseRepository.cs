using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Infrastructure.Persistence.Repositories
{
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext Context;
        protected readonly IMapper Mapper;

        protected BaseRepository(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }

        protected abstract IQueryable<TEntity> BaseQueryable { get; }

        public async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> expression,
            bool isTracking = true,
            CancellationToken cancellationToken = default)
        {
            var queryable = !isTracking ? BaseQueryable.AsNoTracking() : BaseQueryable;

            return await queryable
                .Where(expression)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression,
            CancellationToken cancellationToken = default)
        {
            return await BaseQueryable.AsNoTracking().AnyAsync(expression, cancellationToken);
        }

        public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Context.Set<TEntity>().Update(entity);
            await Context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Context.Set<TEntity>().Remove(entity);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}