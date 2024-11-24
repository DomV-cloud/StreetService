namespace Contracts.Request.StreetRequestDto
{
    public class CreateStreetRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public List<List<double>> Geometry { get; set; } = [];
    }
}
