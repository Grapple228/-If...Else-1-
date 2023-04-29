using System.Linq.Expressions;

namespace Database.Interfaces;

public interface ILongRepository<TEntity> where TEntity : class, ILongEntity
{
    IQueryable<TEntity> GetAll();

    IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>> sortBy,
        int? skip, int? take);

    Task<TEntity?> Get(long id);
    Task<TEntity?> Get(Expression<Func<TEntity, bool>> filter);

    Task<TEntity> Create(TEntity entity);

    Task<TEntity> Update(long id, TEntity entity);

    Task Delete(long id);
}