using Microsoft.EntityFrameworkCore;
using TaskManager.Data;

namespace TaskManager.UnitTests
{
    internal class TaskManagerContextMocker
    {
        public static TaskManagerContext GetProcessesContextForTests(string dbName)
        {
            // Create options for DbContext instance
            DbContextOptions<TaskManagerContext> options = new DbContextOptionsBuilder<TaskManagerContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;

            // Create instance of DbContext
            TaskManagerContext dbContext = new TaskManagerContext(options);

            // Add entities in memory
            dbContext.Seed();

            return dbContext;
        }

    }
}
