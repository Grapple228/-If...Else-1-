namespace WebApi.EntityDtos.AnimalVisitedPoints;

public record AnimalVisitedPointUpdateDto(
    long visitedLocationPointId,
    long locationPointId
);