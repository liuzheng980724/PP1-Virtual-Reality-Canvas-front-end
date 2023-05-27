#pragma warning disable 649
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Snobal.Utilities;
using Snobal.Library;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Snobal.Extras
{
    public class Resources
    {
        public static Action<string> DeleteFileCallback;

        #region Data structures
        [Serializable]
        public abstract class DeserializableResource
        {
            public string id;
            public string jsonObject;
        }

        [Serializable]
        public class ResourceData : DeserializableResource
        {
            // Serialized variables (from the json)
            public string type;
            public string url;                                  // This is the raw resource string, may contain tokens, be relative etc. Use uri below for network functions

            // Properties + non-serialised variables
            private string fileName = null;
            public string FileName
            {
                get
                {
                    if (fileName == null)
                        fileName = id + "." + type;

                    return fileName;
                }
                set
                {
                    fileName = value;
                }
            }

            public string MetaFileName { get { return FileName + ".meta"; } }
            [NonSerialized] public ResourceMetaData metaData;
            [NonSerialized] public Uri uri;                     // Created frow url after any token or relative paths are dealt with.
        }

        [Serializable]
        public struct ResourceMetaData
        {
            public DateTimeOffset dateTime;
            public string etag;
            public long fileSize;
        }

        // Scripts
        [Serializable]
        public class ScriptData : DeserializableResource
        {
            public ScriptBlockData[] blocks = null;
        }

        [Serializable]
        public class ScriptBlockData
        {
            public string id = null;
            public bool timeControlPoint = false;
            public string[] randomNextBlockIds = null;
            public ScriptStepData[] steps = null;
        }

        [Serializable]
        public class ScriptStepData
        {
            // Inuse for current presentation system
            public string type = null;
            public string animationName = null;
            public string target = null;
            public SubtitleData subtitles = null;
            public string resourceId = null;
            public float? duration = null;
            public string id = null;

            // likely can remove. Wait for presentation refactor
            public string modelId = null;
            public string jumpToBlockId = null;
            public float startDelay = 0f;
            public string scriptId = null;
            public string targetId = null;
        }

        [Serializable]
        public class SubtitleData
        {
            public string resourceId = null;
            public string text = null;
            public string target = null;
        }

        // Assets

        [Serializable]
        public class AssetData
        {
            public string assetId = null;
            public string assetName = null;
            public string creator = null;
            public string createTime = null;
            public string modifier = null;
            public string modifyTime = null;

            //public string properties = null;

            public string[] tags = null;

            public class primaryAssetData
            {
                public string creator = null;
                public string createTime = null;
                public string modifier = null;
                public string modifyTime = null;
                public string fingerprint = null;
                public string originalName = null;

                // Derivatives Data
                [Serializable]
                public class DerivativesData
                {
                    // Transcoded data
                    [Serializable]
                    public class TranscodeStatus
                    {
                        public string status = null;
                        public string message = null;
                    }
                    public TranscodeStatus transcode = null;

                    // Details 
                    [Serializable]
                    public class DetailsData
                    {
                        // Format data
                        [Serializable]
                        public class FormatData
                        {
                            public float duration = 0.0f;
                            //public string format_name = null;
                        }
                        public FormatData format = null;

                        // Stream data
                        [Serializable]
                        public class StreamData
                        {
                            public int index = 0;
                            public int width = 0;
                            public int height = 0;
                            public float duration = 0.0f;

                            //public string nb_frames = null;
                            //public string codec_name = null;
                            //public string codec_type = null;
                            //public string avg_frame_rate = null;
                            //public int sample_rate = 0;
                        }
                        public StreamData[] streams = null;

                    }
                    public DetailsData details = null;
                    // End DetailsData

                    //public object thumbnail = null;
                }
                public DerivativesData derivatives = null;
                // End DerivativesData

                public string filesize = null;
                public string mimeType = null;
                public string locked = null;
            }
            public primaryAssetData primary;

            //public object variants = null;

            public bool available;


            public enum TranscodeStatus { NONE, UNKNOWN, PROCESSING, SUBMITTED, COMPLETE }

            /// <summary>
            /// Returns: PROCESSING, SUBMITTED, COMPLETE
            /// </summary>
            /// <returns></returns>
            public TranscodeStatus GetCloudMediaTranscodeStatus()
            {
                if (primary.derivatives == null || primary.derivatives.transcode == null || primary.derivatives.transcode.status == null)
                {
                    return TranscodeStatus.NONE;
                }

                TranscodeStatus status;
                if (Enum.TryParse(primary.derivatives.transcode.status, out status))
                {
                    return status;
                }
                else
                {
                    Logger.Log("Unknow Transcode status: '" + assetId + " (" + assetName + ")" + ", has status of " + primary.derivatives.transcode.status + ". Ignoring asset.");
                    return TranscodeStatus.UNKNOWN;
                }
            }
        }


        /// <summary>
        /// TranscodedData is the return value of calling LoadTranscodeData
        /// </summary>
        [Serializable]
        public class TranscodedData
        {
            public string URL = null;
            public string CLOUDFRONT_COOKIE = null;
        }


        // Scene

        [Serializable]
        public class SubtitleSettings
        {
            // Not currently supported. Could refactor.
            public string subtitleStyle = "none";  // none, scrolling or popup
            public int subtitleMaxCharacters = 50;
            public float subtitleScrollStartDelay = 2f;
            public float subtitleScrollEndDelay = 2f;
            public float subtitleEndPadding = 10f;
            public float subtitleDurationOffsetFromAudio = -2f;
        }

        // Overarching control model
        [Serializable]
        class ExperienceData
        {
            // Unused. By likely can be refactored into a bunch of settings. ie like subtitles settings, controller settings etc.
            public SubtitleSettings subtitleSettings = new SubtitleSettings();
            public string supportedControllerMode = "none";
        }

        #endregion // Data structures

        #region Experience data

        static Dictionary<string, ResourceData> resources;
        static Dictionary<string, ScriptData> scripts;
        static ExperienceData experience;
        public static bool forceDownloadAllResources = false;

        static AssetData[] mediaList;     // List of all media of the tentant site.

        #endregion // Experience data

        #region Json deserialisation


        public static void LoadSubExperienceFromPath(SnobalCloud sc, string experienceFilePath)
        {
            // Load from given path
            string jsonString;
            try
            {
                jsonString = FileIO.ReadAllText(experienceFilePath);
            }
            catch (FileNotFoundException e)
            {
                throw new SnobalException("Missing file: " + e.FileName, "Unfortunately not all files were completely downloaded before entering offline mode and the experience can not continue.", e);
            }
            LoadSubExperienceFromJsonString(sc, jsonString);
        }

        public static void LoadSubExperience(SnobalCloud sc, string experienceFileName)
        {
            // Load from snobal cloud cache folder
            string filePath = FileIO.GetSubExperienceFilePath(experienceFileName);
            LoadSubExperienceFromPath(sc, filePath);
        }

        public static void LoadSubExperienceFromJsonString(SnobalCloud sc, string jsonArray)
        {
            if (resources == null)
                resources = new Dictionary<string, ResourceData>();
            if (scripts == null)
                scripts = new Dictionary<string, ScriptData>();

            dynamic jsonBlob;
            try
            {
                jsonBlob = Serialization.DeserializeToDynamic(jsonArray);
            }
            catch
            {
                throw new SnobalException("Deserialize error in: " + jsonArray, "Unfortunately not all files were completely downloaded before entering offline mode and the experience can not continue.");
            }

            // If we are not paired allow, allow loading but we can't use anything from settings etc. 
            if (!sc.IsPaired)
            {
                DeserializeToDictionary(jsonBlob.resources, resources);
            }
            else
            {
                DeserializeResourceDataToDictionary(jsonBlob.resources, resources, sc);
            }

            DeserializeToDictionary<ScriptData>(jsonBlob.scripts, scripts);

            if (jsonBlob.experiences != null)
                experience = Serialization.DeserializeToType<ExperienceData>(jsonBlob.experiences[0].ToString());
        }


        public static void LoadExperience(SnobalCloud sc)
        {
            var cachedExperiencePaths = ReadCache();
            resources = new Dictionary<string, ResourceData>();
            scripts = new Dictionary<string, ScriptData>();

            foreach (var experienceFileName in cachedExperiencePaths.subExperiences)
            {
                LoadSubExperience(sc, experienceFileName);
            }

            if (experience == null)
                throw new SnobalException("SubExperiences were loaded but no experience object was defined.", "Session contains either an invalid experience or no experience and can not continue.");
        }

        /// <summary>
        /// Delete all experience data
        /// </summary>
        public static void UnloadExperience()
        {
            resources = null;
            scripts = null;
            experience = null;
        }

        private static string ReplaceStringInResourceURL(string url, string searchStr, string replacement)
        {
            if (replacement == "")
            {
                // if replacing with an empty string remove the extra '/' as well.
                return url.Replace(searchStr + "/", replacement);
            }
            else
            {
                return url.Replace(searchStr, replacement);
            }
        }

        /// <summary>
        /// Deserialize resource data to dictionary, replace any URL tokens with correct details, fix up any relative URL paths.
        /// </summary>
        private static void DeserializeResourceDataToDictionary(dynamic jsonBlobResources, Dictionary<string, ResourceData> resources, SnobalCloud sc)
        {
            // load resource data into seperate dictionary
            Dictionary<string, ResourceData> newResources = new Dictionary<string, ResourceData>();
            DeserializeToDictionary<ResourceData>(jsonBlobResources, newResources);

            if (newResources.Count > 0)
            {
                // New resources data to add. 

                // Replace any platform & Device names into the asset bundles URL's in order to download the correct asset bundle.
                //
                // ie "https://mckinsey-iiot-walkathon-dev.snobal.net/files/unity/<PLATFORM>/femaleafricanexecutive.bundle"
                // becomes:
                // "https://mckinsey-iiot-walkathon-dev.snobal.net/files/unity/femaleafricanexecutive.bundle" for Windows (no folder name for Windows)
                // "https://mckinsey-iiot-walkathon-dev.snobal.net/files/unity/Android/femaleafricanexecutive.bundle" for the Android
                //
                // and:
                // "https://mckinsey-iiot-walkathon-dev.snobal.net/files/unity/<PLATFORM>/<DEVICE>/myImage.png" 
                // becomes:
                // "https://mckinsey-iiot-walkathon-dev.snobal.net/files/unity/Android/OculusQuest/myImage.png"  for the Quest on Android etc.

                // Tenant host name
                Uri tenantURI = new Uri(sc.Settings.Values.TenantURL);
                string tenantHost = tenantURI.Host;

                // language
                string language = "en";     // TODO: Get language from user?
                string resourceUrl;
                foreach (var kvp in newResources)
                {
                    resourceUrl = kvp.Value.url;

                    // Substitute required platform & device names
                    resourceUrl = ReplaceStringInResourceURL(resourceUrl, "<TENANT_HOST>", tenantHost);
                    resourceUrl = ReplaceStringInResourceURL(resourceUrl, "<LANGUAGE>", language);

                    // NOTE: Do any other URL string processing before we create the absolute Uri class below.

                    // Covert any relative URL's to absolute URL's & return our Uri class. Uses the tentant URL from settings.
                    // ie "/files/unity/femaleafricanexecutive.bundle" to "https://mckinsey-iiot-walkathon-dev.snobal.net/files/unity/femaleafricanexecutive.bundle"
                    kvp.Value.uri = sc.GetAbsoluteUri(resourceUrl);

                    // Add to passed in dictionary                    
                    if (!resources.ContainsKey(kvp.Key))
                    {
                        resources.Add(kvp.Key, kvp.Value);
                    }
                    else if (resources[kvp.Key].FileName == kvp.Value.FileName)
                    {
                        Logger.Log("Ingoring identical value for key '" + kvp.Key + "' = '" + kvp.Value.FileName + "'", Library.Logger.LogLevels.Debug);
                    }
                    else
                    {
                        throw new SnobalException("Redefined key '" + kvp.Key + "' old = '" + resources[kvp.Key].FileName + "', new = '" + kvp.Value.FileName + "'", "An error occurred while retrieving content from the server.");
                    }
                }
            }
        }

        static void DeserializeToDictionary<T>(dynamic blob, Dictionary<string, T> dictionary) where T : DeserializableResource
        {
            if (blob != null)
            {
                foreach (var item in blob)
                {
                    T data = Serialization.DeserializeToType<T>(item.ToString());
                    data.jsonObject = item.ToString();
                    if (!dictionary.ContainsKey(data.id))
                    {
                        dictionary.Add(data.id, data);
                    }
                    else if (dictionary[data.id].jsonObject == item.ToString())
                    {
                        Logger.Log("Ingoring identical value for key '" + data.id + "' = '" + item.ToString() + "'", Library.Logger.LogLevels.Debug);
                    }
                    else
                    {
                        throw new SnobalException("Redefined key '" + data.id + "' old = '" + dictionary[data.id].jsonObject + "', new = '" + item.ToString() + "'", "An error occurred while retrieving content from the server.");
                    }
                }
            }
        }

        #endregion // Json deserialisation

        #region Data accessors

        public static IEnumerable<string> GetResourceIds()
        {
            return resources.Keys;
        }

        public static ResourceData GetResourceData(string _resourceId)
        {
            return resources[_resourceId];
        }

        /// <summary> Combines the resource name with the content path, checking the file exists </summary>
        public static string GetResourceFilePath(string _id, bool _assertFileExists = true)
        {
            if (!resources.ContainsKey(_id))
                throw new SnobalException("Resource '" + _id + "' not found", "A content file was missing from the experience.");

            string fullPath = Path.Combine(FileIO.GetXRContentDataPath() + resources[_id].FileName);

            if (_assertFileExists && !File.Exists(fullPath))
                throw new SnobalException("Resource '" + fullPath + "' does not exist.", "A content file was missing from the experience.");

            return fullPath;
        }

        /// <summary> Combines the resource name with the content path, checking the file exists, then turns it into a "file:///..." URI </summary>
        public static string GetResourceFilePathAsURL(string _resourceId)
        {
            string filePath = GetResourceFilePath(_resourceId);
            return new Uri(filePath).AbsoluteUri;
        }

        /// <summary> Have a valid loaded experience </summary>
        public static bool IsExperienceLoaded()
        {
            if (experience == null)
            {
                return false;
            }
            return true;
        }

        public static Dictionary<string, ScriptData> GetScripts()
        {
            return scripts;
        }

        public static string GetSupportedControllerMode()
        {
            if (experience == null)
                throw new SnobalException("Tried to get controlled mode but experience hasn't been loaded", "An error occurred");
            return experience.supportedControllerMode;
        }

        public static SubtitleSettings GetSubtitleSettings()
        {
            return experience.subtitleSettings;
        }

        public static ScriptBlockData[] GetScriptBlocks(string _scriptId)
        {
            if (!scripts.ContainsKey(_scriptId))
                throw new SnobalException("Script '" + _scriptId + "' not found in the experience", "There was content missing from the experience");

            return scripts[_scriptId].blocks;
        }

        #endregion // Data accessors

        #region Caching
        [Serializable]
        internal class ExperienceCache
        {
            public string[] subExperiences;
        }

        internal static ExperienceCache ReadCache()
        {
            var cachePath = FileIO.GetCacheFilePath();
            ExperienceCache cache = Serialization.DeserializeToType<ExperienceCache>(File.ReadAllText(cachePath));
            return cache;
        }

        internal static string[] GetSubExperiences()
        {
            var stringArray = ReadCache().subExperiences;
            return stringArray;
        }


        public static void DownloadExperiences(string[] urlsToDownload, System.Net.Http.HttpClient client)
        {
            List<string> subExperienceNames = new List<string>();

            // clear out old subexperiences, they are downloaded every time
            foreach (var file in Directory.GetFiles(FileIO.GetSubExperienceFilePath()))
            {
                File.Delete(file);
            }

            int index = 0;

            List<Task> tasks = new List<Task>();

            foreach (var subExperienceUrl in urlsToDownload)
            {
                string subExperienceName = index.ToString();
                index++;
                subExperienceNames.Add(subExperienceName);

                Uri uri = new Uri(subExperienceUrl);
                var clientWithProgress = new Utilities.HttpClientWithProgress(client, uri, FileIO.GetSubExperienceFilePath(subExperienceName));
                tasks.Add(clientWithProgress.StartDownload());
            }

            // Run all tasks
            var downloadResult = false;
            try
            {
                downloadResult = Utilities.TaskWatcher.RunTasks(tasks, 30000);
            }
            catch (Exception e)
            {

            }

            // Generate cache file
            ExperienceCache cache = new ExperienceCache();
            cache.subExperiences = subExperienceNames.ToArray();

            File.WriteAllText(FileIO.GetCacheFilePath(), Serialization.SerializeObject(cache));

            if (!downloadResult)
            {
                throw new SnobalException("DownloadExperiences tasks not completed, timeout occured.");
            }
        }

        public static Task DownloadExperiencesAsync(string[] urlsToDownload, System.Net.Http.HttpClient client)
        {
            List<string> subExperienceNames = new List<string>();

            // clear out old subexperiences, they are downloaded every time
            foreach (var file in Directory.GetFiles(FileIO.GetSubExperienceFilePath()))
            {
                File.Delete(file);
            }

            int index = 0;

            List<Task> tasks = new List<Task>();

            foreach (var subExperienceUrl in urlsToDownload)
            {
                string subExperienceName = index.ToString();
                index++;
                subExperienceNames.Add(subExperienceName);

                Uri uri = new Uri(subExperienceUrl);
                var clientWithProgress = new Utilities.HttpClientWithProgress(client, uri, FileIO.GetSubExperienceFilePath(subExperienceName));
                tasks.Add(clientWithProgress.StartDownload());
            }

            // Generate cache file
            ExperienceCache cache = new ExperienceCache();
            cache.subExperiences = subExperienceNames.ToArray();

            File.WriteAllText(FileIO.GetCacheFilePath(), Serialization.SerializeObject(cache));

            return Task.WhenAll(tasks);
        }


        /// <summary>
        /// Download a list of all the media on the tentant site
        /// </summary>
        public static Task DownloadMediaList(SnobalCloud sc, bool includeUnavailableAssets)
        {
            Networking.WebResponse webResponse = null;

            AuthenticatedRestclient authClient = (Snobal.Library.AuthenticatedRestclient)sc.RestClient;

            if (authClient == null)
            {
                throw new Library.SnobalException();
            }

            string url;
            if (includeUnavailableAssets)
            {
                // unavailable assets include created media without any assigned image etc.
                url = sc.Settings.Values.TenantURL + API.allMediaListExtension;
            }
            else
            {
                url = sc.Settings.Values.TenantURL + API.availableMediaListExtension;
            }

            try
            {
                webResponse = authClient.Get(url);
            }
            finally
            {
            }

            if (!webResponse.Success)
            {
                Logger.Log("GetMediaList Error: '" + url + "' returned http code: " + webResponse.HttpStatusCode + ", web response: " + webResponse.Response);
            }

            mediaList = Snobal.Utilities.Serialization.DeserializeToType<AssetData[]>(webResponse.Response);

            /*
            Logger.Log("---------------------------------------------------------", Logger.LogLevels.Info);
            Logger.Log("GetMediaList: Got " + mediaList.Length + " items", Logger.LogLevels.Info);
            for(int i=0; i < mediaList.Length; i++)
            {
                Logger.Log(mediaList[i].assetId, Logger.LogLevels.Info);
            }
            Logger.Log("---------------------------------------------------------", Logger.LogLevels.Info);
            */

            return Task.CompletedTask;
        }

        public static AssetData[] GetMediaList()
        {
            return mediaList;
        }


        /// <summary>
        /// Get the transcode details for a online streaming asset (like a video)
        /// </summary>
        public static TranscodedData DownloadTranscodeData(SnobalCloud sc, string GUID)
        {
            Networking.WebResponse webResponse;

            AuthenticatedRestclient authClient = (AuthenticatedRestclient)sc.RestClient;

            if (authClient == null)
            {
                throw new SnobalException();
            }

            string url = sc.Settings.Values.TenantURL + API.transcodeDataExtension + GUID;

            // Get content
            try
            {
                webResponse = authClient.Get(url);
            }
            finally
            {
            }

            if (!webResponse.Success)
            {
                Logger.Log("DownloadTranscodeData Error: '" + url + "' returned http code: " + webResponse.HttpStatusCode + ", web response: " + webResponse.Response);
            }

            TranscodedData data = Serialization.DeserializeToType<TranscodedData>(webResponse.Response);

            return data;
        }


        #endregion

        #region File Downloading
        const int defaultMaxConcurrentHeadRequests = 5;

        public class FileDownloader
        {
            List<ResourceData> resourcesToDownload = new List<ResourceData>();
            string dataPath = FileIO.GetXRContentDataPath();
            public IProgress<double> DownloadProgress;
            private int maxConcurrentHeadRequests = defaultMaxConcurrentHeadRequests;

            public long totalDownloadSize { get; private set; } = 0;
            public long DownloadProgressBytes { get; private set; } = 0;
            public float DownloadProgressPercentage { get; private set; } = 0;
            public int completedResources { get; private set; } = 0;
            public int ResourcesRemaining { get { return resourcesToDownload.Count - completedResources; } }
            SnobalCloud snobalCloudReference;
            Action<ResourceData> resourceDownloadedcallBack = null;

            public FileDownloader(SnobalCloud _sc, int _maxConcurrentHeadRequests = defaultMaxConcurrentHeadRequests)
            {
                snobalCloudReference = _sc;
                maxConcurrentHeadRequests = _maxConcurrentHeadRequests;
            }

            public async Task CheckForContent(Action<ResourceData, bool> resourceUpToDatecallBack = null)
            {
                Logger.Log("Loading data from json...");
                LoadExperience(snobalCloudReference);
                Logger.Log("...loading data complete. Getting resources to download.");

                // Build queue of resources to check
                IEnumerable<string> resourceIds = GetResourceIds();
                List<ResourceData> resourceList = new List<ResourceData>();

                foreach (var id in resourceIds)
                    resourceList.Add(GetResourceData(id));

                await CheckForContent(resourceList, resourceUpToDatecallBack);
            }

            public async Task CheckForContent(List<ResourceData> resourceList, Action<ResourceData, bool> resourceUpToDatecallBack = null)
            {
                // Download latest logo
                resourceList.Add(snobalCloudReference.Tenant.GetTenantLogoResourceData());

                // Perform all head requests
                using (var semaphore = new SemaphoreSlim(maxConcurrentHeadRequests))
                {
                    List<Task> contentCheckTasks = new List<Task>();

                    foreach (var resource in resourceList)
                    {
                        await semaphore.WaitAsync();
                        contentCheckTasks.Add(Task.Run(() =>
                        {
                            try
                            {
                                if (!IsResourceUpToDate(resource))
                                {
                                    snobalCloudReference.Settings.Values.CachedContentComplete = false;
                                    resourcesToDownload.Add(resource);
                                    totalDownloadSize += resource.metaData.fileSize;
                                    resourceUpToDatecallBack?.Invoke(resource, false);
                                }
                                else
                                {
                                    resourceUpToDatecallBack?.Invoke(resource, true);
                                }
                            }
                            catch (SnobalException e)
                            {
                                // Something went wrong. Log the issue, then ignore this task & move on.
                                Logger.Log("Error getting resource '" + resource.id + "', file name: " + resource.FileName, Logger.LogLevels.Error);
                                Logger.Log(e);

                                semaphore.Release();
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }));
                    }

                    // Run all tasks
                    if (!Utilities.TaskWatcher.RunTasks(contentCheckTasks, 30000))
                    {
                        throw new SnobalException("CheckForContent tasks not completed, timeout occured.");
                    }
                }
            }

            /// <summary> Checks if there is an update to be downloaded for a resource </summary>
            /// <param name="_resourceData"> Resource </param>
            bool IsResourceUpToDate(ResourceData _resourceData)
            {
                if (forceDownloadAllResources)
                {
                    return false;
                }

                // Grab header from remote URL to compare last modified date, for calculating length for progress bar, and to write meta
                Logger.Log("Checking resource '" + _resourceData.id + "'...", Logger.LogLevels.Debug);

                Networking.ContentHeaderData headerData;
                headerData = snobalCloudReference.RestClient.Head(_resourceData.uri.ToString());

                _resourceData.metaData = new ResourceMetaData
                {
                    dateTime = headerData.lastModifiedTime,
                    fileSize = headerData.fileSize,
                    etag = headerData.etag.ToString().Replace("\"", "") // Stripping out the quotes here isn't necessary but it simplifies things
                };

                // Check if local file exists?
                string filePath = Path.Combine(dataPath, _resourceData.FileName);
                if (!File.Exists(filePath))
                {
                    Logger.Log("Resource '" + filePath + "' does not exist.", Logger.LogLevels.Debug);
                    return false;
                }

                // Check for meta file?
                string metaFilePath = Path.Combine(dataPath, _resourceData.MetaFileName);
                if (!File.Exists(metaFilePath))
                {
                    Logger.Log("Resource '" + _resourceData.id + "' meta file '" + metaFilePath + "' does not exist.", Logger.LogLevels.Debug);
                    return false;
                }

                ResourceMetaData metaData;
                if (!Serialization.AttemptDeserializeToType<ResourceMetaData>(File.ReadAllText(metaFilePath), out metaData))
                {
                    Logger.Log("Resource '" + _resourceData.id + "' meta data using old outdated meta file and could not be deserialised, files do not match", Logger.LogLevels.Debug);
                    return false;
                }

                if (!metaData.Equals(_resourceData.metaData))
                {
                    Logger.Log("Resource '" + _resourceData.id + "': Meta data does not match server file", Logger.LogLevels.Debug);
                    return false;
                }

                /*
                 * Can't compare file sizes because the device version might be encrypted & a different size to the server.
                 * 
                // Check file size
                FileInfo fi = new FileInfo(filePath);  
                if(metaData.fileSize != fi.Length)
                {
                    Logger.Log("Resource '" + _resourceData.id + "': file size data does not match server file size", Logger.LogLevels.Debug);
                    return false;
                }
                */

                Logger.Log("Resource '" + _resourceData.id + "' is on latest version.", Logger.LogLevels.Debug);
                return true;
            }

            /// <summary> Starts downloading content </summary>
            public async Task DownloadContent(Action<ResourceData> _resourceDownloadedcallBack = null)
            {
                ErrorHandling.Test(DownloadProgress != null, "Resources.DownloadContent, DownloadProgress is not set");

                if (resourcesToDownload.Count > 0)
                {
                    Logger.Log("Downloading " + resourcesToDownload.Count + " resource(s)");
                    resourceDownloadedcallBack = _resourceDownloadedcallBack;
                    using (var semaphore = new SemaphoreSlim(maxConcurrentHeadRequests))
                    {
                        List<Task> tasks = new List<Task>();
                        for (int i = 0; i < resourcesToDownload.Count; ++i)
                        {
                            var resource = resourcesToDownload[i];
                            await semaphore.WaitAsync();
                            tasks.Add(Task.Run(() =>
                            {
                                DownloadAsset(resource);
                                semaphore.Release();
                                string metaFilePath = Path.Combine(FileIO.GetXRContentDataPath(), resource.MetaFileName);
                                File.WriteAllText(metaFilePath, Serialization.SerializeObject(resource.metaData));
                            }));
                        }
                        await Task.WhenAll(tasks.ToArray());
                    }
                }
            }

            /// <summary> Begins download of the specified asset </summary>
            void DownloadAsset(ResourceData _resourceData)
            {
                string filePath = Path.Combine(FileIO.GetXRContentDataPath(), _resourceData.FileName);
                Logger.Log("Downloading asset '" + _resourceData.uri + "' to '" + filePath + "'", Logger.LogLevels.Debug);

                if (!(snobalCloudReference.RestClient is AuthenticatedRestclient))
                {
                    throw new Library.SnobalException();
                }

                var authRestClient = snobalCloudReference.RestClient as AuthenticatedRestclient;
                var clientWithProgress = new Utilities.HttpClientWithProgress(authRestClient.HttpClient, _resourceData.uri, filePath);
                clientWithProgress.ProgressChanged += (totalBytesDownloaded, progressPercentage, progressBytesDownloaded) => { OnProgressUpdate(progressBytesDownloaded, _resourceData); };
                clientWithProgress.StartDownload((response) => OnDownloadComplete(response, _resourceData)).Wait();
            }

            void OnProgressUpdate(long _value, ResourceData _resourceData)
            {
                DownloadProgressBytes += _value;
                DownloadProgressPercentage = ((float)DownloadProgressBytes / totalDownloadSize) * 100;
                DownloadProgress.Report(DownloadProgressPercentage);
            }

            void OnDownloadComplete(System.Net.Http.HttpResponseMessage _response, ResourceData _resourceData)
            {
                completedResources++;

                resourceDownloadedcallBack?.Invoke(_resourceData);

                if (!_response.IsSuccessStatusCode)
                    throw new Exception("Web request returned error downloading '" + _resourceData.id + "':\n" + _response.StatusCode + ":" + _response.ReasonPhrase);
            }
        }
        #endregion
        #region File Uploading

        public class FileUploader
        {
            List<ResourceData> resourcesToUpload = new List<ResourceData>();

            string dataPath = FileIO.GetXRContentUploadPath();

            public IProgress<double> UploadProgress;

            private int maxConcurrentHeadRequests = defaultMaxConcurrentHeadRequests;
            public long totalDownloadSize { get; private set; } = 0;
            public long UploadProgressBytes { get; private set; } = 0;
            public float UploadProgressPercentage { get; private set; } = 0;
            public int completedResources { get; private set; } = 0;
            public int ResourcesRemaining { get { return resourcesToUpload.Count - completedResources; } }

            SnobalCloud snobalCloudReference;

            Action<ResourceData> resourceUploadedcallBack = null;

            public FileUploader(SnobalCloud _sc, int _maxConcurrentHeadRequests = defaultMaxConcurrentHeadRequests)
            {
                snobalCloudReference = _sc;
                maxConcurrentHeadRequests = _maxConcurrentHeadRequests;
            }
            public void SetResourcesToUpload(List<ResourceData> _resourcesToUpload)
            {
                resourcesToUpload = _resourcesToUpload;
            }
            public async Task CheckForContent(Action<ResourceData, bool> resourceUpToDatecallBack = null)
            {
                Logger.Log("Loading data from json...");
                LoadExperience(snobalCloudReference);
                Logger.Log("...loading data complete. Getting resources to download.");

                // Build queue of resources to check
                IEnumerable<string> resourceIds = GetResourceIds();
                List<ResourceData> resourceList = new List<ResourceData>();

                foreach (var id in resourceIds)
                    resourceList.Add(GetResourceData(id));

                await CheckForContent(resourceList, resourceUpToDatecallBack);
            }

            public async Task CheckForContent(List<ResourceData> resourceList, Action<ResourceData, bool> resourceUpToDatecallBack = null)
            {
                // Perform all head requests
                using (var semaphore = new SemaphoreSlim(maxConcurrentHeadRequests))
                {
                    List<Task> contentCheckTasks = new List<Task>();

                    foreach (var resource in resourceList)
                    {
                        await semaphore.WaitAsync();
                        contentCheckTasks.Add(Task.Run(() =>
                        {
                            try
                            {
                                if (!IsResourceUpToDate(resource))
                                {
                                    snobalCloudReference.Settings.Values.CachedContentComplete = false;
                                    resourcesToUpload.Add(resource);
                                    totalDownloadSize += resource.metaData.fileSize;
                                    resourceUpToDatecallBack?.Invoke(resource, false);
                                }
                                else
                                {
                                    resourceUpToDatecallBack?.Invoke(resource, true);
                                }
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }));
                    }

                    // Run all tasks
                    if (!Utilities.TaskWatcher.RunTasks(contentCheckTasks, 30000))
                    {
                        throw new SnobalException("CheckForContent tasks not completed, timeout occured.");
                    }
                }
            }

            /// <summary> Checks if there is an update to be Upload for a resource </summary>
            /// <param name="_resourceData"> Resource </param>
            bool IsResourceUpToDate(ResourceData _resourceData) //TODO: Need to look arround it
            {
                if (forceDownloadAllResources)
                {
                    return false;
                }

                // Grab header from remote URL to compare last modified date, for calculating length for progress bar, and to write meta
                Logger.Log("Checking resource '" + _resourceData.id + "'...", Logger.LogLevels.Debug);

                Networking.ContentHeaderData headerData;
                headerData = snobalCloudReference.RestClient.Head(_resourceData.uri.ToString());

                _resourceData.metaData = new ResourceMetaData
                {
                    dateTime = headerData.lastModifiedTime,
                    fileSize = headerData.fileSize,
                    etag = headerData.etag.ToString().Replace("\"", "") // Stripping out the quotes here isn't necessary but it simplifies things
                };

                // Check if local file exists?
                string filePath = Path.Combine(dataPath, _resourceData.FileName);
                if (!File.Exists(filePath))
                {
                    Logger.Log("Resource '" + filePath + "' does not exist.", Logger.LogLevels.Debug);
                    return false;
                }

                // Check for meta file?
                string metaFilePath = Path.Combine(dataPath, _resourceData.MetaFileName);
                if (!File.Exists(metaFilePath))
                {
                    Logger.Log("Resource '" + _resourceData.id + "' meta file '" + metaFilePath + "' does not exist.", Logger.LogLevels.Debug);
                    return false;
                }

                ResourceMetaData metaData;
                if (!Serialization.AttemptDeserializeToType<ResourceMetaData>(File.ReadAllText(metaFilePath), out metaData))
                {
                    Logger.Log("Resource '" + _resourceData.id + "' meta data using old outdated meta file and could not be deserialised, files do not match", Logger.LogLevels.Debug);
                    return false;
                }

                if (!metaData.Equals(_resourceData.metaData))
                {
                    Logger.Log("Resource '" + _resourceData.id + "': Meta data does not match server file", Logger.LogLevels.Debug);
                    return false;
                }

                Logger.Log("Resource '" + _resourceData.id + "' is on latest version.", Logger.LogLevels.Debug);
                return true;
            }

            /// <summary> Starts Upload content </summary>
            public async Task UploadContent(Action<ResourceData> _resourceUploadeddcallBack = null)
            {
                ErrorHandling.Test(UploadProgress != null, "Resources.UploadContent, UploadProgress is not set");
                Logger.Log("Resources count to upload: " + resourcesToUpload.Count + " resource(s)");
                if (resourcesToUpload.Count > 0)
                {
                    Logger.Log("Uploading " + resourcesToUpload.Count + " resource(s)");
                    resourceUploadedcallBack = _resourceUploadeddcallBack;
                    using (var semaphore = new SemaphoreSlim(maxConcurrentHeadRequests))
                    {
                        List<Task> tasks = new List<Task>();
                        for (int i = 0; i < resourcesToUpload.Count; ++i)
                        {
                            var resource = resourcesToUpload[i];
                            await semaphore.WaitAsync();
                            tasks.Add(Task.Run(() =>
                            {
                                string contentType = GetContentType(resource.type);
                                if (resource.type == ".json")
                                {
                                    UploadJsonFile(resource, contentType);
                                }
                                else
                                {
                                    UploadFile(resource);
                                }
                                
                                semaphore.Release();                                
                            }));
                        }
                        await Task.WhenAll(tasks.ToArray());
                    }
                }
            }

            private void UploadJsonFile(ResourceData resource, string contentType)
            {
                RequestUploadUrl(resource, contentType, (success, urlStr) =>
                {
                    if (success == true)
                    {
                        UploadAsset(resource, urlStr, (result) =>
                        {
                            if (result == true)
                            {
                                //Once uploaded to server, delete asset from local
                                if (File.Exists(resource.url))
                                {
                                    File.Delete(resource.url);
                                    DeleteFileCallback?.Invoke(resource.FileName + resource.type);
                                }
                            }
                        });
                    }
                });
            }

            private void UploadFile(ResourceData resource)
            {
                CreateAsset(resource, (success, assetId) =>
                {
                    if (success == true)
                    {
                        Snobal.Library.Logger.Log("asset id" + assetId + " success  " + success);
                        StartMasterUpload(resource, assetId, (success, url) =>
                        {
                            if (success == true)
                            {
                                Snobal.Library.Logger.Log("url: " + url);
                                UploadAsset(resource, url, (result) =>
                                {
                                    if (result == true)
                                    {
                                        //Once uploaded to server, delete asset from local
                                        string path = (Path.Combine(FileIO.GetXRContentUploadPath(), (resource.FileName + resource.type)));
                                        if (File.Exists(path))
                                        {
                                            File.Delete(path);
                                        }
                                        EndAssetTask(resource, assetId, (success) =>
                                        {
                                            if (success)
                                                Snobal.Library.Logger.Log("End task successfully with assetname " + resource.FileName);
                                        });
                                    }
                                });
                            }
                        });
                    }
                });
            }

            private void CreateAsset(ResourceData _resourceData, Action<bool, string> OnSuccessCreateAsset)
            {
                if (!(snobalCloudReference.RestClient is AuthenticatedRestclient))
                {
                    throw new Library.SnobalException();
                }
                //Url to create an asset
                string url = snobalCloudReference.Settings.Values.TenantURL + API.requestCreateAsset;
                Networking.WebResponse webResponse = null;
                try
                {
                    var authRestClient = snobalCloudReference.RestClient as AuthenticatedRestclient;
                    UploadAsset createAsset = new UploadAsset();
                    createAsset.assetName = _resourceData.FileName + _resourceData.type;
                    string email = snobalCloudReference.Participant?.email;
                    createAsset.creator = email;
                    string body = JsonConvert.SerializeObject(createAsset, Formatting.Indented);
                    Snobal.Library.Logger.Log("Create Asset body " + body);
                    webResponse = authRestClient.Post(url, body);
                }
                catch (Exception e)
                {
                    Snobal.Library.Logger.Log("Error occur while creating an asset" + e);
                }
                if (!webResponse.Success)
                {
                    Logger.Log("requesting create asset Error: '" + url + "' returned http code: " + webResponse.HttpStatusCode + ", web response: " + webResponse.Response);
                    OnSuccessCreateAsset?.Invoke(false, null);
                }
                if (webResponse.Success)
                {
                    Logger.Log("Success create asset Request '" + url + "' returned http code: " + webResponse.HttpStatusCode + ", web response: " + webResponse.Response);

                    CreateAssetResponse response = JsonConvert.DeserializeObject<CreateAssetResponse>(webResponse.Response);
                    OnSuccessCreateAsset?.Invoke(true, response.assetId);
                }

            }
            private void StartMasterUpload(ResourceData _resourceData, string assetId, Action<bool, string> OnSuccessUpload)
            {
                if (!(snobalCloudReference.RestClient is AuthenticatedRestclient))
                {
                    throw new Library.SnobalException();
                }
                //Url to create an asset
                string url = snobalCloudReference.Settings.Values.TenantURL + API.startMasterUpload;
                Networking.WebResponse webResponse = null;
                try
                {
                    var authRestClient = snobalCloudReference.RestClient as AuthenticatedRestclient;
                    MasterUpload uploadObj = new MasterUpload();
                    uploadObj.filename = _resourceData.FileName + _resourceData.type;
                    string email = snobalCloudReference.Participant?.email;
                    uploadObj.creator = email;
                    uploadObj.assetId = assetId;
                    string body = JsonConvert.SerializeObject(uploadObj, Formatting.Indented);
                    Snobal.Library.Logger.Log("StartAssetBody " + body);
                    webResponse = authRestClient.Post(url, body);
                }
                catch (Exception e)
                {
                    Snobal.Library.Logger.Log("Error occur while creating an asset" + e);
                }
                if (!webResponse.Success)
                {
                    Logger.Log("requesting startmasterupload Error: '" + url + "' returned http code: " + webResponse.HttpStatusCode + ", web response: " + webResponse.Response);
                    OnSuccessUpload?.Invoke(false, null);
                }
                if (webResponse.Success)
                {
                    Logger.Log("Success startmasterupload Request '" + url + "' returned http code: " + webResponse.HttpStatusCode + ", web response: " + webResponse.Response);

                    MasterUploadResponse response = JsonConvert.DeserializeObject<MasterUploadResponse>(webResponse.Response);
                    OnSuccessUpload?.Invoke(true, response.access.endpoint);
                }


            }

            private void EndAssetTask(ResourceData _resourceData, string assetID, Action<bool> OnSuccess)
            {
                if (!(snobalCloudReference.RestClient is AuthenticatedRestclient))
                {
                    throw new Library.SnobalException();
                }
                //Url to create an asset
                string url = snobalCloudReference.Settings.Values.TenantURL + API.endMasterUpload;
                Networking.WebResponse webResponse = null;
                try
                {
                    var authRestClient = snobalCloudReference.RestClient as AuthenticatedRestclient;
                    MasterUpload asset = new MasterUpload();
                    asset.assetId = assetID;
                    string email = snobalCloudReference.Participant?.email;
                    asset.creator = email;
                    string body = JsonConvert.SerializeObject(asset, Formatting.Indented);
                    webResponse = authRestClient.Post(url, body);
                }
                catch (Exception e)
                {
                    Snobal.Library.Logger.Log("Error occur while creating an asset" + e);
                }
                if (!webResponse.Success)
                {
                    Logger.Log("requesting end asset Error: '" + url + "' returned http code: " + webResponse.HttpStatusCode + ", web response: " + webResponse.Response);
                    OnSuccess?.Invoke(false);
                }
                if (webResponse.Success)
                {
                    Logger.Log("Success end Request '" + url + "' returned http code: " + webResponse.HttpStatusCode + ", web response: " + webResponse.Response);
                    OnSuccess?.Invoke(true);
                }

            }

            private void RequestUploadUrl(ResourceData _resourceData, string contentType, Action<bool, string> OnSuccessRequestURL)
            {
                if (!(snobalCloudReference.RestClient is AuthenticatedRestclient))
                {
                    throw new Library.SnobalException();
                }

                if (File.Exists(_resourceData.url))
                {
                    string url = API.requestUploadAssetUrl;

                    Networking.WebResponse webResponse = null;

                    try
                    {
                        string key = GetSHA1KeyFromTenantName();
                        Networking.Header[] headers =
                            { new Networking.Header { key = "X-Api-Key", value = key } };

                        var authRestClient = snobalCloudReference.RestClient as AuthenticatedRestclient;

                        PathBase pathbase = new PathBase();
                        if (_resourceData.url == FileIO.GetXRContentUploadPath())
                        {
                            pathbase.path = API.requestUploadPathExtension + (_resourceData.FileName + _resourceData.type);
                        }
                        else
                        {
                            //If the file is inside any folder of the base URL
                            pathbase.path = GetResourceFolderName(_resourceData.url) + (_resourceData.FileName + _resourceData.type);
                        }

                        pathbase.ContentType = contentType;

                        string body = JsonConvert.SerializeObject(pathbase, Formatting.Indented);

                        webResponse = authRestClient.Post(url, body, headers);
                    }
                    catch (Exception e)
                    {
                        Logger.Log("Error occur while requesting uploading " + e + " , URL " + url);
                    }

                    if (!webResponse.Success)
                    {
                        Logger.Log("requesting uploading Error: '" + url + "' returned http code: " + webResponse.HttpStatusCode + ", web response: " + webResponse.Response);
                        OnSuccessRequestURL?.Invoke(false, null);
                    }
                    if (webResponse.Success)
                    {
                        Logger.Log("Success Request '" + url + "' returned http code: " + webResponse.HttpStatusCode + ", web response: " + webResponse.Response);

                        UploadURL uploaduRL = JsonConvert.DeserializeObject<UploadURL>(webResponse.Response);
                        OnSuccessRequestURL?.Invoke(true, uploaduRL.url);
                    }
                }

            }

            private string GetResourceFolderName(string url)
            {
                System.Text.StringBuilder folderName = new System.Text.StringBuilder(url);
                folderName = folderName.Replace(FileIO.GetXRContentUploadPath(), "");
                int index = folderName.ToString().IndexOf(Path.DirectorySeparatorChar); //Index of the '\' character
                folderName = folderName.Remove(index, folderName.Length - index);
                folderName = folderName.Append(Path.AltDirectorySeparatorChar);
                return folderName.ToString();
            }


            private string GetSHA1KeyFromTenantName()
            {
                string tenantUrl = snobalCloudReference.Tenant.GetTenantURL();

                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    //We want to remove the first 8 characters of the URL
                    //e.g. turn https://demo-sphere-dev.snobal.net to demo-sphere-dev.snobal.net
                    var tURL = tenantUrl[8..];
                    var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(tURL));
                    string key = string.Concat(hash.Select(b => b.ToString("x2")));

                    return key;
                }
            }

            /// <summary> Begins Uploadin of the specified asset </summary>
            void UploadAsset(ResourceData _resourceData, string url, Action<bool> OnSuccessUpload)
            {
                if (!(snobalCloudReference.RestClient is AuthenticatedRestclient))
                {
                    throw new Library.SnobalException();
                }

                if (File.Exists(_resourceData.url))
                {
                    var authRestClient = snobalCloudReference.RestClient as AuthenticatedRestclient;

                    Networking.WebResponse webResponse = null;

                    try
                    {
                        webResponse = authRestClient.Put(url, "", _resourceData.url, GetContentType(_resourceData.type), null);

                    }
                    catch (Exception e)
                    {
                        Logger.Log("Error occur while uploading " + e + " , URL " + url);
                    }
                    finally
                    {
                        OnUploadComplete(webResponse, _resourceData);
                    }

                    if (!webResponse.Success)
                    {
                        Logger.Log("uploading Error: '" + url + "' returned http code: " + webResponse.HttpStatusCode + ", web response: " + webResponse.Response);
                        OnSuccessUpload?.Invoke(false);
                    }

                    if (webResponse.Success)
                    {
                        // TODO: delete file here from local directory
                        Logger.Log("Success  Upload'" + url + "' returned http code: " + webResponse.HttpStatusCode + ", web response: " + webResponse.Response);
                        OnSuccessUpload?.Invoke(true);
                    }
                }
            }

            void OnProgressUpdate(long _value, ResourceData _resourceData)
            {
                UploadProgressBytes += _value;
                UploadProgressPercentage = ((float)UploadProgressBytes / totalDownloadSize) * 100;
                UploadProgress.Report(UploadProgressPercentage);
            }

            void OnUploadComplete(Networking.WebResponse _response, ResourceData _resourceData)
            {
                completedResources++;

                resourceUploadedcallBack?.Invoke(_resourceData);

                if (!_response.Success)
                    throw new Exception("Web request returned error uploading '" + _resourceData.id + "':\n" + _response.HttpStatusCode + ":" + _response.Response);
            }

            string GetContentType(string type)
            {
                string contentType = null;
                switch (type)
                {
                    case ".png": contentType = "image/png"; break;
                    case ".json": contentType = "application/json"; break;
                }
                return contentType;
            }
        }
        #endregion
    }

    [Serializable]
    public class PathBase
    {
        public string path;
        public string ContentType;
    }
    [Serializable]
    public class UploadURL
    {
        public string url;
    }
    [Serializable]
    public class UploadAsset
    {
        public string assetName;
        public string creator;
    }
    [Serializable]
    public class CreateAssetResponse
    {
        public string assetId;
        public string assetName;
    }
    [Serializable]
    public class MasterUpload
    {
        public string assetId;
        public string filename;
        public string creator;
    }
    [Serializable]
    public class MasterUploadResponse
    {
        public bool necessary;
        public Access access;
    }
    [Serializable]
    public class Access
    {
        public string endpoint;
    }

}
#pragma warning restore 649
