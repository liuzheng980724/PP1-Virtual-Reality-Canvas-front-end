using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Snobal.Library
{
    /**
     * Class TextFile
     */
    internal class TextFileSunchronised : IDisposable
    {
        // DATA

        // The file to access
        private TextWriter writer;
        private bool disposedValue;

        // PUBLIC

        /**
         * Constructor
         * 
         * @param filePath The file path to write to
         * @throws Exception on error
         */
        public TextFileSunchronised(string filePath)
        {
            try
            {
                writer = TextWriter.Synchronized(new StreamWriter(filePath, true));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /**
         * Finaliser
         */
        ~TextFileSunchronised()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /**
         * Append a line to the text file
         * 
         * Will add a newline for you
         * 
         * @param line The string to append
         * @throws SnobalException if invalide file path
         */
        public void Append(string line)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(line);
            sb.Append(Environment.NewLine);

            writer.Write(sb.ToString());
            writer.Flush();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    writer.Close();         // Will Dispose
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        /**
         * Dispose
         */
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
