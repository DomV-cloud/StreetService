using Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
        }
    }
}
