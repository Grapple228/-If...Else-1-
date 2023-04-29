using System.Net.Mail;
using WebApi.Repositories.Accounts;
using WebApi.Repositories.Animals;
using WebApi.Repositories.AnimalTypes;
using WebApi.Repositories.Locations;
using WebApi.Repositories.Types;
using WebApi.Repositories.VisitedPoints;

namespace WebApi.Misc;

public static class Extensions
{
    public static IServiceCollection AddScopes(this IServiceCollection services)
    {
        services.AddScoped<IAccountsRepository, AccountsRepository>();
        services.AddScoped<AnimalsRepository>();
        services.AddScoped<AnimalTypeRepository>();
        services.AddScoped<PointsRepository>();
        services.AddScoped<VisitedPointRepository>();
        services.AddScoped<TypeRepository>();
        return services;
    }
}