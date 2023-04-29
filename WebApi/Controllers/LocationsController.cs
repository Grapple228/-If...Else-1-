using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.EntityDtos.Points;
using WebApi.Misc.Authentication;
using WebApi.Repositories.Accounts;
using WebApi.Repositories.Animals;
using WebApi.Repositories.Locations;
using WebApi.Repositories.VisitedPoints;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("[Controller]")]
public class LocationsController : ControllerBase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly AnimalsRepository _animalsRepository;
    private readonly PointsRepository _pointsRepository;
    private readonly VisitedPointRepository _visitedPointRepository;

    public LocationsController(PointsRepository pointsRepository,
        VisitedPointRepository visitedPointRepository, IAccountsRepository accountsRepository,
        AnimalsRepository animalsRepository)
    {
        _pointsRepository = pointsRepository;
        _visitedPointRepository = visitedPointRepository;
        _accountsRepository = accountsRepository;
        _animalsRepository = animalsRepository;
    }

    [HttpGet("{pointId:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<PointDto>> GetLocation([FromHeader] string? authorization, long pointId)
    {
        if (authorization != null && authorization.Contains("Basic"))
            if (await _accountsRepository.CheckAuthorization(Request) == null)
                return Unauthorized();

        if (pointId <= 0) return BadRequest("id должно быть положительным");
        var entity = await _pointsRepository.Get(pointId);
        return entity == null ? NotFound("Точка с таким id не найдена") : entity.AsDto();
    }

    [HttpPost("")]
    public async Task<ActionResult<PointDto>> PostLocation([FromBody] PointCreateDto dto,
        [FromHeader] string? authorization)
    {
        if (!dto.Check()) return BadRequest("Некорректные параметры");

        if (await _pointsRepository.Get(x => (x.Latitude == dto.latitude) & (x.Longitude == dto.longitude)) !=
            null)
            return Conflict($"Точка с координатами (latitude: {dto.latitude}; longitude: {dto.longitude}) уже имеется");

        var created = await _pointsRepository.Create(new Point
        {
            Latitude = dto.latitude,
            Longitude = dto.longitude
        });

        return CreatedAtAction(nameof(GetLocation), new { pointId = created.Id }, created.AsDto());
    }

    [HttpPut("{pointId:long}")]
    public async Task<ActionResult<PointDto>> PutLocation([FromBody] PointCreateDto dto, long pointId)
    {
        if (pointId <= 0)
            return BadRequest("id должно быть положительным");

        if (!dto.Check()) return BadRequest("Некорректные данные");

        var current = await _pointsRepository.Get(x => x.Id == pointId);
        if (current == null) return NotFound("Точка с таким id не найдена");

        if (await _pointsRepository.Get(x => x.Latitude == dto.latitude && x.Longitude == dto.longitude) !=
            null) return Conflict("Точка с такими координатами уже имеется");

        current.Latitude = dto.latitude;
        current.Longitude = dto.longitude;

        var updated = await _pointsRepository.Update(pointId, current);

        return Ok(updated.AsDto());
    }

    [HttpDelete("{pointId:long}")]
    public async Task<ActionResult> DeleteLocation(long pointId)
    {
        if (pointId <= 0) return BadRequest("id должно быть положительным");

        if (await _pointsRepository.Get(pointId) == null)
            return NotFound("Точка с таким id не найдена");

        if (_visitedPointRepository.GetAll(x => x.LocationId == pointId, x => x.Id, null, null).Count() != 0)
            return BadRequest("Точка связана с животным");

        if (_animalsRepository.GetAll(x => x.ChippingLocationId == pointId, x => x.Id, null, int.MaxValue).Count() != 0)
            return BadRequest("Удаление локации чипирования, связанной с животным");

        await _pointsRepository.Delete(pointId);

        return Ok();
    }
}