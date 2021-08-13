using Microsoft.EntityFrameworkCore;
using System;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.UnitTests
{
    public static class TaskManagerContextExtension
    {
        public static void Seed(this TaskManagerContext dbContext)
        {
            // Add entities for DbContext instance
            dbContext.Processes.Add(new Process
            {
                PID = 1,
                Priority = Priority.low,
                CreatedAt = DateTime.Now
            });
            dbContext.Processes.Add(new Process
            {
                PID = 2,
                Priority = Priority.low,
                CreatedAt = DateTime.Now
            });
            dbContext.Processes.Add(new Process
            {
                PID = 3,
                Priority = Priority.high,
                CreatedAt = DateTime.Now
            });
            dbContext.Processes.Add(new Process
            {
                PID = 4,
                Priority = Priority.medium,
                CreatedAt = DateTime.Now
            });


            foreach (Process prc in dbContext.Processes)
            {
                dbContext.Entry(prc).State = EntityState.Detached;
            }
            dbContext.SaveChanges();
        }
    }
}
