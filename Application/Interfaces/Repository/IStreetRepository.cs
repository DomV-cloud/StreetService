using Domain.Entities;
using NetTopologySuite.Geometries;

namespace Application.Interfaces.Repository
{
    public interface IStreetRepository
    {
        public Task<Street> CreateStreetAsync(Street streetToInsert);

        public Task DeleteStreetAsync(int streetId);

        public Task<Street> GetStreetByIdAsync(int streetId);

        public Task AddPointToStreetAsync(int streetId, Coordinate newPoint, bool addToEnd);

        Task UpdateStreetAsync(Street streetToUpdate);
    }
}
