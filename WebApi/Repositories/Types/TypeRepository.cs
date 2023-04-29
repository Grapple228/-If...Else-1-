using Database;
using Database.Entities;
using Database.Misc;

namespace WebApi.Repositories.Types;

public class TypeRepository : LongRepository<AType>
{
    public TypeRepository(ApiDbContext dbContext) : base(dbContext)
    {
    }
}