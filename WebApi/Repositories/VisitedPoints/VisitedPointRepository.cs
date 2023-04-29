using Database;
using Database.Entities;
using Database.Misc;

namespace WebApi.Repositories.VisitedPoints;

public class VisitedPointRepository : LongRepository<AnimalVisitedPoint>
{
    public VisitedPointRepository(ApiDbContext dbContext) : base(dbContext)
    {
    }
}