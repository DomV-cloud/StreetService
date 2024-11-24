namespace Contracts.Request.StreetRequestDto
{
    public record AddPointRequestDto(double Latitude, double Longitude, bool AddToEnd);
}
