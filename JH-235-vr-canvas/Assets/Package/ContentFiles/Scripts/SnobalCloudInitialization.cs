using UnityEngine;
using Snobal.Library;
using Snobal.EnumFsmNET_0_1;
using Logger = Snobal.Library.Logger;

namespace Snobal.CloudSDK
{
    public static class SnobalCloudInitialization
    {
        private const string androidPath = "/storage/emulated/0/SnobalSharedFiles";
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            //Debug.Log($"*************************************************************** {SystemInfo.deviceUniqueIdentifier.ToString()}");

            Debug.Log("<color=blue>Runtime initialization of Snobal Cloud services Started</color>");

            // Evaluate parameters for windows builds, eg: botmode
            if (StartedInBotClientMode())
            {
                // If we have started in bot mode we could be running 
                FileIO.Init("C:\\SnobalBot", "C:\\SnobalBot");

                var botname = WindowsExternalParameters.GetCommandLineArgValue(WindowsExternalParameters.Botname);
                ErrorHandling.Test(botname != null, "Error: No bot name for bot client!");
                var logfileName = "snolib_" + botname + ".log";
                Logger.Init(Logger.LogLevels.Info, logfileName);
            }
            else
            {
                // initialise our file system
#if UNITY_EDITOR
                FileIO.Init(Application.persistentDataPath, Application.persistentDataPath);
#else
                FileIO.Init(androidPath, Application.persistentDataPath);
#endif
                
                Logger.Init(Logger.LogLevels.Info);
            }

            Logger.AddCustomLogger(new UnityDebugLogger());
            Logger.Log(new string('=', 80));
            Logger.Log("Application version = " + Application.version);
            Logger.Log("Network connectivity is " + Application.internetReachability.ToString());
        }

        /// <summary>
        /// Checks the command line arguments for "botmode" to check if we were started in bot client mode.
        /// </summary>
        /// <returns></returns>
        public static bool StartedInBotClientMode()
        {
            return WindowsExternalParameters.StartedInBotClientMode();
        }

        public static void Initalise()
        {
            AllSettingsFilesReady();
            
            // If in bot mode, disable done create local http file server (multiple instances can't use the same port. Could make this smarter if required)
            if (!WindowsExternalParameters.StartedInBotClientMode())
            {
                SnobalCloudHTTPFileServer.Create();
            }

            // Log all state changes
            EnumFsmStatic.newStateCallback += StateChange;
        }

        private static void StateChange(string newState)
        {
            if (Logger.isInitialised) Logger.Log("EnumFsm - State change, new state " + newState);
        }

        private static void AllSettingsFilesReady()
        {
            Events.Init("application");

            SnobalErrorHandler.Init();

            Debug.Log("<color=blue>Runtime initialization of Snobal Cloud services Complete</color>");

            SnobalCloudMonobehaviour.InitaliseDone();
        }

        /// <summary>
        /// Create a login data struct to record details about this device. This is optional, but useful to track device details on every device logging in.
        /// </summary>
        /// <returns></returns>
        public static Library.Scopes.Login.Data.OpenData CreateLoginData()
        {
            Library.Scopes.Login.Data.OpenData data = new Library.Scopes.Login.Data.OpenData
            {
                details =
                {
                    device =
                    {
                        batteryLevel = UnityEngine.SystemInfo.batteryLevel,
                        deviceModel = UnityEngine.SystemInfo.deviceModel,
                        deviceName = UnityEngine.SystemInfo.deviceName,
                        deviceUniqueIdentifier = UnityEngine.SystemInfo.deviceUniqueIdentifier,
                        graphicsDeviceName = UnityEngine.SystemInfo.graphicsDeviceName,
                        graphicsDeviceVendor = UnityEngine.SystemInfo.graphicsDeviceVendor,
                        graphicsMemorySize = UnityEngine.SystemInfo.graphicsMemorySize,
                        operatingSystem = UnityEngine.SystemInfo.operatingSystem,
                        processorType = UnityEngine.SystemInfo.processorType,
                        systemMemorySize = UnityEngine.SystemInfo.systemMemorySize,
                    }
                }
            };

            return data;
        }


        public static void Shutdown()
        {
            Logger.Log("Shutting down Snobal Cloud Services");

            //Snobal.Library.Networking.Http.Shutdown();
            SnobalErrorHandler.Shutdown();
            Events.Shutdown();

            Logger.Shutdown();
            FileIO.Shutdown();

            SnobalCloudHTTPFileServer.Stop();

            Debug.Log("<color=blue>Runtime shutdown of Snobal Cloud services complete</color>");
        }
    }
}