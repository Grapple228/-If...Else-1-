using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.EntityDtos.Types;
using WebApi.Misc.Authentication;
using WebApi.Repositories.Accounts;
using WebApi.Repositories.Animals;
using WebApi.Repositories.Types;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("Animals/Types")]
public class TypesController : ControllerBase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly AnimalsRepository _animalsRepository;
    private readonly TypeRepository _typeRepository;

    public TypesController(TypeRepository typeRepository, AnimalsRepository animalsRepository,
        IAccountsRepository accountsRepository)
    {
        _typeRepository = typeRepository;
        _animalsRepository = animalsRepository;
        _accountsRepository = accountsRepository;
    }

    [HttpGet("{typeId:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<ATypeDto>> GetAnimalType([FromHeader] string? authorization, long typeId)
    {
        if (authorization != null && authorization.Contains("Basic"))
            if (await _accountsRepository.CheckAuthorization(Request) == null)
                return Unauthorized();

        if (typeId <= 0) return BadRequest();
        var entity = await _typeRepository.Get(typeId);
        return entity == null ? NotFound("Тип с таким id не найден") : entity.AsDto();
    }

    [HttpPut("{typeId:long}")]
    public async Task<ActionResult<ATypeDto>> PutAnimalType([FromBody] ATypeCreateDto dto, long typeId)
    {
        if (!dto.Check()) return BadRequest("Некорректные данные");

        var current = await _typeRepository.Get(x => x.Id == typeId);
        if (current == null) return NotFound("Тип с таким id не найден");

        var normType = dto.type.ToLower();

        if (await _typeRepository.Get(x => x.Type == normType) !=
            null) return Conflict("Такой тип уже есть");

        current.Type = dto.type;

        var updated = await _typeRepository.Update(typeId, current);

        return Ok(updated);
    }

    [HttpPost("")]
    public async Task<ActionResult<ATypeDto>> PostAnimalType([FromBody] ATypeCreateDto dto)
    {
        if (!dto.Check()) return BadRequest("Некорректные параметры");

        var normType = dto.type.ToLower();

        if (await _typeRepository.Get(x => x.Type == normType) != null)
            return Conflict("Такой тип уже есть");

        var created = await _typeRepository.Create(new AType
        {
            Type = normType
        });

        return CreatedAtAction(nameof(GetAnimalType), new { typeId = created.Id }, created.AsDto());
    }

    [HttpDelete("{typeId:long}")]
    public async Task<ActionResult> DeleteAnimalType(long typeId)
    {
        if (typeId <= 0) return BadRequest("id должно быть положительным");

        if (await _typeRepository.Get(typeId) == null)
            return NotFound("Такого типа нет");

        if (_animalsRepository.GetAll(x => x.AnimalTypes.Select(t => t.TypeId).Contains(typeId),
                x => x.Id, null, null).Count() != 0)
            return BadRequest("Есть животные с этим типом");

        await _typeRepository.Delete(typeId);

        return Ok();
    }
}