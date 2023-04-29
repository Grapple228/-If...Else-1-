using Database.Interfaces;

namespace Database.Entities;

public class Account : IIntEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int Id { get; init; }
}