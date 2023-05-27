/// <summary>
/// Lists all the API calls between this app & the platform.
/// 
/// 5/Oct/2022 Cleanup. Removed unused API calls & code for:
/// public const string openReadyExtension = @"/device/api/scope/v1/open-ready";
/// public const string closeReadyExtension = @"/device/api/scope/v1/close-ready";
/// public const string openControlExtension = @"/device/api/scope/v1/open-control";
/// public const string closeControlExtension = @"/device/api/scope/v1/close-control";
/// public const string sessionEndExtension = @"/device/api/session/v2/end";
/// public const string startOnDemand = @"/device/api/session/v2/start-on-demand";
/// public const string availableExperiences = @"/device/api/session/v2/available-experiences/"; //device/api/session/v2/available-experiences/:participantId
/// public const string buildExtension = @"/device/api/experience/v1/build";
/// ReadyScope
/// ControlScope
/// SessionManager
/// Session
/// Experiences
/// remove all DISABLE_LEGACY_ACTIVITIES code
/// 
/// </summary>
namespace Snobal.Library
{
    internal static class API
    {
        // Pairing
        public const string pairingUrl = @"https://pairing.snobal.net/device/api/pairing/v1/pair-device";

        // Testing
        public const string initialPingUrl = @"http://pairing.snobal.net/hello";

        // Video streaming to tenant site. Been some time since we used this. Delete?  Frontend currently does not support.
        public const string streamingMode = @"/device/api/video-streaming/v1/get-mode";
        public const string signallingConfiguration = @"/device/api/video-streaming/v1/signalling-config";
        public const string openChannel = @"/device/api/video-streaming/v1/open-channel";
        public const string closeChannel = @"/device/api/video-streaming/v1/close-channel";
        public const string channelConsumer = @"/device/api/video-streaming/v1/get-channel-consumer/"; //device/api/video-streaming/v1/get-channel-consumer/:channelId
        public const string videoStreamingLicense = @"/license/v1/get/video-streaming";

        // Logging/Events
        public const string eventDatabaseExtension = @"/device/api/events/v1/raise";

        // Login/out
        public const string loginExtension = @"/device/auth/v1/login";
        public const string logoutExtension = @"/device/auth/v1/logout";
        public const string validatePasscode = @"/device/api/session/v2/identify-participant";
        
        // Schemes
        public const string getSchemeExtension = @"/device/api/ar/scheme/v1/get/";
        public const string startSchemeExtension = @"/device/api/ar/scheme/v1/start/";
        public const string endSchemeExtension = @"/device/api/ar/scheme/v1/end/";

        // Tasks
        public const string startTaskExtension = @"/device/api/ar/task/v1/start";
        public const string endTaskExtension = @"/device/api/ar/task/v1/end";
        
        // Presentations
        public const string listPresentationExtension = @"/device/api/presentation/v3/list";

        // Software
        public const string installerListExtension = @"/software/v1/list-installers";
        public const string installerAPKExtension = @"/software/v1/installer-apk";
        public const string cloudApplicationVersion = @"/software/v1/get-version-apk";

        // License
        public const string licenseExtension = @"/license/v1/get/";

        // Assets
        public const string availableMediaListExtension = @"/dam/v2/asset/media/list-available-assets";
        public const string allMediaListExtension = @"/dam/v2/asset/media/list-assets";

        // Deployment profiles 
        public const string defaultProfileURL = @"/device/api/profile/v1/get";

        // Misc
        public const string metaDataExtension = @"/api/company/v1/metadata";
        public const string keepAliveExtension = @"/device/_session/keepalive";
        public const string telemetryExtension = @"/device/api/reporting/v1/telemetry";
        public const string binaryDownloadExtension = @"/device/api/app/v1/metadata";

        // Transcode stream url details
        public const string transcodeDataExtension = @"/dam/v2/transcoded-url/media/";     // https://<tenant>//dam/v2/transcoded-url/media/<assetId>

        //Upload Assets
        public const string requestUploadAssetUrl = "https://iv1fq4ulo8.execute-api.ap-southeast-2.amazonaws.com/prod/getUploadUrl";
        public const string requestCreateAsset = @"/dam/v2/asset/screenshot/create-asset";
        public const string startMasterUpload = @"/dam/v2/asset/screenshot/start-master-upload";
        public const string endMasterUpload = @"/dam/v2/asset/screenshot/end-master-upload";
        public const string requestUploadPathExtension = "/cdn/files/uploaded/";
        public struct BadRequestErrorObject
        {
            public BadRequestError error;
        }
        public struct BadRequestError
        {
            public string message;
            public int code;
            public object details;
        }

        public static class Errors
        {
            public const int NO_SESSION_CONFIGURED = 524353;
            public const int NO_SESSION_CONFIGURED_OLD = 32833; // Remove after backend changes go live
            public const int NO_SUCH_CHANNEL = 917539;
            public const int NO_SUCH_CHANNEL_OLD = 49699; // Remove after backend changes go live
        }
    }
}
