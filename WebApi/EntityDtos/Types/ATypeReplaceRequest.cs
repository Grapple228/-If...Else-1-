using System.ComponentModel.DataAnnotations;

namespace WebApi.EntityDtos.Types;

public record ATypeReplaceRequest([Required] long oldTypeId, [Required] long newTypeId);