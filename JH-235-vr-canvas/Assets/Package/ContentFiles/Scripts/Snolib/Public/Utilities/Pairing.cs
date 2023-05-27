using System;
using System.Threading.Tasks;
using Snobal.Library;

namespace Snobal.Utilities
{
    /// <summary>
    /// Pairing is an important step that allows a device to connect to a specific instance of the Snobal Cloud backend that only contains data and resources associated with a specific client.
    /// We refer to these as Tenants.
    /// </summary>
    public static class Pairing
    {
        public const string invalidTokenResponse = "Not Found";
        public const int codeLength = 8;

        /// <summary>
        /// Use this call to connect to a tenant that has distributed a pairing code.
        /// </summary>
        /// <param name="deviceToken">An 8 digit code that is used to pair a device to a specific back end.
        /// Device tokens are one time use and unless configured otherwise, expire after 24 hours.</param>
        /// <param name="restClient">The restclient used to process the pairing.</param>
        /// <returns>Returns a PairingResponse object if pairing is successful, otherwise will return null.</returns>
        public static PairingResponse Pair(string deviceToken, IRestClient restClient)
        {
            var pairingRequest = new PairingRequest { pairingToken = deviceToken, processorId = "" };
            var jsonBody = Serialization.SerializeObject(pairingRequest);
            var response = restClient.Post(Library.API.pairingUrl, jsonBody);

            if (!response.Success)
            {
                if (response.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Logger.Log("Invalid pairing code.");
                    return null;
                }
                else
                    throw new Library.SnobalException(string.Format("Couldn't contact pairing server, status code: {0}.\nAdditional response context: {1}", response.HttpStatusCode, response.Response), "There was an error contacting the server. Please retry shortly.");
            }

            return Serialization.DeserializeToType<PairingResponse>(response.Response);
        }

        [Serializable]
        class PairingRequest
        {
            public string pairingToken;
            public string processorId;
        }

        [Serializable]
        public class PairingResponse
        {
            public string tenantUrl = null;
            public string deviceToken = null;
        }
    }
}
