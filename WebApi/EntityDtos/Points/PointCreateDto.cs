using System.ComponentModel.DataAnnotations;

namespace WebApi.EntityDtos.Points;

public record PointCreateDto([Required] double? latitude = null, [Required] double? longitude = null);