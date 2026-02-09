using FluentValidation;
using GHCAA.Application.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace GHCAA.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register validators from this assembly
            services.AddValidatorsFromAssemblyContaining<MemberRegistrationValidator>();

            return services;
        }
    }
}
