using Microsoft.Extensions.DependencyInjection;
using OnibusExpress.Infrastructure.Repositories;

namespace OnibusExpress.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IRouteRepository, EfRouteRepository>();
        services.AddScoped<ITripRepository, EfTripRepository>();
        services.AddScoped<IReservationRepository, EfReservationRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
