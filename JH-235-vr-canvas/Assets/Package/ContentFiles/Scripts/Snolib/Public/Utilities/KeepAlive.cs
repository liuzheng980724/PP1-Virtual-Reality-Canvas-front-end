using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Snobal.Library;

namespace Snobal.Utilities
{
    public class KeepAlive
    {
        public enum SessionStatus { Default, Active, TimedOut, Error }
        public static int Frequency { get; private set; } = 60 * 1000; // Thread sleep time (in ms)

        private Action<SessionStatus> callBack;
        private bool terminationFlag = false;
        public bool Terminated { get { return terminationFlag; } }
        public SnobalException exception;

        public Task taskLoop;
        SnobalCloud snobalCloudReference;
        HttpClient httpClient;
        
        public void Initialise(Action<SessionStatus> _callback, SnobalCloud snobalCloud, HttpClient httpClient)
        {
            callBack = _callback;
            snobalCloudReference = snobalCloud;
            this.httpClient = httpClient;
        }

        public void Begin()
        {
            taskLoop = Task.Run(Loop);
        }

        void Loop()
        {
            while (terminationFlag != true)
            {
                SessionStatus currentSessionStatus;
                try
                {
                    PingBackend(out currentSessionStatus);
                }
                catch (SnobalException e)
                {
                    exception = e;
                    callBack(SessionStatus.Error);
                    return;
                }
                if (terminationFlag)
                    return;
                callBack(currentSessionStatus);
                Task.Delay(Frequency).Wait();
            }
        }

        public void Terminate()
        {
            terminationFlag = true;
        }

        private void PingBackend(out SessionStatus status)
        {
            status = SessionStatus.Default;
            if (!(snobalCloudReference.Settings.Values.DeviceToken.Length == 0))
                return;

            HttpResponseMessage response = Ping();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                status = SessionStatus.Active;

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                status = SessionStatus.TimedOut;
        }

        public static void SetFrequency(float _seconds)
        {
            Frequency = (int)(_seconds * 1000f);
        }

        private HttpResponseMessage Ping()
        {
            string keepAliveURL = snobalCloudReference.Tenant.BuildTenantAPIURL(API.keepAliveExtension);
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, keepAliveURL))
            {
                HttpResponseMessage response;
                try
                {
                    response = Task.Run(() => httpClient.SendAsync(request)).Result;
                }
                catch (HttpRequestException e)
                {
                    throw new SnobalException("HttpRequestException: '" + e.Message + "' : " + keepAliveURL, "Something went wrong on the network, please retry soon", e);
                }
                catch (InvalidOperationException e)
                {
                    throw new SnobalException("InvalidOperationException: '" + e.Message + "' : " + keepAliveURL, "Something went wrong on the network, please retry soon", e);
                }

                return response;
            }
        }
    }
}
