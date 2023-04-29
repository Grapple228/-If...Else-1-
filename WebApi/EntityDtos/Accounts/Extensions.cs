using Database.Entities;
using Database.Misc;

namespace WebApi.EntityDtos.Accounts;

public static class Extensions
{
    public static AccountDto AsDto(this Account account)
    {
        return new(account.Id, account.FirstName, account.LastName, account.Email);
    }

    public static bool Check(this AccountCreateDto dto)
    {
        return dto.email.CheckEmail()
               && dto.password.CheckForNull()
               && dto.firstName.CheckForNull()
               && dto.lastName.CheckForNull();
    }
}