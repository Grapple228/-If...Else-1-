using Database.Enums;
using Database.Interfaces;

namespace Database.Entities;

public class Animal : ILongEntity
{
    private string _lifeStatus;

    public Animal()
    {
        LifeStatus = LifeStatusEnum.ALIVE.ToString();
        ChippingDateTime = DateTime.UtcNow;
    }

    public IEnumerable<AnimalType>? AnimalTypes { get; set; }
    public float Weight { get; set; }
    public float Length { get; set; }
    public float Height { get; set; }
    public string Gender { get; set; }

    public string LifeStatus
    {
        get => _lifeStatus;
        set
        {
            _lifeStatus = value;
            Enum.TryParse<LifeStatusEnum>(value, out var status);
            DeathDateTime = status switch
            {
                LifeStatusEnum.ALIVE => null,
                LifeStatusEnum.DEAD => DateTime.Now,
                _ => null
            };
        }
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    public DateTime ChippingDateTime { get; private set; }
    public int ChipperId { get; set; }
    public long ChippingLocationId { get; set; }
    public IEnumerable<AnimalVisitedPoint>? VisitedPoints { get; set; }
    public DateTime? DeathDateTime { get; private set; }
    public long Id { get; init; }
}