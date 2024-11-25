using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Infrastructure.Implementation.Repository;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace Infrastructure.Implementation.Service
{
    public class AlgorithmicStreetOperationService : IStreetOperationService
    {
        private readonly IStreetRepository _repository;
        private readonly ILogger<AlgorithmicStreetOperationService> _logger;
        private readonly Object _lock = new();

        public AlgorithmicStreetOperationService(IStreetRepository repository, ILogger<AlgorithmicStreetOperationService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task AddPointToStreetAsync(int streetId, Coordinate newPoint, bool addToEnd)
        {
            var retrievedStreet = await _repository.GetStreetByIdAsync(streetId);

            lock (_lock)
            {
                if (retrievedStreet is null)
                {
                    _logger.LogError("Street with ID {StreetId} was not found.", streetId);
                    throw new ArgumentNullException(nameof(retrievedStreet), "Street with ID {StreetId} was not found.");
                }

                Coordinate[] updatedCoordinates;
                if (addToEnd)
                {
                    updatedCoordinates = retrievedStreet.Geometry.Coordinates.Concat(new[] { newPoint }).ToArray();
                }
                else
                {
                    updatedCoordinates = new[] { newPoint }.Concat(retrievedStreet.Geometry.Coordinates).ToArray();
                }

                retrievedStreet.Geometry = new LineString(updatedCoordinates);
            }

            await _repository.UpdateStreetAsync(retrievedStreet);
        }
    }
}
