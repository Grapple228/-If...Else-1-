using System.Linq.Expressions;

namespace Database.Interfaces;

public interface IIntRepository<TEntity> where TEntity : class, IIntEntity
{
    IQueryable<TEntity> GetAll();

    IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>> sortBy,
        int? skip, int? take);

    Task<TEntity?> Get(int id);
    Task<TEntity?> Get(Expression<Func<TEntity, bool>> filter);

    Task<TEntity> Create(TEntity entity);

    Task<TEntity> Update(int id, TEntity entity);

    Task Delete(int id);
}