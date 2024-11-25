using Application.DatabaseContext;
using Application.Interfaces.Service;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation.Service
{
    public class DatabaseExecutor : IDatabaseExecutor
    {
        private readonly StreetContext _dbContext;

        public DatabaseExecutor(StreetContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }
    }
}
