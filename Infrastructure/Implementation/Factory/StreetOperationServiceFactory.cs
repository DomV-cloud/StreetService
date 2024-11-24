using Application.Interfaces.Service;
using Domain.Entities;
using Infrastructure.Implementation.Service;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;

namespace Infrastructure.Implementation.Factory
{
    public class StreetOperationServiceFactory : IStreetOperationService
    {
        private readonly FeatureFlags _featureFlags;
        private readonly IStreetOperationService _postGISService;
        private readonly IStreetOperationService _algorithmicService;

        public StreetOperationServiceFactory(
            IOptions<FeatureFlags> featureFlags,
            PostGISStreetOperationService postGISService,
            AlgorithmicStreetOperationService algorithmicService)
        {
            _featureFlags = featureFlags.Value;
            _postGISService = postGISService;
            _algorithmicService = algorithmicService;
        }

        public Task AddPointToStreetAsync(int streetId, Coordinate newPoint, bool addToEnd)
        {
            if (_featureFlags.UsePostGIS)
            {
                return _postGISService.AddPointToStreetAsync(streetId, newPoint, addToEnd);
            }
            else
            {
                return _algorithmicService.AddPointToStreetAsync(streetId, newPoint, addToEnd);
            }
        }
    }
}
