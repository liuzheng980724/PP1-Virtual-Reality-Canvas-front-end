using System;
using Snobal.Utilities;
using static Snobal.Extras.Resources;

namespace Snobal.Library
{
    /// <summary>
    /// A Tenant object is an abstract interface that represents the specific Snobal Cloud backend instance that this device is paired to.
    /// </summary>
    public class Tenant
    {
        /// <summary>
        /// Represents data stored in the tenant configuration on the backend that the device can retrieve
        /// </summary>
        [Serializable]
        public struct CompanyMetadata
        {
            /// <summary>
            /// Company's website
            /// </summary>
            public string companyUrl;

            /// <summary>
            /// Name of tenant
            /// </summary>
            public string name;

            /// <summary>
            /// Currently unused by this SDK
            /// </summary>
            public object details;

            /// <summary>
            /// Tenant flags can be used for anything. Disabling rooms, disabling objects. How they are used is up to the app.
            /// </summary>
            public string[] tenantFlags;

            /// <summary>
            /// Download this url to retrieve an image that can be used for branding.
            /// We use this for branding different tenants with the same app.
            /// You may want to hard code your branding though.
            /// </summary>
            public Uri logoUrl;

            /// <summary>
            /// Define the company colors to tint materials with.
            /// </summary>
            public ColourMetaData colors;
        }

        [Serializable]
        public struct ColourMetaData
        {
            public string interior;     // string with 3 floats, R, G, B in the range 0-255 ie "[7,113,166]"
        }


        [Serializable]
        public struct ApplicationMetaData
        {
            public string softwareFolder;
        }

        public const string TenantLogoFileExtension = "png";
        private string tenantURL;
        public CompanyMetadata Details;

        public Tenant(string tenantURL)
        {
            this.tenantURL = tenantURL;
        }


        /// <summary> Returns the base URL for downloading this client's asset content </summary>
        /// <param name="_clientId"> Client's ID </param>
        /// <returns> Client specific URL </returns>
        public string GetTenantURL()
        {
            return tenantURL;
        }
        public string BuildTenantAPIURL(string _API)
        {
            return GetTenantURL() + _API;
        }

        public ResourceData GetTenantLogoResourceData()
        {
            return new ResourceData
            {
                type = TenantLogoFileExtension,
                id = GetTenantLogoFileName(false),
                url = Details.logoUrl.OriginalString,
                uri = Details.logoUrl
            };
        }


        /// <summary> Generates & returns the filename to use for this tenant's logo </summary>
        public static string GetTenantLogoFileName(bool _withExtension)
        {
            string filename = "snobal_tenant_branding_logo";
            if (_withExtension)
                filename += "." + TenantLogoFileExtension;
            return filename;
        }

        public void GetTenantMetaData(IRestClient restClient)
        {
            var url = BuildTenantAPIURL(API.metaDataExtension);
            var responseString = restClient.Get(url).Response;

            try
            {
                Details = Serialization.DeserializeToType<CompanyMetadata>(responseString);
            }
            catch
            {
                Logger.Log("GetTenantMetaData Deserialize fail. Reponse "+responseString, Logger.LogLevels.Error);
            }
        }

        public ApplicationMetaData GetAppMetaData(IRestClient restClient)
        {
            var responseString = restClient.Get(BuildTenantAPIURL(API.binaryDownloadExtension)).Response;
            return Serialization.DeserializeToType<ApplicationMetaData>(responseString);
        }
    }
}
