using Database.Entities;
using Database.Interfaces;

namespace WebApi.Repositories.Accounts;

public interface IAccountsRepository : IIntRepository<Account>
{
    public Task<Account?> Authenticate(string email, string password);
}