using System.Linq.Expressions;
using Database.Interfaces;
using Database.Misc;
using Microsoft.EntityFrameworkCore;

namespace Database;

public abstract class LongRepository<TEntity> : ILongRepository<TEntity> where TEntity : class, ILongEntity
{
    private readonly ApiDbContext _dbContext;

    protected LongRepository(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual IQueryable<TEntity> GetAll()
    {
        return _dbContext.Set<TEntity>().AsNoTracking();
    }

    public virtual IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, object>> sortBy,
        int? skip, int? take)
    {
        return _dbContext.Set<TEntity>().AsNoTracking().Where(filter).OrderBy(sortBy).Skip(skip ?? DefaultValues.Skip)
            .Take(take ?? DefaultValues.Take);
    }

    public virtual async Task<TEntity?> Get(long id)
    {
        return await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<TEntity?> Get(Expression<Func<TEntity, bool>> filter)
    {
        return await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(filter);
    }

    public virtual async Task<TEntity> Create(TEntity entity)
    {
        var created = await _dbContext.Set<TEntity>().AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return created.Entity;
    }

    public virtual async Task<TEntity> Update(long id, TEntity entity)
    {
        var updated = _dbContext.Set<TEntity>().Update(entity);
        await _dbContext.SaveChangesAsync();
        return updated.Entity;
    }

    public virtual async Task Delete(long id)
    {
        var entity = await Get(id);
        if (entity == null) return;

        _dbContext.Set<TEntity>().Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}