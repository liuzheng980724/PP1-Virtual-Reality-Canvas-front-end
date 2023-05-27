using System;
using Snobal.Utilities;

namespace Snobal.Library.Scopes
{
    public class Login : ScopeTemplate
    {
        /// <summary>
        /// Currently unused, Snobal Library does not support offline mode in this iteration.
        /// </summary>
        public bool IsOfflineMode { get; private set; } = false;

        /// <summary>
        /// Cached version of response returned from the backend when Open is called.
        /// </summary>
        public Data.BackendResponses AttemptResponse { get; private set; } = Data.BackendResponses.Unattempted;

        private Data.OpenData openData;
        private Data.CloseData closeData;
        private string deviceToken;

        public Login(string DeviceToken) : base()
        {
            deviceToken = DeviceToken;
        }

        /// <summary>
        /// Open this scope.
        /// Can only be called once, if you need to attempt to open a scope again, create a new scope.
        /// </summary>
        /// <param name="openData">Expects a Scopes.Login.Data.OpenData object.
        /// The data you put in here will appear on the front end.</param>
        public override void Open(ScopeParameter openData)
        {
            this.openData = TestParameter<Data.OpenData>(openData, "Login.Open");
            TestState(ScopeStates.Unopened, State);

            if (deviceToken == null)
            {
                AttemptResponse = Data.BackendResponses.Unpaired;
                State = ScopeStates.Failed;
                return;
            }

            string loginDataString = Data.Serialize(this.openData);

            var tenantLoginUrl = BuildAPIURL(API.loginExtension);

            Snobal.Utilities.Networking.Header[] headers = { new Snobal.Utilities.Networking.Header { key = "Authorization", value = "SnobalDevice " + deviceToken } };
            Snobal.Utilities.Networking.WebResponse response = null;
            try
            {
                // Snobal.Library.Networking remembers returned authentication cookies for us
                response =  restClient.Post(tenantLoginUrl, loginDataString, headers);
            }
            catch (SnobalHttpException)
            {
                Logger.Log("Login error " + response.HttpStatusCode + ": Snobal Exception during login. Network connection error during login, defaulting to offline. " + tenantLoginUrl, Logger.LogLevels.Error);
                IsOfflineMode = true;
                AttemptResponse = Data.BackendResponses.Offline;
                State = ScopeStates.Failed;
                return;
            }
            catch
            {
                Logger.Log("Login error " + response.HttpStatusCode + ": Exception during login. Network connection error during login. " + tenantLoginUrl, Logger.LogLevels.Error);
            }

            if (response.Success)
            {
                Logger.Log("Successful login, " + tenantUrl + ", " + deviceToken, Logger.LogLevels.Event);
                Id = Serialization.DeserializeToDynamic(response.Response).sessionId;
                AttemptResponse = Data.BackendResponses.LoggedIn;
                State = ScopeStates.Opened;
                IsOfflineMode = false;
                //sc.metaData = sc.Tenant.GetTenantMetaDataAsync(sc);
                //sc.Settings.TenantId = sc.metaData.name;
                //sc.Settings.Save();

                // Set the devices encrytion password
                Security_0_0.Encryption.Password = Serialization.DeserializeToDynamic(response.Response).deviceEncryptionPassword;
                //Logger.Log("Encryption password is " + Security_0_0.Encryption.Password);
                return;
            }
            else
            {
                Logger.Log("Unsuccessful login, " + tenantUrl + ", " + deviceToken, Logger.LogLevels.Event);
                Logger.Log("Unsuccessful login, " + tenantUrl + ", " + deviceToken, Logger.LogLevels.Error);

                State = ScopeStates.Failed;

                switch (response.HttpStatusCode)
                {
                    case (System.Net.HttpStatusCode.Forbidden):
                        Logger.Log("Login error response = " + Serialization.SerializeObject(response), Logger.LogLevels.Error);

                        var token = Serialization.DeserializeToDynamic(response.Response);
                        if (token.disabled != null && token.disabled == true)
                        {
                            AttemptResponse = Data.BackendResponses.Disabled;
                            Logger.Log("Login error " + response.HttpStatusCode + ": Snobal disabled device while trying to contact " + tenantLoginUrl, Logger.LogLevels.Error);
                            return;
                        }

                        AttemptResponse = Data.BackendResponses.Unpaired;
                        Logger.Log("Login error " + response.HttpStatusCode + ": Snobal unpaired device while trying to contact " + tenantLoginUrl, Logger.LogLevels.Error);
                        return;

                    default:
                        Logger.Log("Login attempt returned an unhandled status code: " + response.HttpStatusCode + " while trying to contact " + tenantLoginUrl + ", and could not continue", Logger.LogLevels.Error);
                        return;
                }

            }

        }

        /// <summary>
        /// Call this to close the scope.
        /// </summary>
        /// <exception cref="SnobalException">Will throw if the method is called but the scope is not open.</exception>
        /// /// <exception cref="SnobalBadRequestException">A bad request is thrown when the library is attempting behaviour that is inconsistent with the backend, such as trying to close a scope that is already considered closed.</exception>
        /// <param name="closeData">Expects a Scopes.Login.Data.CloseData object.</param>
        public override void Close(ScopeParameter closeData)
        {
            this.closeData = TestParameter<Data.CloseData>(closeData, "Login.Close");

            if (Logger.isInitialised)
                Logger.Log("Logout reason = " + this.closeData.details.reason.ToString());

            string body = Serialization.SerializeObject(this.closeData);
            string url = BuildAPIURL(API.logoutExtension);

            var response = restClient.Post(url, body);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                dynamic responseBody = Serialization.DeserializeToDynamic(response.Response);
                throw new SnobalBadRequestException("Bad request '" + responseBody.error + "' from '" + url + "' API call\nBody = '" + body + "'", "An unexpected network error occurred");
            }
            if (!response.Success)
                throw new SnobalException("Unexpected response '" + response.HttpStatusCode + "' from '" + url + "' API call\nBody = '" + body + "'", "An unexpected network error occurred");

            Logger.Log("Successful logout, " + tenantUrl + ", " + deviceToken, Logger.LogLevels.Event);

            State = ScopeStates.Closed;
        }

        [Serializable]
        public static class Data
        {
            public enum BackendResponses { Unattempted, LoggedIn, Unpaired, Disabled, Offline, Failed, LoggedOut }
            public enum LogoutReasons { Unknown, LauncherAutoRestart, LaunchingXR, UserQuit, UnexpectedQuit, FatalError }
            public struct DeviceDetails
            {
                public float batteryLevel;
                public string deviceModel;
                public string deviceName;
                public string deviceUniqueIdentifier;
                public string graphicsDeviceName;
                public string graphicsDeviceVendor;
                public int graphicsMemorySize;
                public string operatingSystem;
                public string processorType;
                public int systemMemorySize;
            }

            public struct Details
            {
                public DeviceDetails device;
            }

            [Serializable]
            public class OpenData : ScopeParameter
            {
                public Details details;
                [NonSerialized]
                public string DeviceToken;
            }

            [Serializable]
            public class CloseData : ScopeParameter
            {
                public CloseData(LogoutReasons reason)
                {
                    details = new Details { reason = reason.ToString() };
                }

                public struct Details
                {
                    public string reason;
                }

                public Details details;
            }

            public static string Serialize(OpenData data)
            {
                if (data == null)
                {
                    throw new ArgumentNullException("LoginData.Serialize trying to serialize a null LoginData");
                }

                return Serialization.SerializeObject(data);
            }
        }
    }
}
