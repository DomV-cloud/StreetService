using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Infrastructure.Implementation.Factory;
using Infrastructure.Implementation.Repository;
using Infrastructure.Implementation.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IStreetRepository, StreetRepository>();

            services.AddTransient<PostGISStreetOperationService>();
            services.AddTransient<AlgorithmicStreetOperationService>();
            services.AddTransient<IStreetOperationService, StreetOperationServiceFactory>();

            return services;
        }
    }
}
