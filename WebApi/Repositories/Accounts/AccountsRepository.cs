using Database;
using Database.Entities;
using Database.Misc;

namespace WebApi.Repositories.Accounts;

public class AccountsRepository : IntRepository<Account>, IAccountsRepository
{
    public AccountsRepository(ApiDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Account?> Authenticate(string email, string password)
    {
        return await Get(x => x.Email.ToLower() == email && x.Password == password);
    }
}