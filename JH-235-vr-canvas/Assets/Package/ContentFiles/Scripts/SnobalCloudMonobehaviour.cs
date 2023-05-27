using Snobal.Library;
using Snobal.Utilities;
using UnityEngine;
using UnityEngine.Profiling;
using Logger = UnityEngine.Logger;

namespace Snobal.CloudSDK
{
    // This class is designed to be inherited from when deployed in a target project.

    public class SnobalCloudMonobehaviour : MonoBehaviour
    {
        // DATA
        public static SnobalCloud SnobalCloudInstance = null;
        public static Snobal.Utilities.TaskWatcher TaskWatcher { get; private set; }

        [SerializeField]
        private bool encryptDownloadedContent = false;

        protected static bool encryptingDownloadedContent { get; private set; } = false;

        void Awake()
        {
            // Remember if encryption is turned on
            encryptingDownloadedContent = encryptDownloadedContent;

            SnobalCloudInitialization.Initalise();
        }

        public static void InitaliseDone()
        {
            SnobalCloudInstance = new SnobalCloud();
            
#if !UNITY_EDITOR && UNITY_ANDROID
            // Evaluate the android intents, note: has to be done after the SnobalCloud class has been created
            EvaluateAndroidIntents();
#endif

#if SNOBAL_CLOUD_DEPLOY
            // Cloud deploy shouldn't encrypt any downloaded files. ie dont encrypt downloaded .apk files etc
            SnobalCloudInstance.EncryptDownloadedContent = false;
#else
            SnobalCloudInstance.EncryptDownloadedContent = encryptingDownloadedContent;
#endif

            // Check if the encryption flag has changed.
            if (SnobalCloudInstance.Settings.Values.EncryptedContents != SnobalCloudInstance.EncryptDownloadedContent)
            {
                Library.Logger.Log("Encryption flag on contents has changed. Deleting all local cache files for re-download.");
                SnobalCloudInstance.Settings.Values.EncryptedContents = SnobalCloudInstance.EncryptDownloadedContent;       // save happens below
                FileIO.DeleteLocalCache();
            }

            TaskWatcher = new Utilities.TaskWatcher(SnobalErrorHandler.Throw);
        }

        /// <summary>
        /// Evaluate if various Android Intents have been passed to the app.
        /// </summary>
        private static void EvaluateAndroidIntents()
        {
            // Set the SnobalCloudSettings data
            var settingsJson = AndroidExternalParameters.GetParameter(ExternalParameterType.Settings);
            if (settingsJson != null)
            {
                var tempSettings = Serialization.DeserializeToType<SnobalCloudSettings>(settingsJson);
                if (tempSettings != null)
                {
                    // Update and save the settings
                    SnobalCloudInstance.Settings.Write(tempSettings);
                }
            }
            // Set the Participant data
            var participantJsonString = AndroidExternalParameters.GetParameter(ExternalParameterType.Participant);
            if (participantJsonString!=null)
            {
                var newParticipant  = Participant.Deserialize(participantJsonString);
                if (newParticipant != null)
                {
                    SnobalCloudInstance.Participant = newParticipant;
                }
            }
        }

        void Update()
        {
            if(TaskWatcher != null)
            {
                TaskWatcher.Update();
            }
        }

        void OnApplicationQuit()
        {
            if(Library.Logger.isInitialised)    // check because we could shutdown before this to uninstall etc
            {
                Library.Logger.Log("Application has quit");
            }
            SnobalCloudInitialization.Shutdown();
        }
    }
}
