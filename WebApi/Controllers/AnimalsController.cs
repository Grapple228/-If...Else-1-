using System.Linq.Expressions;
using Database.Entities;
using Database.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.EntityDtos.Animals;
using WebApi.EntityDtos.Types;
using WebApi.Misc;
using WebApi.Misc.Authentication;
using WebApi.Repositories.Accounts;
using WebApi.Repositories.Animals;
using WebApi.Repositories.AnimalTypes;
using WebApi.Repositories.Locations;
using WebApi.Repositories.Types;
using WebApi.Repositories.VisitedPoints;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("[Controller]")]
public class AnimalsController : ControllerBase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly AnimalsRepository _animalsRepository;
    private readonly AnimalTypeRepository _animalTypeRepository;
    private readonly PointsRepository _pointsRepository;
    private readonly TypeRepository _typeRepository;
    private readonly VisitedPointRepository _visitedPointRepository;

    public AnimalsController(AnimalsRepository animalsRepository, AnimalTypeRepository animalTypeRepository,
        IAccountsRepository accountsRepository, PointsRepository pointsRepository,
        VisitedPointRepository visitedPointRepository, TypeRepository typeRepository)
    {
        _animalsRepository = animalsRepository;
        _animalTypeRepository = animalTypeRepository;
        _accountsRepository = accountsRepository;
        _pointsRepository = pointsRepository;
        _visitedPointRepository = visitedPointRepository;
        _typeRepository = typeRepository;
        _animalTypeRepository = animalTypeRepository;
    }

    [HttpGet("{animalId:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<AnimalDto>> GetAnimal([FromHeader] string? authorization, long animalId)
    {
        if (authorization != null && authorization.Contains("Basic"))
            if (await _accountsRepository.CheckAuthorization(Request) == null)
                return Unauthorized();

        if (animalId <= 0) return BadRequest("id должно быть положительным");
        var entity = await _animalsRepository.Get(animalId);
        if (entity == null) return NotFound("Животное с таким id не найдено");

        entity.AnimalTypes =
            _animalTypeRepository.GetAll(x => x.AnimalId == animalId, x => x.TypeId, null, int.MaxValue);
        entity.VisitedPoints = _visitedPointRepository.GetAll(x => x.AnimalId == animalId,
            x => x.DateTimeOfVisitLocationPoint,
            null, int.MaxValue);
        return entity.AsDto();
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<AnimalDto>>> SearchAnimal(
        [FromHeader] string? authorization,
        [FromQuery(Name = "startDateTime")] DateTime? startDateTime,
        [FromQuery(Name = "endDateTime")] DateTime? endDateTime,
        [FromQuery(Name = "chipperId")] int? chipperId,
        [FromQuery(Name = "chippingLocationId")] long? chippingLocationId,
        [FromQuery(Name = "lifeStatus")] string? lifeStatus,
        [FromQuery(Name = "gender")] string? gender,
        [FromQuery(Name = "from")] int? from,
        [FromQuery(Name = "size")] int? size
    )
    {
        if (authorization != null && authorization.Contains("Basic"))
            if (await _accountsRepository.CheckAuthorization(Request) == null)
                return Unauthorized();

        if (from < 0 || size <= 0 || chipperId <= 0 || chippingLocationId <= 0) return BadRequest();
        if (gender != null && !Enum.TryParse<GenderEnum>(gender, out _)) return BadRequest();
        if (lifeStatus != null && !Enum.TryParse<LifeStatusEnum>(lifeStatus, out _)) return BadRequest();

        var filters = new List<Expression<Func<Animal, bool>>>();
        if (chipperId != null)
            filters.Add(x => x.ChipperId == chipperId);
        if (startDateTime != null)
            filters.Add(x => x.ChippingDateTime >= startDateTime);
        if (endDateTime != null)
            filters.Add(x => x.ChippingDateTime <= endDateTime);
        if (chippingLocationId != null)
            filters.Add(x => x.ChippingLocationId == chippingLocationId);
        if (lifeStatus != null)
            filters.Add(x => x.LifeStatus == lifeStatus);
        if (gender != null)
            filters.Add(x => x.Gender == gender);

        Expression<Func<Animal, bool>> filter = animal => true;
        filter = filters.Aggregate(filter, (current, func) => current.And(func));

        var found = _animalsRepository.GetAll(filter,
            x => x.ChippingDateTime, from, size).ToArray();

        var animals = found.Select(x =>
        {
            x.AnimalTypes = _animalTypeRepository.GetAll(a => a.AnimalId == x.Id, a => a.TypeId, null, int.MaxValue);
            x.VisitedPoints = _visitedPointRepository.GetAll(p => p.AnimalId == x.Id,
                p => p.DateTimeOfVisitLocationPoint, null, int.MaxValue);
            return x.AsDto();
        });

        return Ok(animals);
    }

    [HttpPost("")]
    public async Task<ActionResult<AnimalDto>> PostAnimal(AnimalCreateDto dto)
    {
        if (!dto.Check(out var message)) return BadRequest(message);

        if (!dto.animalTypes.Any())
            return BadRequest("Типы животного не указаны");

        var typesList = new List<AType>();
        foreach (var typeId in dto.animalTypes)
        {
            var type = await _typeRepository.Get(typeId);
            if (type == null)
                return NotFound($"Тип с id '{typeId}' не найден");
            typesList.Add(type);
        }

        var chipper = await _accountsRepository.Get(dto.chipperId);
        if (chipper == null) return NotFound("Аккаунта с таким id нет");
        var locationPoint = await _pointsRepository.Get(dto.chippingLocationId);
        if (locationPoint == null) return NotFound("Локации с таким id нет");

        var created = await _animalsRepository.Create(new Animal
        {
            Gender = dto.gender,
            Height = dto.height,
            Length = dto.length,
            Weight = dto.weight,
            ChipperId = chipper.Id,
            ChippingLocationId = locationPoint.Id,
        });

        var animalTypes = typesList.Select(aType => new AnimalType { AnimalId = created.Id, TypeId = aType.Id })
            .ToList();
        for (var i = 0; i < animalTypes.Count; i++)
            animalTypes[i] = await _animalTypeRepository.Create(animalTypes[i]);
        created.AnimalTypes = animalTypes.OrderBy(x => x.TypeId);

        return CreatedAtAction(nameof(GetAnimal), new { animalId = created.Id }, created.AsDto());
    }

    [HttpPut("{animalId:long}")]
    public async Task<ActionResult<AnimalDto>> PutAnimalType([FromBody] AnimalUpdateDto dto, long animalId)
    {
        if (animalId <= 0) return BadRequest("id должно быть положительным");
        if (!dto.Check()) return BadRequest("Некорректные данные");

        var current = await _animalsRepository.Get(x => x.Id == animalId);
        if (current == null) return NotFound("Животное с таким id не найдено");

        Enum.TryParse<LifeStatusEnum>(current.LifeStatus, out var curLifeStatusEnum);
        Enum.TryParse<LifeStatusEnum>(dto.lifeStatus, out var newLifeStatusEnum);
        if (curLifeStatusEnum == LifeStatusEnum.DEAD && newLifeStatusEnum == LifeStatusEnum.ALIVE)
            return BadRequest("Попытка установки статуса Живой для животного со статусом Мертв");

        var visitedLocations = _visitedPointRepository.GetAll(x => x.AnimalId == animalId,
            x => x.DateTimeOfVisitLocationPoint, null, int.MaxValue);
        if (visitedLocations.Count() != 0 && visitedLocations.First().LocationId == dto.chippingLocationId)
            return BadRequest("Точка чипирования совпадает с первой посещенной точкой локации");

        var chipper = await _accountsRepository.Get(dto.chipperId);
        if (chipper == null) return NotFound("Аккаунта с таким id нет");
        var location = await _pointsRepository.Get(dto.chippingLocationId);
        if (location == null) return NotFound("Локации с таким id нет");

        current.Gender = dto.gender;
        current.LifeStatus = dto.lifeStatus;
        current.Height = dto.height;
        current.Length = dto.length;
        current.Weight = dto.weight;
        current.ChipperId = chipper.Id;
        current.ChippingLocationId = location.Id;

        var updated = await _animalsRepository.Update(animalId, current);

        updated.AnimalTypes =
            _animalTypeRepository.GetAll(x => x.AnimalId == current.Id, type => type.TypeId, null, int.MaxValue);
        updated.VisitedPoints = _visitedPointRepository.GetAll(x => x.AnimalId == current.Id,
            point => point.DateTimeOfVisitLocationPoint,
            null, int.MaxValue);

        return Ok(updated.AsDto());
    }

    [HttpDelete("{animalId:long}")]
    public async Task<ActionResult> DeleteAnimal(long animalId)
    {
        if (animalId <= 0) return BadRequest("id должно быть положительным");

        var animal = await _animalsRepository.Get(x => x.Id == animalId);
        if (animal == null)
            return NotFound("Животное с таким id не найдено");

        var visited = _visitedPointRepository.GetAll(x => x.AnimalId == animalId, x => x.DateTimeOfVisitLocationPoint,
            null, int.MaxValue);

        if (visited.Count() != 0)
            return BadRequest("Животное покинуло точку локации, при этом имеются другие посещенные точки");

        await _animalsRepository.Delete(animalId);

        return Ok();
    }

    [HttpPost("{animalId:long}/types/{typeId:long}")]
    public async Task<ActionResult<AnimalDto>> AddTypeToAnimal(long animalId, long typeId)
    {
        if (animalId <= 0 || typeId <= 0) return BadRequest("Параметры должны быть больше 0");
        var type = await _typeRepository.Get(typeId);
        if (type == null) return NotFound("Тип с таким id не найден");
        var animal = await _animalsRepository.Get(animalId);
        if (animal == null) return NotFound("Животное с таким id не найдено");

        var types = _animalTypeRepository.GetAll(x => x.AnimalId == animal.Id, t => t.TypeId, null, int.MaxValue)
            .ToList();

        if (types.SingleOrDefault(x => x.TypeId == typeId) != null)
            return Conflict("У животного уже имеется этот тип");

        var created =
            await _animalTypeRepository.Create(new AnimalType { AnimalId = animal.Id, TypeId = type.Id });

        types.Add(created);

        animal.AnimalTypes = types;

        return CreatedAtAction(nameof(GetAnimal), new { animalId = animal.Id }, animal.AsDto());
    }

    [HttpPut("{animalId:long}/types")]
    public async Task<ActionResult<AnimalDto>> ReplaceTypeToAnimal(long animalId,
        [FromBody] ATypeReplaceRequest replaceRequest)
    {
        if (animalId <= 0 || replaceRequest.oldTypeId <= 0 || replaceRequest.newTypeId <= 0)
            return BadRequest("Параметры должны быть больше 0");
        var oldType = await _typeRepository.Get(replaceRequest.oldTypeId);
        if (oldType == null) return NotFound("Тип с таким id не найден");

        var newType = await _typeRepository.Get(replaceRequest.newTypeId);
        if (newType == null) return NotFound("Тип с таким id не найден");

        var animal = await _animalsRepository.Get(animalId);
        if (animal == null) return NotFound("Животное с таким id не найдено");

        var types = _animalTypeRepository.GetAll(x => x.AnimalId == animal.Id, type => type.TypeId, null, int.MaxValue)
            .ToList();

        var typeToUpdate = types.SingleOrDefault(x => x.TypeId == replaceRequest.oldTypeId);
        var typeToAdd = types.SingleOrDefault(x => x.TypeId == replaceRequest.newTypeId);

        if (typeToAdd != null && typeToUpdate != null)
            return Conflict("У животного уже имеются новый и старый типы");
        if (typeToUpdate == null)
            return NotFound("У животного отсутствует этот тип");
        if (typeToAdd != null)
            return Conflict("У животного уже имеется этот тип");

        var index = types.IndexOf(typeToUpdate);
        typeToUpdate.TypeId = newType.Id;
        var updated = await _animalTypeRepository.Update(typeToUpdate.Id, typeToUpdate);

        types[index] = updated;
        animal.AnimalTypes = types;
        animal.VisitedPoints = _visitedPointRepository.GetAll(x => x.AnimalId == animalId,
            x => x.DateTimeOfVisitLocationPoint, null, int.MaxValue);

        return animal.AsDto();
    }

    [HttpDelete("{animalId:long}/types/{typeId:long}")]
    public async Task<ActionResult<AnimalDto>> DeleteTypeFromAnimal(long animalId, long typeId)
    {
        if (animalId <= 0 || typeId <= 0) return BadRequest("Параметры должны быть больше 0");
        var type = await _typeRepository.Get(typeId);
        if (type == null) return NotFound("Тип с таким id не найден");
        var animal = await _animalsRepository.Get(animalId);
        if (animal == null) return NotFound("Животное с таким id не найдено");
        var types = _animalTypeRepository.GetAll(x => x.AnimalId == animal.Id, t => t.TypeId, null, int.MaxValue)
            .ToList();
        if (types.SingleOrDefault(x => x.TypeId == typeId) == null)
            return NotFound("У животного нет такого типа");
        if (types.Count == 1 && types[0].TypeId == typeId)
            return BadRequest("У животного один тип, его нельзя удалить");
        var itemToRemove = types.Single(x => x.TypeId == typeId);
        await _animalTypeRepository.Delete(itemToRemove.Id);
        types.Remove(itemToRemove);

        animal.AnimalTypes = types;

        return animal.AsDto();
    }
}