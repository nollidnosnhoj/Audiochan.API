using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Common.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Infrastructure.Persistence.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TDto> SingleOrDefaultAsync<TDto>(
            Expression<Func<TEntity, bool>> wherePredicate = null,
            Expression<Func<TEntity, TDto>> selectPredicate = null,
            CancellationToken cancellationToken = default);
        
        Task<PagedList<TDto>> ToPaginatedListAsync<TDto>(
            PaginationQuery<TDto> query, 
            Expression<Func<TEntity, bool>> wherePredicate = null, 
            Expression<Func<TEntity, TDto>> selectPredicate = null,
            CancellationToken cancellationToken = default);

        Task AddAsync(TEntity entity);
        
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        void Update(TEntity entity);

        void Remove(TEntity entity);

        void RemoveRange(IEnumerable<TEntity> entities);
    }
    
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext DbContext;
        protected readonly DbSet<TEntity> DbSet;
        protected readonly IMapper _mapper;

        public BaseRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            DbContext = dbContext;
            _mapper = mapper;
            DbSet = dbContext.Set<TEntity>();
        }
        
        /**
         * Note: Added a parameter for the select predicate since not all entities are mapped to a DTO using
         * Automapper. Some mapping requires a dependency, like a user id of a current user.
         * I know there are ways to add dependency into the mapping profile, but, unless I do not know, it will not
         * work when projecting the IQueryable.
         */
        
        public virtual async Task<TDto> SingleOrDefaultAsync<TDto>(
            Expression<Func<TEntity, bool>> wherePredicate = null,
            Expression<Func<TEntity, TDto>> selectPredicate = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> entityQueryable = DbSet;
            
            if (wherePredicate is not null)
                entityQueryable = entityQueryable.Where(wherePredicate);
            
            var dtoQueryable = selectPredicate is not null
                ? entityQueryable.Select(selectPredicate)
                : entityQueryable.ProjectTo<TDto>(_mapper.ConfigurationProvider);

            return await dtoQueryable.SingleOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<PagedList<TDto>> ToPaginatedListAsync<TDto>(
            PaginationQuery<TDto> query,
            Expression<Func<TEntity, bool>> wherePredicate = null,
            Expression<Func<TEntity, TDto>> selectPredicate = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> entityQueryable = DbSet;
            
            if (wherePredicate is not null)
                entityQueryable = entityQueryable.Where(wherePredicate);

            var dtoQueryable = selectPredicate is not null
                ? entityQueryable.Select(selectPredicate)
                : entityQueryable.ProjectTo<TDto>(_mapper.ConfigurationProvider);

            return await dtoQueryable.PaginateAsync(query, cancellationToken);
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await DbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await DbSet.AddRangeAsync(entities);
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        public virtual void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            DbSet.RemoveRange(entities);
        }
    }
}