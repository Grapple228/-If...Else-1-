using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.EntityDtos.Accounts;
using WebApi.Misc.Authentication;
using WebApi.Repositories.Accounts;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("")]
public class AuthenticationController : ControllerBase
{
    private readonly IAccountsRepository _accountsRepository;

    public AuthenticationController(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
    }

    [AllowAnonymous]
    [HttpPost("Registration")]
    public async Task<ActionResult<AccountDto>> Registration([FromHeader] string? authorization,
        [FromBody] AccountCreateDto dto)
    {
        if (authorization != null && authorization.Contains("Basic"))
            if (await _accountsRepository.CheckAuthorization(Request) != null)
                return Forbid();

        if (!dto.Check()) return BadRequest("Некорректные данные");

        var normEmail = dto.email.ToLower();

        if (await _accountsRepository.Get(x => x.Email == normEmail) != null)
            return Conflict("Пользователь с такой почтой уже зарегистрирован");

        var created = await _accountsRepository.Create(new Account
        {
            Email = normEmail,
            Password = dto.password,
            FirstName = dto.firstName,
            LastName = dto.lastName
        });


        var routeValues = new
        {
            action = nameof(AccountsController.GetAccount),
            controller = "Accounts",
            accountId = created.Id
        };

        return CreatedAtRoute(routeValues, created.AsDto());
    }
}