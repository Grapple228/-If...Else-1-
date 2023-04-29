using Database;
using Database.Entities;
using Database.Misc;

namespace WebApi.Repositories.Locations;

public class PointsRepository : LongRepository<Point>
{
    public PointsRepository(ApiDbContext dbContext) : base(dbContext)
    {
    }
}