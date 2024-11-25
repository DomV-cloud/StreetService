using Application.DatabaseContext;
using Application.Interfaces.Service;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Npgsql;
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
        private readonly IDatabaseExecutor _executor;

        public PostGISStreetOperationService(StreetContext dbContext, IDatabaseExecutor executor)
        {
            _dbContext = dbContext;
            _executor = executor;
        }

        public async Task AddPointToStreetAsync(int streetId, Coordinate newPoint, bool addToEnd)
        {
            if (streetId <= 0 || newPoint is null)
            {
                return;
            }

            string query = addToEnd
            ? @"UPDATE public.""Streets""
                SET ""Geometry"" = ST_AddPoint(""Geometry"", ST_SetSRID(ST_MakePoint(@x, @y), 4326))
                WHERE ""Id"" = @streetId"
            : @"UPDATE public.""Streets""
                SET ""Geometry"" = ST_AddPoint(""Geometry"", ST_SetSRID(ST_MakePoint(@x, @y), 4326), 0)
                WHERE ""Id"" = @streetId";

            await _executor.ExecuteSqlRawAsync(query, new[]
            {
            new NpgsqlParameter("@x", newPoint.X),
            new NpgsqlParameter("@y", newPoint.Y),
            new NpgsqlParameter("@streetId", streetId)
            });
        }
    }
}