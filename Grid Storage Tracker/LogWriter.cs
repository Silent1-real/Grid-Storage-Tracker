using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace GridStorageTracker
{
    public static class LogWriter
    {
        private static readonly Logger fileLogger = LogManager.GetLogger("GridStorageTracker");
        private static FileTarget fileTarget;

        public static void InitializeLogger(string logFolderPath, bool enableDedicatedLogFile)
        {
            if (enableDedicatedLogFile)
            {
                // the log writere method nesting here and only runs if its true in config.

                string logFilePath = Path.Combine(logFolderPath, "GridStorageTracker-${date:format=yyyy-M-d}.log");

                fileTarget = new FileTarget("GridStorageTracker")
                {
                    FileName = logFilePath,
                    Layout = "${longdate} ${message}"
                };

                var config = LogManager.Configuration ?? new LoggingConfiguration();
                config.AddTarget(fileTarget);
                config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget, "GridStorageTracker");
                LogManager.Configuration = config;
            }


                fileLogger.Info("*** Grid Storage Tracker log initialized ***");
        }
        // methods used for 3 events 2 of them are for player events and 1 is for admin event.
        //all logging catches by torch. if the dedicated log file is enabled, it will also write to the dedicated log file.
        public static void LogStoreEvent(string playerName, string gridName, long entityId)
        {
            fileLogger.Info($"Player:{playerName} Stored Grid:'{gridName}' with Entity ID: {entityId}");
        }

        public static void LogRetrieveEvent(string playerName, string gridName, long entityId)
        {
            fileLogger.Info($"Player:{playerName} Retrieved Grid:'{gridName}' with Entity ID: {entityId}");
        }

        public static void LogRemoveEvent(string adminName, string gridName, string playerName)
        {
            fileLogger.Info($"Admin:{adminName} Removed Grid:'{gridName}' from '{playerName}' grid storage terminal.");
        }
    }
}