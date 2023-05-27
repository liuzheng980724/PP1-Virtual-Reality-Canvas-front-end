using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTest")]
namespace Snobal.Library
{
    /**
     * Interface ILogger
     * 
     * Base interface for logging functionality
     */
    public interface ILogger : IDisposable
    {

        /**
         * Log a string with given log level
         * 
         * @param message The message to log
         * @param logLevel The log level for this message
         */
        void Log(string message, string logLevel);

    }
}
