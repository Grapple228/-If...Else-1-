using System.Linq.Expressions;
using Database.Entities;
using Database.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.EntityDtos.AnimalVisitedPoints;
using WebApi.Misc;
using WebApi.Misc.Authentication;
using WebApi.Repositories.Accounts;
using WebApi.Repositories.Animals;
using WebApi.Repositories.AnimalTypes;
using WebApi.Repositories.Locations;
using WebApi.Repositories.VisitedPoints;

namespace WebApi.Controllers;

[Authorize]
[Route("animals/{animalId:long}/locations")]
public class AnimalVisitedLocationController : ControllerBase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly AnimalsRepository _animalsRepository;
    private readonly AnimalTypeRepository _animalTypeRepository;
    private readonly PointsRepository _pointsRepository;
    private readonly VisitedPointRepository _visitedPointRepository;

    public AnimalVisitedLocationController(VisitedPointRepository visitedPointRepository,
        AnimalsRepository animalsRepository, PointsRepository pointsRepository,
        AnimalTypeRepository animalTypeRepository, IAccountsRepository accountsRepository)
    {
        _visitedPointRepository = visitedPointRepository;
        _animalsRepository = animalsRepository;
        _pointsRepository = pointsRepository;
        _animalTypeRepository = animalTypeRepository;
        _accountsRepository = accountsRepository;
    }

    [HttpGet("")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<AnimalVisitedPointDto>>> SearchPoint(
        [FromHeader] string? authorization,
        [FromQuery(Name = "startDateTime")] DateTime? startDateTime,
        [FromQuery(Name = "endDateTime")] DateTime? endDateTime,
        [FromQuery(Name = "from")] int? from,
        [FromQuery(Name = "size")] int? size, long animalId)
    {
        if (authorization != null && authorization.Contains("Basic"))
            if (await _accountsRepository.CheckAuthorization(Request) == null)
                return Unauthorized();

        if (from < 0 || size <= 0 || animalId <= 0) return BadRequest("Числа должны быть положительными");

        var animal = await _animalsRepository.Get(animalId);
        if (animal == null) return NotFound("Животное с таким id не найдено");

        var filters = new List<Expression<Func<AnimalVisitedPoint, bool>>>();
        if (startDateTime != null)
            filters.Add(x => x.DateTimeOfVisitLocationPoint >= startDateTime);
        if (endDateTime != null)
            filters.Add(x => x.DateTimeOfVisitLocationPoint <= endDateTime);

        filters.Add(x => x.AnimalId == animalId);

        Expression<Func<AnimalVisitedPoint, bool>> filter = point => true;
        filter = filters.Aggregate(filter, (current, func) => current.And(func));

        return Ok(_visitedPointRepository.GetAll(filter, x => x.DateTimeOfVisitLocationPoint, from, size
        ).Select(x => x.AsDto()));
    }

    [HttpPost("{pointId:long}")]
    public async Task<ActionResult<AnimalVisitedPointDto>> PostVisitedPoint(long pointId, long animalId)
    {
        if (pointId <= 0 || animalId <= 0) return BadRequest("Параметры должны быть положительными");
        var animal = await _animalsRepository.Get(animalId);
        if (animal == null) return NotFound("Животное с таким id не найдено");

        var pointToAdd = await _pointsRepository.Get(pointId);
        if (pointToAdd == null) return NotFound("Точка с таким id не найдена");

        var visitedLocations = _visitedPointRepository.GetAll(x => x.AnimalId == animalId,
            x => x.DateTimeOfVisitLocationPoint, null, int.MaxValue).ToList();

        if (Enum.TryParse<LifeStatusEnum>(animal.LifeStatus, out var status) && status == LifeStatusEnum.DEAD)
            return BadRequest("Попытка добавить точку животному со стастусом DEAD");

        if (!visitedLocations.Any() && pointToAdd.Id == animal.ChippingLocationId)
            return BadRequest(
                "Животное находится в точке чипирования и никуда не перемещалось, попытка добавить точку, равную точке чипирования");

        if (visitedLocations.Count() != 0 && visitedLocations.Last().LocationId == pointId)
            return BadRequest("Попытка добавить точку, в которой животное уже находится");

        var newPoint = await _visitedPointRepository.Create(new AnimalVisitedPoint
        {
            AnimalId = animal.Id,
            LocationId = pointToAdd.Id
        });

        return Created(Tools.GetUrl(Request), newPoint.AsDto());
    }

    [HttpPut("")]
    public async Task<ActionResult<AnimalVisitedPointDto>> PutVisitedPoint(long animalId,
        [FromBody] AnimalVisitedPointUpdateDto dto)
    {
        if (dto.locationPointId <= 0 || dto.visitedLocationPointId <= 0 || animalId <= 0)
            return BadRequest("Параметры должны быть положительными");

        var animal = await _animalsRepository.Get(animalId);
        if (animal == null) return NotFound("Животное с таким id не найдено");

        var visitedPoint = await _visitedPointRepository.Get(x => x.Id == dto.visitedLocationPointId);
        if (visitedPoint == null) return NotFound("Посещенная точка с таким id не найдена");

        var visitedLocations = _visitedPointRepository.GetAll(x => x.AnimalId == animalId,
            x => x.DateTimeOfVisitLocationPoint, null, int.MaxValue).ToList();

        var pointToUpdate = visitedLocations.SingleOrDefault(x => x.Id == visitedPoint.Id);
        if (pointToUpdate == null) return NotFound("У животного нет посещенной точки с таким id");

        var pointToAdd = await _pointsRepository.Get(dto.locationPointId);
        if (pointToAdd == null) return NotFound("Точка с таким id не найдена");

        var pointIndex = visitedLocations.IndexOf(pointToUpdate);
        if (pointIndex == 0 && pointToAdd.Id == animal.ChippingLocationId)
            return BadRequest("Попытка обновить первую посещенную точку на точку чипирования");

        if (pointToAdd.Id == pointToUpdate.LocationId)
            return BadRequest("Обновление точки на такую же точку");

        var visitedPointsCount = visitedLocations.Count();
        if (visitedPointsCount != 1)
        {
            if (pointIndex > 0 && visitedLocations[pointIndex - 1].LocationId == pointToAdd.Id)
                return BadRequest("Попытка обновить точку на точку, совпадающую с предыдущей точкой");
            if (pointIndex < visitedPointsCount - 1 && visitedLocations[pointIndex + 1].LocationId == pointToAdd.Id)
                return BadRequest("Попытка обновить точку на точку, совпадающую со следующей точкой");
        }

        pointToUpdate.LocationId = pointToAdd.Id;

        var updated = await _visitedPointRepository.Update(dto.visitedLocationPointId, pointToUpdate);
        return updated.AsDto();
    }

    [HttpDelete("{visitedPointId:long}")]
    public async Task<ActionResult> DeleteVisitedPoint(long visitedPointId, long animalId)
    {
        if (visitedPointId <= 0 || animalId <= 0) return BadRequest("Параметры должны быть положительными");

        var animal = await _animalsRepository.Get(animalId);
        if (animal == null) return NotFound("Животное с таким id не найдено");

        var visitedPoint = await _visitedPointRepository.Get(x => x.Id == visitedPointId);
        if (visitedPoint == null)
            return NotFound("Посещенная точка с таким id не найдена");

        var visitedLocations = _visitedPointRepository.GetAll(x => x.AnimalId == animalId,
            x => x.DateTimeOfVisitLocationPoint, null, int.MaxValue).ToList();

        var locationToRemove = visitedLocations.SingleOrDefault(x => x.Id == visitedPointId);
        if (locationToRemove == null)
            return NotFound("У животного нет такой посещенной точки");

        var indexOfLocation = visitedLocations.IndexOf(locationToRemove);
        if (visitedLocations.Count() >= 2 && indexOfLocation == 0 &&
            visitedLocations[1].LocationId == animal.ChippingLocationId)
        {
            // Удалить 2 точки
            await _visitedPointRepository.Delete(visitedPointId);
            await _visitedPointRepository.Delete(visitedLocations[1].Id);
        }
        else
        {
            // Удалить 1 точку
            await _visitedPointRepository.Delete(visitedPointId);
        }

        return Ok();
    }
}