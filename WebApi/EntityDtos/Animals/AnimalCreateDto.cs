namespace WebApi.EntityDtos.Animals;

public record AnimalCreateDto(
    IEnumerable<long> animalTypes,
    float weight,
    float length,
    float height,
    string gender,
    int chipperId,
    long chippingLocationId
);