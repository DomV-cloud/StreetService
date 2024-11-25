using Application.Interfaces.Service;
using Domain.Entities;
using Infrastructure.Implementation.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using System.Threading.Tasks;

namespace Infrastructure.Implementation.Factory
{
    public class StreetOperationServiceFactory : IStreetOperationService
    {
        private readonly FeatureFlags _featureFlags;
        private readonly IServiceProvider _serviceProvider;

        public StreetOperationServiceFactory(
            IOptions<FeatureFlags> featureFlags,
            IServiceProvider serviceProvider)
        {
            _featureFlags = featureFlags.Value;
            _serviceProvider = serviceProvider;
        }

        public async Task AddPointToStreetAsync(int streetId, Coordinate newPoint, bool addToEnd)
        {
            IStreetOperationService service = null;

            if (_featureFlags.UsePostGIS)
            {
                service = _serviceProvider.GetService<PostGISStreetOperationService>();
            }
            else
            {
                service = _serviceProvider.GetService<AlgorithmicStreetOperationService>();
            }

            if (service == null)
            {
                throw new InvalidOperationException("Required service for street operation not found.");
            }

            await service.AddPointToStreetAsync(streetId, newPoint, addToEnd);
        }
    }
}
