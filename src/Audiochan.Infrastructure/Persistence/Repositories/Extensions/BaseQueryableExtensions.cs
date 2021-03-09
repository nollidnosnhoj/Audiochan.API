using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Infrastructure.Persistence.Repositories.Extensions
{
    public static class BaseQueryableExtensions
    {
        public static IQueryable<TEntity> AsTracking<TEntity>(this IQueryable<TEntity> queryable, bool isTracking) where TEntity : class
        {
            return !isTracking ? queryable.AsNoTracking() : queryable;
        }
        
        public static IQueryable<TEntity> Ordering<TEntity>(this IQueryable<TEntity> queryable,
            Expression<Func<TEntity, object>> orderBy, bool isDescending) where TEntity : class
        {
            return isDescending 
                ? queryable.OrderByDescending(orderBy) 
                : queryable.OrderBy(orderBy);
        }
    }
}