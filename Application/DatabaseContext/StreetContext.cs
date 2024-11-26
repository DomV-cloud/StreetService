using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Application.DatabaseContext
{
    public class StreetContext(DbContextOptions<StreetContext> options) : DbContext(options)
    {
        public DbSet<Street> Streets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Street>()
               .Property(s => s.Geometry)
               .HasColumnType("geometry(LineString, 4326)");

            modelBuilder.Entity<Street>().HasData(
            new Street
            {
                Id = 1,
                Name = "Example 1",
                Capacity = 150,
                Geometry = new LineString(new[]
                {
                    new Coordinate(-122.333056, 47.609722),
                    new Coordinate(-122.123889, 47.669444)
                })
            },
            new Street
            {
                Id = 2,
                Name = "Example 2",
                Capacity = 300,
                Geometry = new LineString(new[]
                {
                    new Coordinate( -122.431297, 37.773972),
                    new Coordinate(48.7801, 9.1815)
                })
            },
            new Street
            {
                Id = 3,
                Name = "Example 3",
                Capacity = 250,
                Geometry = new LineString(new[]
                {
                    new Coordinate(85.6522352, 25.983223),
                    new Coordinate(80.2321312, 25.563213)
                })
            }
        );
        }
    }
}
