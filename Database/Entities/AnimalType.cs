using Database.Interfaces;

namespace Database.Entities;

public class AnimalType : IIntEntity
{
    public long AnimalId { get; set; }
    public long TypeId { get; set; }
    public int Id { get; init; }
}