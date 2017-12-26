using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Crawler.Console
{
    public static class LoggerConfig
    {
        public static string GetDefaultLogDir()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Crawler.Console", "logs");
        }

        public static void ConfigureLogging(string dir)
        {
            var nLogFileName = Path.Combine(dir, "${shortdate}.log").Replace("\\", "/");

            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

#if DEBUG
            ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);
            consoleTarget.Layout = "${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=Message,Type,StackTrace:separator=//}";
#endif

            // Step 3. Set target properties 
            fileTarget.CreateDirs = true;
            fileTarget.FileName = nLogFileName;
            fileTarget.KeepFileOpen = false;
            fileTarget.ConcurrentWrites = true;
            fileTarget.Layout = "${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=Message,Type,StackTrace:separator=//}";

            // Step 4. Define rules
#if DEBUG
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));
#endif
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
            LogManager.EnableLogging();
            LogManager.ThrowExceptions = true;

            LogManager.ReconfigExistingLoggers();

            var logger = LogManager.GetLogger("DefaultLogger");
            TinyIoC.TinyIoCContainer.Current.Register<ILogger>(logger);

            logger.Info("========================================================================");
            logger.Info("Start");
        }
    }
}