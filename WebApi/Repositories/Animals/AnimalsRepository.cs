using Database;
using Database.Entities;
using Database.Misc;

namespace WebApi.Repositories.Animals;

public class AnimalsRepository : LongRepository<Animal>
{
    public AnimalsRepository(ApiDbContext dbContext) : base(dbContext)
    {
    }
}