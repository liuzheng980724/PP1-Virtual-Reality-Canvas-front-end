using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snobal.Utilities
{
    [Serializable]
    public abstract class SettingValues
    {
        /// <summary>
        /// When deriving from SettingValues this method must be replaced with a new static method if you intend on having multiple types of settings in your application.
        /// </summary>
        /// <returns>The string "Settings"</returns>
        public static string GetFilename() => "Settings";
    }
}
