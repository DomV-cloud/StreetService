using NetTopologySuite.Geometries;

namespace Application.Interfaces.Service
{
    public interface IStreetOperationService
    {
        Task AddPointToStreetAsync(int streetId, Coordinate newPoint, bool addToEnd);
    }
}
