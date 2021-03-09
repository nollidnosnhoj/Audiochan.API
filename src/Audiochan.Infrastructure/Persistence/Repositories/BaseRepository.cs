using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models.Responses;
using Audiochan.Core.Interfaces.Repositories;
using Audiochan.Infrastructure.Persistence.Repositories.Extensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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

        public async Task<TDto> SingleOrDefaultAsync<TDto>(Expression<Func<TEntity, bool>> expression, bool isTracking = true, object mappingParameters = null,
            CancellationToken cancellationToken = default)
        {
            return await BaseQueryable
                .AsTracking(isTracking)
                .Where(expression)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider, mappingParameters)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> expression, Expression<Func<TEntity, object>> orderBy = null, bool isDescending = true, bool isTracking = true,
            CancellationToken cancellationToken = default)
        {
            return await BaseQueryable
                .AsTracking(isTracking)
                .Where(expression)
                .Ordering(orderBy, isDescending)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<TDto>> ListAsync<TDto>(Expression<Func<TEntity, bool>> expression, Expression<Func<TEntity, object>> orderBy = null, bool isDescending = true, bool isTracking = true,
            object mappingParameters = null, CancellationToken cancellationToken = default)
        {
            return await BaseQueryable
                .AsTracking(isTracking)
                .Where(expression)
                .Ordering(orderBy, isDescending)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider, mappingParameters)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedList<TEntity>> PagedListAsync(int page, int size, Expression<Func<TEntity, bool>> expression, Expression<Func<TEntity, object>> orderBy = null, bool isDescending = true,
            bool isTracking = true, CancellationToken cancellationToken = default)
        {
            return await BaseQueryable
                .AsTracking(isTracking)
                .Where(expression)
                .Ordering(orderBy, isDescending)
                .PaginateAsync(page, size, cancellationToken);
        }

        public async Task<PagedList<TDto>> PagedListAsync<TDto>(int page, int size, Expression<Func<TEntity, bool>> expression, Expression<Func<TEntity, object>> orderBy = null,
            bool isDescending = true, bool isTracking = true, object mappingParameters = null,
            CancellationToken cancellationToken = default)
        {
            return await BaseQueryable
                .AsTracking(isTracking)
                .Where(expression)
                .Ordering(orderBy, isDescending)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider, mappingParameters)
                .PaginateAsync(page, size, cancellationToken);
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