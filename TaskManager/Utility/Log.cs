using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TaskManager.Utility
{

    public interface ILoggable
    {
        DiagnosticLog.ApplicationModules MyModule { get; }
        string GetCurrentUser { get; }
        DiagnosticLog.LogType LogType { get; }
    }

    public static class LogExtension
    {

        public static void TaskManagerBegin(this ILogger log, string message, [CallerMemberName] string caller = "Unknow caller")
        {
            string header = $"-------- { caller }--------";
            log.TaskManagerSingleLogInfo(header);
            log.TaskManagerSingleLogInfo(message);
        }

        public static void TaskManagerEnd(this ILogger log, string message, [CallerMemberName] string caller = "Unknow caller")
        {
            string footer = $"-------- { caller }--------";
            log.TaskManagerSingleLogInfo(message);
            log.TaskManagerSingleLogInfo(footer);
        }



        public static void TaskManagerSingleLogInfo(this ILogger log, string message)
        {
            log.LogInformation("");
            log.LogInformation("-- Task Manager -- " + message);
            log.LogInformation("");
        }

        public static void TaskManagerSingleLogError(this ILogger log, string message)
        {
            log.LogError("");
            log.LogError("-- Task Manager -- " + message);
            log.LogError("");
        }

        public static void TaskManagerSingleLogWarning(this ILogger log, string message)
        {
            log.LogWarning("");
            log.LogWarning("-- Task Manager -- " + message);
            log.LogWarning("");
        }
    }

    public static class DiagnosticLog
    {
        public enum ApplicationModules : long
        {
            Writeback = 0b1_0000_0000_0000_0000_0000_0000_0000_0001
        }

        public enum LogType
        {
            Debug,
            Performance,
            Both
        }

        private static long appToTrace;


        public static void TraceModule(ApplicationModules module)
        {
            appToTrace |= (long)module;
        }

        public static void TraceModule(long modules)
        {
            appToTrace = modules;
        }

        public static void Debug(string message, ILoggable loggable)
        {
            if ((appToTrace & (long)(loggable.MyModule)) != 0L)
            {
                if (loggable.LogType == LogType.Debug || loggable.LogType == LogType.Both)
                {
                    Trace.Write($"User: {loggable.GetCurrentUser}" + message);
                }
            }
        }

        public static void Performance(string message, ILoggable loggable)
        {
            if ((appToTrace & (long)(loggable.MyModule)) != 0L)
            {
                if (loggable.LogType == LogType.Performance || loggable.LogType == LogType.Both)
                {
                    Trace.Write($"User: {loggable.GetCurrentUser}" + message);
                }
            }
        }
    }
}
