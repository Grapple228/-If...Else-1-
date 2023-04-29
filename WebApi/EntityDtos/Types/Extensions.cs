using Database.Entities;
using Database.Misc;

namespace WebApi.EntityDtos.Types;

public static class Extensions
{
    public static ATypeDto AsDto(this AType type)
    {
        return new(type.Id, type.Type);
    }

    public static bool Check(this ATypeCreateDto dto)
    {
        return dto.type.CheckForNull();
    }
}