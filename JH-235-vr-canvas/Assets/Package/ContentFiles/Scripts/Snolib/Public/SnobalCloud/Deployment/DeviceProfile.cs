using System;
using Snobal.CloudSDK;

namespace Snobal.Library
{
    [Serializable]
    public class DeviceProfile
    {
        public static SnobalCloud SnobalCloud { get { return SnobalCloudMonobehaviour.SnobalCloudInstance; } }

        public string name;
        public ProfileDefinition definition;

        internal static DeviceProfile DownloadDeviceProfile(string _path)
        {
            string url = SnobalCloudMonobehaviour.SnobalCloudInstance.Tenant.BuildTenantAPIURL(_path);

            var authRestClient = SnobalCloud.RestClient as Snobal.Library.AuthenticatedRestclient;

            var response = authRestClient.Get(url);

            DeviceProfileResponse deserialisedResponse = Snobal.Utilities.Serialization.DeserializeToType<DeviceProfileResponse>(response.Response);

            return deserialisedResponse.profile;
        }
    }

    [Serializable]
    public class DeviceProfileResponse
    {
        public DeviceProfile profile;
    }

    [Serializable]
    public class ProfileDefinition
    {
        public ApplicationData[] applications;
    }

    public class DeviceProfileFactory
    {
        public static DeviceProfile Create()
        {
            return DeviceProfile.DownloadDeviceProfile(API.defaultProfileURL);
        }

        public static DeviceProfile Create(string _customURL)
        {
            return DeviceProfile.DownloadDeviceProfile(_customURL);
        }
    }
}