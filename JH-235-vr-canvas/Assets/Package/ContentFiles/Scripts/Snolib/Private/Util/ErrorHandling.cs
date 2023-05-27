using System;
using System.Collections.Generic;

namespace Snobal.Library
{
    /**
     * Class Debug
     * 
     * Internal debug features
     */
    internal static class ErrorHandling
    {
        /**
         * Test if condition is valid
         * 
         * @param condition The condition to test
         * @param message The message to send in the exception if condition is false
         * @throws SnobalException
         */
        public static void Test(bool condition, string message)
        {
            if (!condition)
            {
                throw new SnobalException(message, "An internal error occurred");
            }
        }

        /**
         * Raise an error exception
         * 
         * @param message The message to send in the exception
         */
        public static void Fail(string message)
        {
            Test(false, message);
        }
    }
}
