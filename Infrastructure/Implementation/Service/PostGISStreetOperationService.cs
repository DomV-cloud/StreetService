using Application.DatabaseContext;
using Application.Interfaces.Service;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Implementation.Service
{
    public class PostGISStreetOperationService : IStreetOperationService
    {
        private readonly StreetContext _dbContext;

        public PostGISStreetOperationService(StreetContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddPointToStreetAsync(int streetId, Coordinate newPoint, bool addToEnd)
        {
            string query = addToEnd
            ? @"UPDATE streets
                SET geometry = ST_AddPoint(geometry, ST_SetSRID(ST_MakePoint(@x, @y), 4326))
                WHERE id = @streetId"
            : @"UPDATE streets
                SET geometry = ST_AddPoint(geometry, ST_SetSRID(ST_MakePoint(@x, @y), 4326), 0)
                WHERE id = @streetId";

            await _dbContext.Database.ExecuteSqlRawAsync(query, new[]
            {
            new SqlParameter("@x", newPoint.X),
            new SqlParameter("@y", newPoint.Y),
            new SqlParameter("@streetId", streetId)
            });
        }
    }
}