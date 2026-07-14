using Microsoft.Extensions.DependencyInjection;
using OnibusExpress.Application.UseCases.Reservations;
using OnibusExpress.Application.UseCases.Routes;
using OnibusExpress.Application.UseCases.Trips;

namespace OnibusExpress.Application;

/// <summary>
/// Registers the Application layer services. Repositories (Infrastructure) are
/// registered by the API in Phase 3.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IReservationCodeGenerator, ReservationCodeGenerator>();

        services.AddScoped<ListRoutesUseCase>();
        services.AddScoped<SearchTripsUseCase>();
        services.AddScoped<GetTripUseCase>();
        services.AddScoped<CreateReservationUseCase>();
        services.AddScoped<GetReservationUseCase>();
        services.AddScoped<CancelReservationUseCase>();

        return services;
    }
}
