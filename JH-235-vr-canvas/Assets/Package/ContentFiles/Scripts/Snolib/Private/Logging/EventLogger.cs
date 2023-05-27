using System;
using System.Collections.Generic;
using System.Text;

namespace Snobal.Library
{

    /**
     * Class EventLogger
     * 
     * Implementation of a Logger that logs events
     */
    internal class EventLogger : ILogger
    {
        // PUBLIC

        #region ILogger

        /**
         * Log a string with given log level
         * 
         * @param message The message to log
         * @param logLevel The log level for this message
         */
        public void Log(string message, string logLevel)
        {
            if (Events.IsInitialised)
            {
                Events.Post(message, logLevel);
            }
        }

        /**
         * Dispose
         */
        public void Dispose()
        {
        }

        #endregion
    }
}
