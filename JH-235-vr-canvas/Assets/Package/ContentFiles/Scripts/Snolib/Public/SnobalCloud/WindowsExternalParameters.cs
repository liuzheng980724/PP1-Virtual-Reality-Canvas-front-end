using System;

namespace Snobal.CloudSDK
{
    /// <summary>
    /// Used to evaluate cmd line parameters passed through to the application
    /// For use with bots, setting volumes, room IDs
    /// </summary>
    public class WindowsExternalParameters
    {
        public const string Botname = "botname";

        public static string GetCommandLineArgValue(string argument)
        {
            var arguments = Environment.GetCommandLineArgs();

            for (int i = 0; i < arguments.Length; i++)
                if (arguments[i].Contains(argument))
                    return arguments[++i];

            return null;
        }


        /// <summary>
        /// Checks the command line arguments for "botmode" to check if we were started in bot client mode.
        /// </summary>
        /// <returns></returns>
        public static bool StartedInBotClientMode()
        {
            if (GetCommandLineArgValue(Botname) != null)
            {
                return true;
            }

            return false;
        }
    }
}