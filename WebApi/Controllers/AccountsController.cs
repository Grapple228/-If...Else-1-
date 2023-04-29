using System.Linq.Expressions;
using System.Security.Claims;
using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.EntityDtos.Accounts;
using WebApi.Misc;
using WebApi.Misc.Authentication;
using WebApi.Repositories.Accounts;
using WebApi.Repositories.Animals;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("[Controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly AnimalsRepository _animalsRepository;

    public AccountsController(IAccountsRepository accountsRepository, AnimalsRepository animalsRepository)
    {
        _accountsRepository = accountsRepository;
        _animalsRepository = animalsRepository;
    }

    [HttpGet("{accountId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<AccountDto>> GetAccount([FromHeader] string? authorization, int accountId)
    {
        if (authorization != null && authorization.Contains("Basic"))
            if (await _accountsRepository.CheckAuthorization(Request) == null)
                return Unauthorized();

        if (accountId <= 0) return BadRequest();
        var entity = await _accountsRepository.Get(accountId);
        return entity == null ? NotFound() : entity.AsDto();
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<AccountDto>>> SearchAccount([FromHeader] string? authorization,
        [FromQuery(Name = "firstName")] string? firstName,
        [FromQuery(Name = "lastName")] string? lastName,
        [FromQuery(Name = "email")] string? email,
        [FromQuery(Name = "from")] int? from,
        [FromQuery(Name = "size")] int? size
    )
    {
        if (authorization != null && authorization.Contains("Basic"))
            if (await _accountsRepository.CheckAuthorization(Request) == null)
                return Unauthorized();
        
        if (from < 0 || size <= 0) return BadRequest();

        var filters = new List<Expression<Func<Account, bool>>>();
        if (firstName != null)
            filters.Add(x => x.FirstName.Contains(firstName.ToLower()));
        if (lastName != null)
            filters.Add(x => x.LastName.Contains(lastName.ToLower()));
        if (email != null)
            filters.Add(x => x.Email.Contains(email.ToLower()));

        Expression<Func<Account, bool>> filter = account => true;
        filter = filters.Aggregate(filter, (current, func) => current.And(func));

        return Ok(_accountsRepository.GetAll(filter, x => x.Id, from, size).Select(x => x.AsDto()));
    }

    [HttpPut("{accountId:int}")]
    public async Task<ActionResult<AccountDto>> PutAccount(AccountCreateDto dto, int accountId)
    {
        if (accountId <= 0) return BadRequest("id должно быть положительным");
        if (!dto.Check()) return BadRequest("Некорректные данные");

        var current = await _accountsRepository.Get(x => x.Id == accountId);
        if (current == null) return Forbid();

        var id = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (id == null) return Unauthorized("id не найдено");
        if (!long.TryParse(id, out var idLong) || idLong != accountId) return Forbid();

        var normEmail = dto.email.ToLower();
        var account = await _accountsRepository.Get(x => x.Email == normEmail);
        if (account != null && account.Id != accountId) return Conflict("Аккаунт с такой почтой уже есть");

        current.Email = dto.email;
        current.Password = dto.password;
        current.FirstName = dto.firstName;
        current.LastName = dto.lastName;

        var updated = await _accountsRepository.Update(accountId, current);

        return Ok(updated.AsDto());
    }

    [HttpDelete("{accountId:int}")]
    public async Task<ActionResult> DeleteAccount(int accountId)
    {
        if (accountId <= 0) return BadRequest("id должно быть положительным");

        if (await _accountsRepository.Get(accountId) == null) return Forbid();

        var id = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (id == null) return Unauthorized("id не найдено");
        if (!long.TryParse(id, out var idLong) || idLong != accountId) return Forbid();

        if (_animalsRepository.GetAll(x => x.ChipperId == accountId, x => x.Id, null, null).Count() != 0)
            return BadRequest("Аккаунт связан с животным");

        await _accountsRepository.Delete(accountId);

        return Ok();
    }
}