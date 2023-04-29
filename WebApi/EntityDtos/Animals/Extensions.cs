using Database.Entities;
using Database.Enums;

namespace WebApi.EntityDtos.Animals;

public static class Extensions
{
    public static AnimalDto AsDto(this Animal animal)
    {
        return new(animal.Id,
            animal.AnimalTypes == null ? new List<long>() : animal.AnimalTypes.Select(x => x.TypeId).ToArray(),
            animal.Weight, animal.Length, animal.Height,
            animal.Gender, animal.LifeStatus, animal.ChippingDateTime, animal.ChipperId,
            animal.ChippingLocationId,
            animal.VisitedPoints == null ? new List<long>() : animal.VisitedPoints.Select(x => x.Id).ToArray(),
            animal.DeathDateTime);
    }

    public static bool Check(this AnimalCreateDto dto, out string message)
    {
        if (!dto.animalTypes.Any())
        {
            message = "Количество типов должно быть больше 0";
            return false;
        }

        if (dto.animalTypes.Distinct().Count() != dto.animalTypes.Count())
        {
            message = "Массив типов содержит дубликаты";
            return false;
        }

        if (!dto.animalTypes.All(x => x > 0))
        {
            message = "Идентификаторы должны быть больше 0";
            return false;
        }

        if (dto.weight <= 0)
        {
            message = "Вес должен быть больше 0";
            return false;
        }

        if (dto.length <= 0)
        {
            message = "Длина должна быть больше 0";

            return false;
        }

        if (dto.height <= 0)
        {
            message = "Высота должна быть больше 0";
            return false;
        }

        if (dto.chipperId <= 0 || dto.chippingLocationId <= 0)
        {
            message = "Идентификатор должен быть больше 0";
            return false;
        }

        if (dto.height <= 0)
        {
            message = "Высота должна быть больше 0";
            return false;
        }

        if (!Enum.TryParse<GenderEnum>(dto.gender, out _))
        {
            message = $"Допустимые значения для пола: '{GenderEnum.MALE}', '{GenderEnum.FEMALE}', '{GenderEnum.OTHER}'";
            return false;
        }

        message = "";
        return true;
    }

    public static bool Check(this AnimalUpdateDto dto)
    {
        return dto.weight > 0
               && dto.length > 0
               && dto.height > 0
               && dto.chipperId > 0
               && dto.chippingLocationId > 0
               && Enum.TryParse<LifeStatusEnum>(dto.lifeStatus, out _)
               && Enum.TryParse<GenderEnum>(dto.gender, out _);
    }
}