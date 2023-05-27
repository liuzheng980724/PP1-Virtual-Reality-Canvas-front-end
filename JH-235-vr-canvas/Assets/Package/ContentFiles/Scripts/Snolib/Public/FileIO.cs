using System;
using System.IO;
using System.Threading.Tasks;
using Snobal.Security_0_0;
using System.Text;

namespace Snobal.Library
{
    public static class FileIO
    {
        
        public const bool defaultEncryptDownloadedContent = true;
        public static bool encryptDownloadedContent { get; set; } = defaultEncryptDownloadedContent;

        public static bool isInitialised = false;
        
        #region Consts

        private const string CacheFileName = "cache.json";
        private const string DefaultErrorLogExtension = "snolib.log";
        private const string NetworkDebugLogExtension = "network-debug.log";
        
        #endregion

        private static string _snobalSharedFilesPath = null;
        private static string _appDataFilesPath = null;

        /// We have moved away from using Application.persistentDataPath. It requires us to have to do special work to get access to files in another
        /// apps persistentDataPath and at the end of the day, it's not protected from users going onto the device and deleting the data anyway
        /// For windows/editor 'c:/SnobalSharedFiles'
        /// For Android: "/storage/emulated/0/SnobalSharedFiles";
        /// However we want our regular app files like the logging stuff and data to remain in the app specific folder.
        /// Both we can manually create and access with the correct permissions in android.
        public static void Init(string sharedFilePath, string appDataFilePath)
        {
            isInitialised = true;
            _snobalSharedFilesPath = sharedFilePath;
            _appDataFilesPath = appDataFilePath;
            
            if (!Directory.Exists(_snobalSharedFilesPath))
            {
                Directory.CreateDirectory(_snobalSharedFilesPath);
            }
        }

        public static void Shutdown() => isInitialised = false;

        /// <summary>
        /// All our app data stays in the apps folders
        /// </summary>
        public static string GetAppDataPath => _appDataFilesPath;

        /// <summary>
        /// the Android Manifest and the SnobalCloudSettings now reside in this new folder
        /// </summary>
        public static string GetSharedFilesPath => _snobalSharedFilesPath;

        public static string GetXRContentDataPath()
        {
            var dataPath = Path.Combine(GetAppDataPath, "Cache" + Path.DirectorySeparatorChar);
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            return dataPath;
        }

        public static string GetXRContentUploadPath()
        {
            var dataPath = Path.Combine(GetAppDataPath, "Upload" + Path.DirectorySeparatorChar);
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            return dataPath;
        }

        public static string GetCacheFilePath() => Path.Combine(GetAppDataPath, CacheFileName);

        public static string GetNetworkDebugFileLocation() => Path.Combine(GetAppDataPath, NetworkDebugLogExtension);
        
        public static string GetDatabaseFilePath(string fileName) => Path.Combine(GetAppDataPath, $"{fileName}.sqlite");
        
        public static void DeleteLocalCache()
        {
            var cacheFilePath = GetCacheFilePath();
            var experienceFolder = GetSubExperienceFilePath();
            var cacheFolder = GetXRContentDataPath();

            if (File.Exists(cacheFilePath))
                File.Delete(cacheFilePath);

            if (Directory.Exists(experienceFolder))
                Directory.Delete(experienceFolder, true);

            if (Directory.Exists(cacheFolder))
                Directory.Delete(cacheFolder, true);
        }

        public static void DeleteSnobalData()
        {
            if (Directory.Exists(_appDataFilesPath))
            {
                Directory.Delete(_appDataFilesPath, true);
            }
            
            if (Directory.Exists(_snobalSharedFilesPath))
            {
                Directory.Delete(_snobalSharedFilesPath, true);
            }
        }
        
        public static string GetDatabaseDataPath()
        {
            var path = Path.Combine(GetAppDataPath, "Data" + Path.DirectorySeparatorChar);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        public static string GetSubExperienceFilePath(string filename = null)
        {
            var path = Path.Combine(GetAppDataPath, "Experience");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return filename == null ? path : Path.Combine(path, $"{filename}.json");
        }

        public static string GetErrorFileLocation(string logfileName = null)
        {
            if (logfileName != null)
            {
                return Path.Combine(GetAppDataPath, logfileName);
            }
            else
            {
                return Path.Combine(GetAppDataPath, DefaultErrorLogExtension);
            }
        }

        public static byte[] ReadAllBytes(string path)
        {
            try
            {
                if (encryptDownloadedContent)
                {
                    byte[] encrytedBytes = File.ReadAllBytes(path);
                    return Encryption.Decrypt(encrytedBytes);
                }
                else
                {
                    return File.ReadAllBytes(path);
                }
            }
			catch (Exception e)
			{
				throw new SnobalException("Couldn't open or decrypt " + path + ", " + e.Message, "An unexpected error occured", e);
			}
        }

        public static string ReadAllText(string path)
        {
            try
            {
                if (encryptDownloadedContent)
                {
                    byte[] encrytedBytes = File.ReadAllBytes(path);
                    byte[] decrytedBytes =  Encryption.Decrypt(encrytedBytes);
                    return Encoding.UTF8.GetString(decrytedBytes);
                }
                else
                {
                    return File.ReadAllText(path);
                }
            }
			catch (Exception e)
			{
				throw new SnobalException("Couldn't open or decrypt " + path + ", " + e.Message, "An unexpected error occured", e);
			}
        }

        public static async Task DownloadStreamToFile(long? _totalDownloadSize, Stream _contentStream, string _destinationFilePath, Action<long?, long, long> _progressCallBack)
        {
            var totalBytesRead = 0L;
            var readCount = 0L;
            var buffer = new byte[8192];
            var isMoreToRead = true;
            Stream outputStream;

            var fileStream = new FileStream(_destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            //Logger.Log("Downloading " + _destinationFilePath + ", encryptDownloadedContent = " + encryptDownloadedContent.ToString());
            
            // Encrypt the content?
            if (encryptDownloadedContent)
            {
                outputStream = Encryption.EncryptStream(fileStream);
            }
            else
            {
                outputStream = fileStream;
            }

            using (outputStream)
            {
                do
                {
                    var bytesRead = 0;

                    try
                    {
                        bytesRead = await _contentStream.ReadAsync(buffer, 0, buffer.Length);
                    }
                    catch (Exception e)
                    {
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        isMoreToRead = false;

                        _progressCallBack?.Invoke(_totalDownloadSize.Value, totalBytesRead, bytesRead);
                        continue;
                    }

                    try
                    {
                        await outputStream.WriteAsync(buffer, 0, bytesRead);
                    }
                    catch (Exception e)
                    {
                        break;
                    }

                    totalBytesRead += bytesRead;
                    readCount += 1;

                    _progressCallBack?.Invoke(_totalDownloadSize.Value, totalBytesRead, bytesRead);
                } 
                while (isMoreToRead);
            }

            //the last progress trigger should occur after the file handle has been released or you may get file locked error
            _progressCallBack?.Invoke(_totalDownloadSize.Value, totalBytesRead, _totalDownloadSize.HasValue ? _totalDownloadSize.Value - totalBytesRead : 0);
        }

        public static Task DownloadFileTask(string _url, string _path, System.Net.Http.HttpClient httpClient)
        {
            var response = httpClient.GetAsync(_url).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new SnobalException(
                    string.Format("HTTP status {0} downloading '{1}'", (int)response.StatusCode, _url), 
                    "Error requesting file content"
                );
            }

            Stream outputStream;

            var fileStream = new FileStream(_path, FileMode.Create);

            //Logger.Log("Downloading " + _path + ", encryptDownloadedContent = " + FileIO.encryptDownloadedContent.ToString());
            
            // Encrypt the content?
            if (FileIO.encryptDownloadedContent)
            {
                outputStream = Encryption.EncryptStream(fileStream);
            }
            else
            {
                outputStream = fileStream;
            }

            using (outputStream)
            {
                response.Content.CopyToAsync(outputStream).Wait();
            }

            return Task.CompletedTask;
        }


    }
}

