using Database.Entities;

namespace WebApi.EntityDtos.Points;

public static class Extensions
{
    public static PointDto AsDto(this Point point)
    {
        return new(point.Id, point.Longitude, point.Latitude);
    }

    public static bool Check(this PointCreateDto dto)
    {
        return dto.latitude is >= -90.0d and <= 90.0d
               && dto.longitude is >= -180.0d and <= 180.0d;
    }
}