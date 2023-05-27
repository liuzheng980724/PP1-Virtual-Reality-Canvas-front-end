using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Snobal.Library
{

    /**
     * Class Log
     * 
     * The main log file class to use for all general logging
     */
    public static class Logger
    {
        // ENUMS

        public enum LogTypeFlags 
        { 
            TextFile     = 1 << 0,          // Logging events to text file
            EventPosting = 1 << 1,           // Posting events to network event poster
            NetworkDebug = 1 << 2,
            Custom = 1 << 3
        }

        // The range of log levels to choose from
        public enum LogLevels { Debug, Info, Event, Warning, Error, Fatal }

        // DATA

        // The logs that are active by default
        public const uint DEFAULT_LOG_FLAGS = (uint)LogTypeFlags.TextFile | (uint)LogTypeFlags.EventPosting;
        public const uint NO_LOG = 0;

        // Has the Log class been initialised
        public static bool isInitialised { get; private set; }

        // The list of loggers to log to
        private static Dictionary<uint, ILogger> loggers = new Dictionary<uint, ILogger>();

        // PUBLIC

        // The current log level
        public static LogLevels LogLevel { get; set; }

        /**
         * Initialise the Log class
         * 
         * param logLevel The minimum log level
         * param loggerFlags The flags to set on the logger
         */
        public static void Init(LogLevels _LogLevel, string logfileName = null, uint loggerFlags = DEFAULT_LOG_FLAGS)
        {
            LogLevel = _LogLevel;

            // Turn on network logging if we are in debug mode
            if (LogLevel == LogLevels.Debug)
            {
                loggerFlags |= (uint)LogTypeFlags.NetworkDebug;
            }

            // Create all the loggers
            foreach (LogTypeFlags e in Enum.GetValues(typeof(LogTypeFlags)))
            {
                uint id = (uint)e;

                try
                {
                    if ((id & loggerFlags) != 0)
                    {
                        ErrorHandling.Test(FileIO.isInitialised, "Must initialise FileIO before initialising Logger");

                        switch (id)
                        {
                            case (uint)LogTypeFlags.TextFile:
                                {
                                    loggers.Add(id, new TextFileLoggerSynchronised(FileIO.GetErrorFileLocation(logfileName)));
                                    break;
                                }
                            case (uint)LogTypeFlags.EventPosting:
                                {
                                    loggers.Add(id, new EventLogger());
                                    break;
                                }
                            case (uint)LogTypeFlags.NetworkDebug:
                                {
                                    loggers.Add(id, new TextFileLoggerSynchronised(FileIO.GetNetworkDebugFileLocation()));
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandling.Fail(ex.Message);
                }
            }

            // We are now initialised
            isInitialised = true;
        }

        public static void AddCustomLogger(ILogger logger)
        {
            uint id = (uint)loggers.Count + 1;
            loggers.Add(id, logger);
        }

        /**
         * Shutdown the log system
         */
        public static void Shutdown()
        {
            foreach (var key in loggers.Keys)
            {
                loggers[key].Dispose();
            }

            loggers.Clear();

            isInitialised = false;
        }

        /**
         * Log to a given logger. Provide stack trace if desired.
         */
        private static void Log(ILogger logger, string message, LogLevels level)
        {
            logger.Log(message, level.ToString());

            // For warnings & above add a stacktrace
            if(level >= LogLevels.Warning)
            {
                var st = new StackTrace();
                string stackTrace = st.ToString();
                logger.Log(stackTrace, level.ToString());
            }
        }

        /**
         * Log a message at a given log level
         * 
         * param message The message to log
         * param level The log level for this message
         */
        public static void Log(string message, LogLevels level, uint logTypes)
        {
            ErrorHandling.Test(isInitialised, "Tried to using Snobal.Library.Logger before it was initialised");

            if (level < LogLevel)
            {
                return;
            }

            foreach (LogTypeFlags e in Enum.GetValues(typeof(LogTypeFlags)))
            {
                uint id = (uint)e;

                if ((id & logTypes) != 0)
                {
                    if (loggers.ContainsKey(id))
                    {
                        Log(loggers[id], message, level);
                    }
                }
            }
        }

        /// <summary>
        /// Will log to every logger in the collection.
        /// </summary>
        /// <param name="message">What you want to log.</param>
        /// <param name="level">Logs can be filtered by level.
        /// By default, logs below LogLevels.Info are ignored.</param>
        public static void Log(string message, LogLevels level = LogLevels.Info)
        {
            ErrorHandling.Test(isInitialised, "Tried to using Snobal.Library.Logger before it was initialised");

            if (level < LogLevel)
            {
                return;
            }

            foreach (ILogger logger in loggers.Values)
            {
                Log(logger, message, level);
            }
        }

        /// <summary>
        /// Log a SnobalException
        /// </summary>
        public static void Log(SnobalException exception)
        {           
            Log(exception.ToString(), LogLevels.Error);
        }

        /// <summary>
        /// Log a Exception
        /// </summary>
        public static void Log(Exception exception)
        {
            Log(exception.ToString(), LogLevels.Error);
        }


        // PRIVATE

        /**
         * Concert a log level string to the LogLevels enum type
         * 
         * param logLevelString The string to try and convert
         * return Return the converted log level or LogLevels.Info if failed
         */
        public static LogLevels TranslateLogLevel(string logLevelString)
        { 
            if (!string.IsNullOrEmpty(logLevelString))
            {
                LogLevels newLevel;
                if (Enum.TryParse(logLevelString, out newLevel))
                {
                    return newLevel;
                }
            }

            return LogLevels.Info;
        }
    }
}
