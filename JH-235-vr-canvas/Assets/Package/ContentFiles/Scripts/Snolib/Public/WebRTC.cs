using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snobal.Utilities;

namespace Snobal.Library.Networking
{
    public static class WebRTC
    {
        public enum StreamingModes { Default, Unavailable, OnDemand, Always };
        public enum LicenseTypes { Default, NotLicensed, Licensed };
        static LicenseTypes license = LicenseTypes.Default;
        private static IRestClient _restClient;
        struct LicenseResponse
        {
            public bool enabled;
        }

        public static void Initialise(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public static LicenseTypes GetLicense(SnobalCloud sc)
        {
            if (license != LicenseTypes.Default)
                return license;

            var responseString = _restClient.Get(sc.Tenant.BuildTenantAPIURL(API.videoStreamingLicense)).Response;
            LicenseResponse response = Serialization.DeserializeToType<LicenseResponse>(responseString);
            if (response.enabled)
                license = LicenseTypes.Licensed;
            else
                license = LicenseTypes.NotLicensed;

            return license;
        }

        struct StreamingModeResponse
        {
            public string mode;
        }
        public static StreamingModes GetStreamingMode(SnobalCloud sc)
        {
            string response = _restClient.Get(sc.Tenant.BuildTenantAPIURL(API.streamingMode)).Response;
            StreamingModeResponse streamingModeResponse = Serialization.DeserializeToType<StreamingModeResponse>(response);
            switch (streamingModeResponse.mode)
            {
                case "onDemand":
                    return StreamingModes.OnDemand;
            }
            throw new SnobalException("Backend returned an invalid streaming mode, " + streamingModeResponse.mode, "An unexpected error occured");
        }

        public class SignallingConfiguration
        {
            public string server;
            public IceServer[] iceServers;
        }

        public class IceServer
        {
            public string uri;
            public string username;
            public string password;
        }

        public static SignallingConfiguration GetSignallingConfiguration(SnobalCloud sc)
        {
            string response = _restClient.Get(sc.Tenant.BuildTenantAPIURL(API.signallingConfiguration)).Response;
            SignallingConfiguration signallingConfiguration = Serialization.DeserializeToType<SignallingConfiguration>(response);
            return signallingConfiguration;
        }

        [Serializable]
        struct OpenChannelData
        {
            public string channelId;
            public string type;
            public string name;
        }

        public static void OpenChannel(string _channelId, SnobalCloud sc)
        {
            string data = Serialization.SerializeObject(new OpenChannelData
            {
                channelId = _channelId,
                type = "standard",
                name = "Device"
            });
            var response = _restClient.Post(sc.Tenant.BuildTenantAPIURL(API.openChannel), data);
            if (!response.Success)
            {
                throw new SnobalException("Opening new channel failed, response: " + response.Response, "An unexpected error occured");
            }
        }

        struct CloseChannelData
        {
            public string channelId;
        }

        public static void CloseChannel(string _channelId, SnobalCloud sc)
        {
            string data = Serialization.SerializeObject(new CloseChannelData
            {
                channelId = _channelId,
            });
            var response = _restClient.Post(sc.Tenant.BuildTenantAPIURL(API.closeChannel), data);
            if (!response.Success)
            {
                API.BadRequestErrorObject error = Serialization.DeserializeToType<API.BadRequestErrorObject>(response.Response);
                if (error.error.code == API.Errors.NO_SUCH_CHANNEL || error.error.code == API.Errors.NO_SUCH_CHANNEL_OLD)
                    return; // If we try to close a channel that's already closed.. not really an issue
                throw new SnobalException("Closing channel failed, response: " + response.Response, "An unexpected error occured");
            }
        }

        public struct ConsumerDataObject
        {
            public ConsumerData? consumer; // If this is null, channel has not been claimed yet
        }

        public struct ConsumerData
        {
            public string name;
            public string email;
        }

        public static void GetChannelConsumerData(string _channelId, out ConsumerDataObject _consumerDataObject, SnobalCloud sc)
        {
            string url = sc.Tenant.BuildTenantAPIURL(API.channelConsumer) + _channelId;
            var response = _restClient.Get(url);

            _consumerDataObject = Serialization.DeserializeToType<ConsumerDataObject>(response.Response);

        }

        [Serializable]
        public class VideoConfiguration
        {
            public string transmitMicrophoneAudio;
            public string videoWidth;
            public string videoHeight;
        }

        public static VideoConfiguration GetVideoConfiguration()
        {
            return new VideoConfiguration
            {
                transmitMicrophoneAudio = "true",
                videoWidth = "1920",
                videoHeight = "1080"
            };
        }
    }
}
