using Database.Interfaces;

namespace Database.Entities;

public class AType : ILongEntity
{
    public string Type { get; set; } = null!;
    public long Id { get; init; }
}