using Microsoft.Extensions.DependencyInjection;

namespace GHCAA.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Keep application-layer registrations here (DTOs, validators, domain services interfaces)
            // Example:
            // services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            return services;
        }
    }
}