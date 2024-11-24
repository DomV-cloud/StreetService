using NetTopologySuite.Geometries;

namespace Contracts.Responds.StreetResponseDto
{
    public record CreateStreetResponseDto(string Name, LineString Geometry,int Capacity);
}
