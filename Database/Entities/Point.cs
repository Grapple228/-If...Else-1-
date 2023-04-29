using Database.Interfaces;

namespace Database.Entities;

public class Point : ILongEntity
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public long Id { get; init; }
}