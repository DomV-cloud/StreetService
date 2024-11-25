namespace Application.Interfaces.Service
{
    /// <summary>
    /// based on question on SO https://stackoverflow.com/questions/60966101/how-to-unit-test-a-function-in-net-core-3-1-that-uses-executesqlrawasync-to-cal
    /// </summary>
    public interface IDatabaseExecutor
    {
        Task ExecuteSqlRawAsync(string sql, params object[] parameters);
    }

}
