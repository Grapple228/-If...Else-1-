using Database;
using Database.Entities;
using Database.Misc;

namespace WebApi.Repositories.AnimalTypes;

public class AnimalTypeRepository : IntRepository<AnimalType>
{
    public AnimalTypeRepository(ApiDbContext dbContext) : base(dbContext)
    {
    }
}