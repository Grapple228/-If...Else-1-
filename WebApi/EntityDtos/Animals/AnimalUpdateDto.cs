namespace WebApi.EntityDtos.Animals;

public record AnimalUpdateDto(
    float weight,
    float length,
    float height,
    string gender,
    string lifeStatus,
    int chipperId,
    long chippingLocationId
);