using System;

namespace TaskManager.Utility
{
    public static class WebAppSettings
    {
        public static string GetTaskManagerCapacity()
        {
            return string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TaskManager_Capacity")?.ToString()) ? "10" : Environment.GetEnvironmentVariable("TaskManager_Capacity");
        }
    }
}
