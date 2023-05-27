using System.Collections.Generic;

namespace Snobal.Library
{
    /// <summary>
    /// This is a manifest of the applications installed
    /// </summary>
    public class ApplicationManifest
    {
        private const string FILENAME = "ApplicationManifest.json";
        
        /// <summary>
        /// This dictionary helps when resetting teh device after a token failure.
        /// We can use this in 'GetProfile'. Loop over it and assign it as our 'Device profile'
        /// See GetProfile:GetProfileData()
        /// </summary>
        public Dictionary<string, ApplicationData> InstalledApplications { get; }

        public ApplicationManifest(string _path)
        {
            InstalledApplications = ReadApplicationManifest(_path);
        }

        static Dictionary<string, ApplicationData> ReadApplicationManifest(string _path)
        {
            string path = _path;
            if (!System.IO.File.Exists(path))
                return new Dictionary<string, ApplicationData>();

            string applicationDataString = System.IO.File.ReadAllText(path);
            return Snobal.Utilities.Serialization.DeserializeToType<Dictionary<string, ApplicationData>>(applicationDataString);
        }

        static public void WriteApplicationManifest(List<ApplicationData> installedApplications)
        {
            Dictionary<string, ApplicationData> applicationsDict = new Dictionary<string, ApplicationData>();

            foreach (ApplicationData app in installedApplications)
            {
                applicationsDict.Add(app.packageName, app);
            }

            DeleteApplicationManifest();

            string appString = Snobal.Utilities.Serialization.SerializeObject(applicationsDict);

            string path = GetDefaultPath();
            System.IO.File.WriteAllText(path, appString);
        }

        static public void DeleteApplicationManifest()
        {
            string path = GetDefaultPath();
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        public static bool Check(ApplicationData profile, Dictionary<string, ApplicationData> installedApplications)
        {
            if (!installedApplications.ContainsKey(profile.name))
                return true;

            if (!string.Equals(installedApplications[profile.name].checksum, profile.checksum))
                return true;

            return false;
        }

        public static string GetDefaultPath()
        {
            return System.IO.Path.Combine(Snobal.Library.FileIO.GetSharedFilesPath, FILENAME);
        }
    }
}
