using Bogus;
using Domain.Entities;
using NetTopologySuite.Geometries;

namespace Tests.MockData
{
    public class FakeData
    {
        private static readonly Faker _faker = new();

        public static int GenerateRandomId()
        {
            return _faker.Random.Int(1, 100);
        }

        public static Coordinate GenerateRandomCoordinate()
        {
            return new Coordinate(
                _faker.Random.Double(-180, 180),
                _faker.Random.Double(-90, 90)
            );
        }

        public static Street GenerateRandomStreet(int streetId)
        {
            var initialCoordinates = GenerateRandomCoordinates(2);

            return new Street
            {
                Id = streetId,
                Name = _faker.Address.StreetName(),
                Geometry = new LineString(initialCoordinates),
                Capacity = _faker.Random.Int(1, 10)
            };
        }

        public static Coordinate[] GenerateRandomCoordinates(int count)
        {
            var coordinates = new Coordinate[count];
            for (int i = 0; i < count; i++)
            {
                coordinates[i] = GenerateRandomCoordinate();
            }
            return coordinates;
        }
    }
}
