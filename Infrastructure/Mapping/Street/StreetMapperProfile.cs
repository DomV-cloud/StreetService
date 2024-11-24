using AutoMapper;
using Contracts.Request.StreetRequestDto;
using Contracts.Responds.StreetResponseDto;
using NetTopologySuite.Geometries;

namespace Infrastructure.Mapping.Street
{
    public class StreetMapperProfile : Profile
    {
        public StreetMapperProfile()
        {
            CreateMap<CreateStreetRequestDto, Domain.Entities.Street>()
           .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => new LineString(
               src.Geometry.Select(coord => new Coordinate(coord[0], coord[1])).ToArray())));

            CreateMap<Domain.Entities.Street, CreateStreetResponseDto>()
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src =>
                    src.Geometry.Coordinates.Select(coord => new[] { coord.X, coord.Y }).ToArray()));
        }
    }
}
