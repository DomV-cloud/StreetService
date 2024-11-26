using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Application.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "Streets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Geometry = table.Column<LineString>(type: "geometry(LineString, 4326)", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Streets", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Streets",
                columns: new[] { "Id", "Capacity", "Geometry", "Name" },
                values: new object[,]
                {
                    { 1, 150, (NetTopologySuite.Geometries.LineString)new NetTopologySuite.IO.WKTReader().Read("SRID=0;LINESTRING (-122.333056 47.609722, -122.123889 47.669444)"), "Example 1" },
                    { 2, 300, (NetTopologySuite.Geometries.LineString)new NetTopologySuite.IO.WKTReader().Read("SRID=0;LINESTRING (-122.431297 37.773972, 48.7801 9.1815)"), "Example 2" },
                    { 3, 250, (NetTopologySuite.Geometries.LineString)new NetTopologySuite.IO.WKTReader().Read("SRID=0;LINESTRING (85.6522352 25.983223, 80.2321312 25.563213)"), "Example 3" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Streets");
        }
    }
}
