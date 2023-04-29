namespace WebApi.EntityDtos.AnimalVisitedPoints;

public record AnimalVisitedPointDto(
    long id,
    DateTime dateTimeOfVisitLocationPoint,
    long locationPointId
);