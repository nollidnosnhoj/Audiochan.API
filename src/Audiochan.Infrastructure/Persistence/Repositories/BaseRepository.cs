using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Audiochan.Infrastructure.Persistence.Repositories
{
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<TEntity> _dbSet;
        protected readonly IMapper _mapper;
        
        protected abstract IQueryable<TEntity> BaseQuery { get; }

        public BaseRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _dbSet = _dbContext.Set<TEntity>();
        }

        public virtual async Task<TDto> SingleOrDefaultAsync<TDto>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            Expression<Func<TEntity, bool>> @where = null, 
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Expression<Func<TEntity, TDto>> @select = null,
            bool asNoTracking = true,
            CancellationToken cancellationToken = default)
        {
            var queryable = BuildQueryable(include, where, select, orderBy, asNoTracking);
            return await queryable.SingleOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<TDto> FirstOrDefaultAsync<TDto>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            Expression<Func<TEntity, bool>> @where = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Expression<Func<TEntity, TDto>> @select = null,
            bool asNoTracking = true,
            CancellationToken cancellationToken = default)
        {
            var queryable = BuildQueryable(include, where, select, orderBy, asNoTracking);
            return await queryable.FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(expression, cancellationToken);
        }

        public virtual async Task<List<TDto>> ToListAsync<TDto>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            Expression<Func<TEntity, bool>> @where = null, 
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Expression<Func<TEntity, TDto>> @select = null,
            bool asNoTracking = true,
            CancellationToken cancellationToken = default)
        {
            var queryable = BuildQueryable(include, where, select, orderBy, asNoTracking);
            return await queryable.ToListAsync(cancellationToken);
        }

        public virtual async Task<PagedList<TDto>> ToPaginatedListAsync<TDto>(
            PaginationQuery<TDto> paginationQuery,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            Expression<Func<TEntity, bool>> @where = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Expression<Func<TEntity, TDto>> @select = null,
            bool asNoTracking = true,
            CancellationToken cancellationToken = default)
        {
            var queryable = BuildQueryable(include, where, select, orderBy, asNoTracking);
            return await queryable.PaginateAsync(paginationQuery, cancellationToken);
        }

        public virtual async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public virtual async Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public virtual void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        protected virtual IQueryable<TDto> BuildQueryable<TDto>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            Expression<Func<TEntity, bool>> @where = null,
            Expression<Func<TEntity, TDto>> @select = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool asNoTracking = true)
        {
            var queryable = BaseQuery;
            if (asNoTracking) queryable = queryable.AsNoTracking();
            include?.Invoke(queryable);
            if (where != null) queryable = queryable.Where(where);
            orderBy?.Invoke(queryable);

            return @select is not null 
                ? queryable.Select(@select) 
                : queryable.ProjectTo<TDto>(_mapper.ConfigurationProvider);
        }
    }
}