using System;
using System.IO;
using Snobal.Library;

namespace Snobal.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">A type derived from SettingValues</typeparam>
    public class Settings<T> where T: SettingValues, new()
    {
        public T Values { get; private set; }

        string filepath { get { if (cachedFilepath == null) { CacheFilepath(); } return cachedFilepath; } } // Only need to use reflection once
        string cachedFilepath { get; set; } = null; // Don't access this directly, use filepath

        /// <summary>
        /// Upon calling will create a Settings<T> object and attempt to load from the associated file
        /// See SettingValues.Getfilename for more information
        /// If the file does not exist in the default location, the settings values will maintain their default definition.
        /// </summary>
        /// <returns>An object of type T that has already attempted</returns>
        public static Settings<T> SettingsFactory()
        {
            var settings = new Settings<T>();
            settings.Load();
            return settings;
        }

        private void CacheFilepath()
        {
            cachedFilepath = Path.Combine(FileIO.GetSharedFilesPath, GetFilename());
        }

        private string GetFilename()
        {
            Type type = typeof(T);
            System.Reflection.MethodInfo method = type.GetMethod("GetFilename");
            object returnValue = method.Invoke(null,null); // slow
            string filename = (string)returnValue;
            return string.Format("{0}.json", filename);
        }

        public T Load()
        {
            Logger.Log("Loading settings");

            T loadedValues;

            if (!File.Exists(filepath))
                loadedValues = new T();
            else
            {
                var valuesFromFile = File.ReadAllText(filepath);
                if (valuesFromFile.Length == 0)
                    loadedValues = new T();
                else
                    loadedValues = Serialization.DeserializeToType<T>(valuesFromFile);
            }
            Values = loadedValues;
            return loadedValues;
        }

        /// <summary> Updates the settings objective with a new value object and saves it to file </summary>
        public void Write(T newValues)
        {
            Values = newValues;
            Save();
        }

        /// <summary> Saves current values to the file defined by SettingValues.GetFilename()</summary>
        public void Save()
        {
            Logger.Log("Saving settings");

            string dataPath = FileIO.GetSharedFilesPath;
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            string json = Serialization.SerializeObject(Values);
            File.WriteAllText(filepath, json);
        }

        /// <summary> Resets all settings back to default values </summary>
        public void ResetToDefaults()
        {
            Values = new T();
            Save();
        }
    }
}
