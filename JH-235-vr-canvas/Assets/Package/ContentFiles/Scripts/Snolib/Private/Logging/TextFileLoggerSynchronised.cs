using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snobal.Library
{

    /**
     * Class TextFileLogger
     * 
     * Implementation of a Logger that logs to a text file in a thread safe manner
     */
    internal class TextFileLoggerSynchronised : ILogger
    {
        // DATA

        // The text file writer
        private TextFileSunchronised textFile;
        private bool disposedValue;

        // PUBLIC

        /**
         * Constructor
         * 
         * @param textFilePath The full path and filename to write to
         * @throws Exception on file path error
         */
        public TextFileLoggerSynchronised(string textFilePath)
        {
            try
            {
                textFile = new TextFileSunchronised(textFilePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~TextFileLoggerSynchronised()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }


        #region ILogger

        /**
         * Log a string with given log level
         * 
         * @param message The message to log
         * @param logLevel The log level for this message
         */
        public void Log(string message, string logLevel)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(DateTime.Now.ToString());
            sb.Append("]");

            sb.Append("[");
            sb.Append(logLevel);
            sb.Append("]");

            sb.Append(" ");

            sb.Append(message);

            textFile.Append(sb.ToString());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    textFile.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
