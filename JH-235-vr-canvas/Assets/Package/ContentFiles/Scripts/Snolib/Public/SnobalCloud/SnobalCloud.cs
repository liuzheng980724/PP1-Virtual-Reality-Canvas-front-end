using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Snobal.Library;
using Snobal.Library.Scopes;
using Snobal.Library.DataStructures;
using Snobal.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Logger = Snobal.Library.Logger;

namespace Snobal
{
    /// <summary>
    /// This class behaves as an abstract interface for using features of the Snobal Cloud backend.
    /// It implements some other features of the SDK, many of which have an internal access modifier
    /// If you wish to implement features of the Snobal Cloud backend yourself, please contact us directly and we'll discuss it
    /// </summary>
    public class SnobalCloud
    {
        //public enum CloudStates { Unpaired, Paired, LoggedIn, Ready, Controlled }
        //public bool IsOfflineMode { get; private set; } = false;
        public bool HasLoginBeenGranted { get { if (LoginScope == null) return false; return LoginScope.IsOpen; } }

        /// <summary>
        /// Should we encrypt any downloaded files from snobal cloud when saved to the device?
        /// </summary>
        public bool EncryptDownloadedContent
        {
            get { return FileIO.encryptDownloadedContent; }
            set { FileIO.encryptDownloadedContent = value; }
        }

        /// <summary>
        /// Used to store configuration information to connect to the paired tenant.
        /// </summary>
        public Settings<SnobalCloudSettings> Settings { get; private set; }

        /// <summary>
        /// This object will be created automatically when loading settings if it is paired
        /// It is also created when pairing an instance of the SnobalCloud
        /// If this object is null, SnobalCloud is not paired
        /// </summary>
        public Tenant Tenant { get; private set; }

        /// <summary>
        /// This object will be created automatically after a ValidatePasscode call
        /// </summary>
        public Participant Participant { get; set; }

        /// <summary>
        /// This object will be created automatically after a GetAvailablePresentations call
        /// </summary>
        public PresentationData[] PresentationData { get; private set; }

        /// <summary>
        /// Represents an instance of a login. See the LoginScope class for more info or consult our document describing scopes.
        /// </summary>
        public Library.Scopes.Login LoginScope { get; private set; }

        /// <summary>
        /// The interface used for making various REST calls. The default is AuthenticatedRestClient
        /// You can implement your own and plug it into the SnobalCloud but currently it is not recommended.
        /// Contact us if you want to discuss this further and what caveats to be aware of.
        /// Some methods expect an AuthenticatedRestClient specifically.
        /// </summary>
        public IRestClient RestClient { get; private set; }

        /// <summary>
        /// Created automatically when a Tenant object is created
        /// It is used to imprint the correct URLs on a scope for various API calls
        /// </summary>
        public ScopeFactory ScopeFactory { get; private set; }
        public bool IsPaired { get { return Tenant != null; } }

        /// <summary>
        /// This object will be created automatically after a EndScheme call
        /// </summary>
        public EndSchemeData RecievedEndSchemeData { get; private set; }

        /// <summary>
        /// This object will be created automatically after a EndScheme call
        /// </summary>
        public SchemeData RecievedSchemeData { get; private set; }

        /// <summary>
        /// SnobalCloud constructor will instantiate itself an AuthenticatedRestclient, see thy AuthenticatedRestclient object or IRestClient interface for more information
        /// It will then attempt to load a SnobalCloudSettings file, creating the file if it does not already exist. As a prerequisite the FileIO static class must be initialised.
        /// Finally, if the device is paired it will create a Tenant object
        /// </summary>
        public SnobalCloud()
        {
            Logger.Log("SnobalCloud starting.");

            ErrorHandling.Test(FileIO.isInitialised, "You must initialise FileIO before creating an instance of the SnobalCloud class.");

            ConfigureNetworking(new AuthenticatedRestclient());

            Settings = Settings<SnobalCloudSettings>.SettingsFactory();

            ConfigureTenant(Settings.Values.TenantURL);
        }

        /// <summary>
        /// This will be called automatically when the SnobalCloud class is created
        /// Unless you have a reason to do so, you probably don't need to call this
        /// </summary>
        /// <param name="restClient">Used for a custom implementation. Leave empty for default implementation (recommended)</param>
        public void ConfigureNetworking(IRestClient restClient)
        {
            Logger.Log("Configuring networking.");

            if (restClient == null)
                throw new ArgumentNullException("Can't configure networking with a null rest client.");

            if (RestClient != null)
                RestClient.Shutdown();

            RestClient = restClient;
            RestClient.Initialize();

            if (ScopeFactory != null)
                ScopeFactory.SetRestclient(RestClient);
        }

        /// <summary>
        /// This will be called automatically when the SnobalCloud class is created and then again when the Pair method is called, should that pairing succeed.
        /// Unless you have a reason to do so, you probably don't need to call this. 
        /// </summary>
        /// <param name="tenantURL">Used for a custom implementation. Leave empty for default implementation (recommended)</param>
        public void ConfigureTenant(string tenantURL)
        {
            Logger.Log("Configuring Tenant.");

            if (string.IsNullOrEmpty(tenantURL))
            {
                Logger.Log("No tenant, Snobal Cloud is not paired.");
                return;
            }

            Tenant = new Tenant(tenantURL);

            Logger.Log("Snobal Cloud is paired to " + tenantURL);

            if (Tenant != null)
            {
                ScopeFactory = new ScopeFactory(Tenant.GetTenantURL(), RestClient);
            }
        }

        /// <summary>
        /// This method will block the calling thread.
        /// If you would rather the task run on a thread, initialise an instance of the TaskWatcher class by calling SnobalCloud.InitialiseUpdater and call the action returned by GetUpdateAction in the update step of your main thread loop
        /// Read the documentation for TaskWatcher for more information
        /// </summary>
        /// <param name="code">This is a pairing code, you can get these from the Snobal Cloud web front end by creating a new device</param>
        /// <returns>A boolean indicating success</returns>
        public bool Pair(string code)
        {
            Logger.Log("Attempting to pair with code " + code + ".");

            var pairingData = Pairing.Pair(code, RestClient);
            if (pairingData == null)
            {
                Logger.Log("Pairing failed.");
                return false;
            }

            Settings.Values.DeviceToken = pairingData.deviceToken;
            Settings.Values.TenantURL = pairingData.tenantUrl;
            Settings.Values.Registered = true;
            Settings.Save();

            ConfigureTenant(Settings.Values.TenantURL);

            return true;
        }

        /// <summary>
        /// Will generate empty LoginData for you, useful for quick implementation
        /// </summary>
        public void Login()
        {
            Login(new Login.Data.OpenData());
        }

        /// <summary>
        /// Opening a loginscope will allow full use of the Snobal Cloud REST features.
        /// Check this.LoginScope.State to determine if it is open and you are now logged in, or if it is failed and something went wrong.
        /// This method also performs the authentication step of the AuthenticatedRestClient, effectively assigning all the necessary tokens to gain access to SC API calls.
        /// </summary>
        /// <param name="_input">Will appear on the device page of your tenant front end when hovering over the little (i) icon.</param>
        public void Login(Login.Data.OpenData _input)
        {
            Logger.Log("Snobal Cloud logging in.");

            ErrorHandling.Test(IsPaired, "Tried to login but device is not yet paired");

            LoginScope = ScopeFactory.BuildLoginScope(Settings.Values.DeviceToken);
            LoginScope.Open(_input);

            if (LoginScope.IsOpen)
            {
                Tenant.GetTenantMetaData(RestClient);
                Settings.Values.TenantId = Tenant.Details.name;
                Settings.Save();

                if (RestClient is AuthenticatedRestclient)
                {
                    (RestClient as AuthenticatedRestclient).Authenticate();
                }

                // Init live event logger connection.
                Events.InitLiveConnection(this);
            }
        }

        /// <summary>
        /// If you don't call log out, your login will automatically time out after 2 hours by default and you may get strange results in your device use reporting.
        /// Aside from that though it's not a huge issue. You will still be able to log in even if you did not previously log out, however this behaviour may change in the future.
        /// </summary>
        /// <param name="reason">Useful for telemetry/reporting to determine if a remote device logged out intentionally or because of a crash.</param>
        public void Logout(Login.Data.LogoutReasons reason = Library.Scopes.Login.Data.LogoutReasons.UserQuit)
        {
            if (Logger.isInitialised)
                Logger.Log("Snobal Cloud logging out.");

            ErrorHandling.Test(LoginScope != null, "LoginScope is null");
            ErrorHandling.Test(!LoginScope.IsClosed, "LoginScope already closed");
            ErrorHandling.Test(LoginScope.IsOpen, "Not logged in");

            LoginScope.Close(new Login.Data.CloseData(reason));
        }


        /// <summary>
        /// Validates a passcode & stores the participant details
        /// </summary>
        /// <param name="_passcode"></param>
        /// <param name="_participant"></param>
        /// <returns></returns>
        public bool ValidatePasscode(string _passcode)
        {
            Logger.Log("Snobal Cloud validate passcode.");
            ErrorHandling.Test(LoginScope != null, "LoginScope is null");
            ErrorHandling.Test(LoginScope.IsOpen, "Not logged in");

            Participant TestParticipant;
            bool success = Participant.ValidatePasscode(
                RestClient,
                Tenant,
                _passcode,
                out TestParticipant);

            if (success)
            {
                Participant = TestParticipant;
            }
            return success;
        }

        /// <summary>
        /// Get a specific Scheme ID
        /// </summary>
        /// <param name="_schemeID"></param>
        public void GetScheme(string _schemeID)
        {
            var url = Tenant.BuildTenantAPIURL(API.getSchemeExtension + _schemeID);
            var response = RestClient.Get(url);

            if (response.Response == "Not Found")
            {
                Logger.Log("Scheme not found: " + response.Response, Logger.LogLevels.Error);

                var fakeData = new SchemeData
                {
                    name = "Data not found",
                    ID = _schemeID
                };
                RecievedSchemeData = fakeData;
                return;
            }
            else
            {
                RecievedSchemeData = Utilities.Serialization.DeserializeToType<SchemeData>(response.Response);
            }
        }

        /// <summary>
        /// Tell the server the user has started a scheme
        /// It supports Anonymous in case participant id is null.
        /// </summary>
        /// <param name="_schemeID"></param>
        /// <param name="appID"></param>
        public void StartScheme(string _schemeID, string appID)
        {
            ErrorHandling.Test(LoginScope != null, "LoginScope is null");
            ErrorHandling.Test(LoginScope.IsOpen, "Not logged in");

            var body = Serialization.SerializeObject(new
            { schemeCode = _schemeID, applicationId = appID, participantId = (Participant == null) ? "anonymous" : Participant.participantId });

            var url = Tenant.BuildTenantAPIURL(API.startSchemeExtension);
            var response = RestClient.Post(url, body);

            if (!response.Success)
            {
                Logger.Log("StartScheme Error: " + response.Response, Logger.LogLevels.Error);
            }

            RecievedSchemeData = Utilities.Serialization.DeserializeToType<SchemeData>(response.Response);
        }

        /// <summary>
        /// Tell the server that the user has finished a Scheme
        /// It supports Anonymous in case participant id is null.
        /// </summary>
        /// <param name="_reportId"></param>
        /// <param name="_taskCode"></param>
        /// <param name="_outcome"></param>
        public void EndScheme(string _reportId, string _taskCode, string _outcome)
        {
            ErrorHandling.Test(LoginScope != null, "LoginScope is null");
            ErrorHandling.Test(LoginScope.IsOpen, "Not logged in");
            //  ErrorHandling.Test(Participant.participantId != null, "No participantId found, validate user first");

            var body = Serialization.SerializeObject(new
            { reportId = _reportId, taskCode = _taskCode, outcome = _outcome });
            var url = Tenant.BuildTenantAPIURL(API.endSchemeExtension);
            var response = RestClient.Post(url, body);

            if (!response.Success)
            {
                Logger.Log("EndScheme Error: " + response.Response, Logger.LogLevels.Error);
            }

            RecievedEndSchemeData = Utilities.Serialization.DeserializeToType<EndSchemeData>(response.Response);
        }

        /// <summary>
        /// Tell the server the user is starting a scheme task
        /// It supports Anonymous in case participant id is null.
        /// </summary>
        /// <param name="_reportId"></param>
        /// <param name="_taskCode"></param>
        public bool StartSchemeTask(string _reportId, string _taskCode)
        {
            ErrorHandling.Test(LoginScope != null, "LoginScope is null");
            ErrorHandling.Test(LoginScope.IsOpen, "Not logged in");

            var body = Serialization.SerializeObject(new { reportId = _reportId, taskCode = _taskCode });
            var url = Tenant.BuildTenantAPIURL(API.startTaskExtension);
            var response = RestClient.Post(url, body);

            if (!response.Success)
            {
                Logger.Log("StartSchemeTask Error: " + response.Response, Logger.LogLevels.Error);
            }

            return response.Success;
        }

        /// <summary>
        /// Tell the server the user has ended a scheme task.
        ///  It supports Anonymous in case participant id is null.
        /// </summary>
        /// <param name="_reportId"></param>
        /// <param name="_taskCode"></param>
        /// <param name="_outcome"></param>
        public bool EndSchemeTask(string _reportId, string _taskCode, string _outcome)
        {
            ErrorHandling.Test(LoginScope != null, "LoginScope is null");
            ErrorHandling.Test(LoginScope.IsOpen, "Not logged in");

            var body = Serialization.SerializeObject(new { reportId = _reportId, taskCode = _taskCode, outcome = _outcome });
            var url = Tenant.BuildTenantAPIURL(API.endTaskExtension);
            var response = RestClient.Post(url, body);

            if (!response.Success)
            {
                Logger.Log("EndSchemeTask Error: " + response.Response, Logger.LogLevels.Error);
            }

            return response.Success;
        }

        /// <summary>
        /// Used for listing all the presentations
        /// </summary>
        /// <param name="_participantId"></param>
        /// <returns></returns>
        public void GetAvailablePresentations()
        {
            Logger.Log("Snobal Cloud getting available presentations.");
            ErrorHandling.Test(LoginScope != null, "LoginScope is null");
            ErrorHandling.Test(LoginScope.IsOpen, "Not logged in");

            var response = RestClient.Get(Tenant.BuildTenantAPIURL(API.listPresentationExtension));

            if (!response.Success)
            {
                Logger.Log("GetAvailablePresentations Error: " + response.Response, Logger.LogLevels.Error);
            }

            PresentationData = Utilities.Serialization.DeserializeToType<PresentationData[]>(response.Response);
        }

        // This stuff below should be moved to a Utility function somewhere imo

        /// <summary>
        /// Return an Uri from a given relative or absolute URL
        /// </summary>
        public Uri GetAbsoluteUri(string baseURL, string relativeOrAbsoluteURL)
        {
            // Is this already an absolute URL?
            if (IsAbsoluteUrl(relativeOrAbsoluteURL))
            {
                return CreateUriFromString(relativeOrAbsoluteURL);
            }

            // Check for valid baseURL
            if (!IsAbsoluteUrl(baseURL))
            {
                throw new SnobalException(string.Format("Invalid base URL address {0}", baseURL), "Something went wrong on the network, please retry soon");
            }

            // No relativeOrAbsoluteURL?
            if (String.IsNullOrEmpty(relativeOrAbsoluteURL))
            {
                return CreateUriFromString(baseURL);
            }

            // Remove any standard leading / chars from this relative path ie "/files/etc/a.png". We build the absoluteUri string without.
            relativeOrAbsoluteURL = relativeOrAbsoluteURL.TrimStart('/');

            string absoluteUri = baseURL + "/" + relativeOrAbsoluteURL;
            return CreateUriFromString(absoluteUri);
        }

        public Uri GetAbsoluteUri(string relativeUrl)
        {
            return GetAbsoluteUri(Settings.Values.TenantURL, relativeUrl);
        }
        /// <summary>
        /// Try to create a Uri from the given string. Throws an exception if url is malformed.
        /// </summary>
        public Uri CreateUriFromString(string url)
        {
            try
            {
                // Make Uri, if syntax is incorrect this will throw an exception. Ie must be a full valid address without invalid characters etc.
                return new Uri(url);
            }
            catch
            {
                throw new SnobalException(string.Format("Invalid URL address {0}", url), "Something went wrong on the network, please retry soon");
            }
        }

        /// <summary>
        /// Check if this is an absolute Url
        /// </summary>
        public bool IsAbsoluteUrl(string url)
        {
            if (!String.IsNullOrEmpty(url) && (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return false;
        }
    }
}
