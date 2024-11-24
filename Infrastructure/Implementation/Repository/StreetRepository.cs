using Application.DatabaseContext;
using Application.Interfaces.Repository;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace Infrastructure.Implementation.Repository
{
    public class StreetRepository : IStreetRepository
    {
        private readonly StreetContext _streetContext;
        private readonly ILogger<StreetRepository> _logger;
        private readonly Object _lock = new();

        public StreetRepository(StreetContext streetContext, ILogger<StreetRepository> logger)
        {
            _streetContext = streetContext ?? throw new ArgumentNullException(nameof(streetContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddPointToStreetAsync(int streetId, Coordinate newPoint, bool addToEnd)
        {
            var retrievedStreet = await GetStreetByIdAsync(streetId);

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

            await UpdateStreetAsync(retrievedStreet);
        }

        public async Task<Street> CreateStreetAsync(Street street)
        {
            if (street is null)
            {
                _logger.LogError("Failed Attempted to create a street.");
                throw new ArgumentNullException(nameof(street), "Street cannot be null.");
            }

            _logger.LogInformation("Adding a new street with name '{StreetName}' to the database.", street.Name);

            await _streetContext.Streets.AddAsync(street);

            await _streetContext.SaveChangesAsync();

            _logger.LogInformation("Street with name '{StreetName}' successfully created with ID {StreetId}.", street.Name, street.Id);

            return street;
        }

        public async Task DeleteStreetAsync(int streetId)
        {
            _logger.LogInformation("Attempting to delete street with ID {StreetId}.", streetId);

            var streetToDelete = await GetStreetByIdAsync(streetId);

            if (streetToDelete is null)
            {
                _logger.LogError("Street with ID {StreetId} was not found and cannot be deleted.", streetId);
                throw new ArgumentNullException(nameof(Street), "Street cannot be null.");
            }

            _streetContext.Streets.Remove(streetToDelete);

            await _streetContext.SaveChangesAsync();

            _logger.LogInformation("Street with ID {StreetId} successfully deleted.", streetId);
        }

        public async Task<Street> GetStreetByIdAsync(int streetId)
        {
            if (streetId <= 0)
            {
                _logger.LogError("Invalid street ID {streetId}. ID cannot be less than zero.", streetId);
                throw new ArgumentOutOfRangeException(nameof(streetId), "Invalid street ID {streetId}. ID cannot be less than zero.");
            }

            var retrievedStreet = await _streetContext.Streets.FindAsync(streetId);

            if (retrievedStreet is null)
            {
                _logger.LogError("Street with ID {StreetId} was not found and cannot be deleted.", streetId);
                throw new ArgumentNullException(nameof(Street), "Street cannot be null.");
            }

            return retrievedStreet;
        }

        public async Task UpdateStreetAsync(Street streetToUpdate)
        {
            if (streetToUpdate is null)
            {
                _logger.LogError("Street with ID {StreetId} was not found and cannot be updated.", streetToUpdate.Id);
                throw new ArgumentNullException(nameof(streetToUpdate), "Street to update cannot be null.");
            }

            _streetContext.Streets.Update(streetToUpdate);
            await _streetContext.SaveChangesAsync();
        }
    }
}
