using Database.Entities;

namespace WebApi.EntityDtos.AnimalVisitedPoints;

public static class Extensions
{
    public static AnimalVisitedPointDto AsDto(this AnimalVisitedPoint point)
    {
        return new(point.Id, point.DateTimeOfVisitLocationPoint, point.LocationId);
    }
}