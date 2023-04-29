using Database.Interfaces;

namespace Database.Entities;

public class AnimalVisitedPoint : ILongEntity
{
    public AnimalVisitedPoint()
    {
        DateTimeOfVisitLocationPoint = DateTime.UtcNow;
    }

    public long AnimalId { get; set; }
    public DateTime DateTimeOfVisitLocationPoint { get; set; }
    public long LocationId { get; set; }
    public long Id { get; init; }
}