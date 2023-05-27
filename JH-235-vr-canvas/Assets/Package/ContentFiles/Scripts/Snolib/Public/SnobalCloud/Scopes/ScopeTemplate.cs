using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snobal.Utilities;

namespace Snobal.Library.Scopes
{
    public abstract class ScopeTemplate
    {
        public enum ScopeStates { 
            /// <summary>
            /// Scope has been created, but there has been no attempt to open it
            /// </summary>
            Unopened, 
            /// <summary>
            /// Scope was succesfully opened and is currently open
            /// </summary>
            Opened, 
            /// <summary>
            /// Scope was succesfully opened and then succesfully closed
            /// </summary>
            Closed, 
            /// <summary>
            /// Scope was unsuccesfully opened
            /// </summary>
            Failed }

        public ScopeStates State { get; protected set; }

        /// <summary>
        /// Shorthand for State == Opened
        /// </summary>
        public bool IsOpen { get { return State == ScopeStates.Opened; } }

        /// <summary>
        /// Shorthand for State == Closed
        /// </summary>
        public bool IsClosed { get { return State == ScopeStates.Closed; } }

        /// <summary>
        /// Shorthand for State ==  Failed
        /// </summary>
        public bool HasFailed { get { return State == ScopeStates.Failed; } }

        /// <summary>
        /// Every scope has an associated uuid that is consistent with the backend database
        /// </summary>
        public string Id { get; protected set; }

        internal string tenantUrl;
        internal IRestClient restClient;
        public ScopeTemplate()
        {
            State = ScopeStates.Unopened;
        }

        /// <summary>
        /// Open Scope
        /// </summary>
        public abstract void Open(ScopeParameter param);

        public abstract void Close(ScopeParameter param);

        public abstract class ScopeParameter { }

        /// <summary>
        /// Make sure that the parameter being given to this scope is actually of the type that you require
        /// Then return the type you expected
        /// </summary>
        /// <typeparam name="T">ScopeParameter is abstract so you should never put that in here.
        /// Instead use the derived class that your method expects.</typeparam>
        /// <param name="param">An object derived from ScopeParameter</param>
        /// <param name="callingMethod">The name of the enclosing that called this, for more verbose error messages</param>
        /// <returns>Returns param after it has been casted to T</returns>
        protected static T TestParameter<T>(object param, string callingMethod) where T : ScopeParameter
        {
            if (!(param is T))
            {
                throw new ArgumentException(string.Format("Scopes.{0} requires a {1} object", callingMethod, typeof(T).ToString()));
            }

            return param as T;
        }

        protected static void TestState(ScopeStates desiredState, ScopeStates state)
        {
            if (state != desiredState)
            {
                throw new SnobalException(string.Format("Tried calling a method that expected scope to be {0}, but it is currently {1}", desiredState.ToString(), state.ToString()));
            }
        }

        public void Send(string _url, string _body)
        {
            try
            {
                Logger.Log("Sending ready event to " + _url + " message " + _body, Library.Logger.LogLevels.Debug);
                Snobal.Utilities.Networking.WebResponse response = restClient.Post(_url, _body);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    dynamic responseBody = Serialization.DeserializeToDynamic(response.Response);
                    throw new SnobalBadRequestException("Bad request '" + responseBody.error + "' from '" + _url + "' API call\nBody = '" + _body + "'", "An unexpected network error occurred");
                }
                if (!response.Success)
                    throw new SnobalException("Unexpected response '" + response.HttpStatusCode + "' from '" + _url + "' API call\nBody = '" + _body + "'", "An unexpected network error occurred");
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                throw new SnobalHttpException("Http Request Exception from '" + _url + "' API call\nBody = '" + _body + "'", "An unexpected network error occurred", e);
            }
        }

        protected string BuildAPIURL(string APIExtension)
        {
            return tenantUrl + APIExtension;
        }

        protected static long GetCurrentUnixTime()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}
