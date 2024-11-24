using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace Domain.Entities
{
    public class Street : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public LineString Geometry { get; set; } = null!;

        public int Capacity { get; set; }
    }
}
