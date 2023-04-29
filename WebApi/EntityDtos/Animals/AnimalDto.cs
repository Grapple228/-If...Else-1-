namespace WebApi.EntityDtos.Animals;

public record AnimalDto(
    long id,
    IEnumerable<long> animalTypes,
    float weight,
    float length,
    float height,
    string gender,
    string lifeStatus,
    DateTime chippingDateTime,
    int chipperId,
    long chippingLocationId,
    IEnumerable<long> visitedLocations,
    DateTime? deathDateTime = null
);