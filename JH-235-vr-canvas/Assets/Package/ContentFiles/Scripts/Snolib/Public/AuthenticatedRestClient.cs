using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using Snobal.Utilities;
using UnityEngine.UI;
using System.Drawing;
using System.Threading.Tasks;
using Snobal.CloudSDK;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Snobal.Library
{
    /// <summary>
    /// Provides public static methods that expose functionality of the internal http client
    /// </summary>
    public class AuthenticatedRestclient : IRestClient
    {
        public HttpClient HttpClient { get; private set; }
        private CookieContainer cookies = null;
        public bool IsAuthenticated { get; private set; }

        public void Initialize()
        {
            // This allows us to store cookies returned from the remote end automatically in the same way a browser does
            cookies = new CookieContainer();
            var handler = new WebRequestHandler
            {
                AllowAutoRedirect = true,
                AllowPipelining = true,
                CookieContainer = cookies
            };
            HttpClient = new HttpClient(handler);

            // Default timeout is 100 seconds, however Setting the Timout causes an indefinite wait on mono. See https://github.com/mono/mono/issues/12577
            // HttpClient.Timeout = TimeSpan.FromSeconds(30);

            // By default, .net limits the number of simultaneous connections to an end-point to 2
            // This change increases the maximum number of concurrent connections to a given end-point to a higher value
            // Individual tasks should be in control of setting their own concurrency limit
            ServicePointManager.DefaultConnectionLimit = 100;
        }

        public void Shutdown()
        {
            HttpClient = null;
            cookies = null;
        }

        public void Authenticate()
        {
            Logger.Log("RestClient Authenticated");
            IsAuthenticated = true;
        }

        public Snobal.Utilities.Networking.WebResponse Get(string _url)
        {
            return Get(_url, null);
        }
        public Snobal.Utilities.Networking.WebResponse Get(string _url, IEnumerable<Snobal.Utilities.Networking.Header> _headers)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _url))
            {
                if (_headers != null)
                {
                    foreach (var header in _headers)
                    {
                        request.Headers.Add(header.key, header.value);
                    }
                }
                HttpResponseMessage response;
                try
                {
                    response = HttpClient.SendAsync(request).Result;
                }
                catch (HttpRequestException e)
                {
                    throw new SnobalException("HttpRequestException: '" + e.Message + "' : " + _url, "Something went wrong on the network, please retry soon", e);
                }
                catch (InvalidOperationException e)
                {
                    throw new SnobalException("InvalidOperationException: '" + e.Message + "' : " + _url, "Something went wrong on the network, please retry soon", e);
                }

                string responseString = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    dynamic responseBlob = Serialization.DeserializeToDynamic(responseString);

                    // No session on backend
                    if (responseBlob.error.code == API.Errors.NO_SESSION_CONFIGURED ||
                        responseBlob.error.code == API.Errors.NO_SESSION_CONFIGURED_OLD)
                        throw new SnobalNoSessionException(string.Format("No session configured, `{0}`", responseString), "No session is currently configured for this device. Please contact your administrator.");

                    throw new SnobalException(string.Format("GETting {0}, gave a bad request. Unhandled error code {1}. Additional context {2}", _url, responseBlob.error.code, responseString), "Something went wrong on the network, please retry soon");
                }

                Snobal.Utilities.Networking.WebResponse webResponse = new Snobal.Utilities.Networking.WebResponse(response.StatusCode, responseString);

                return webResponse;
            }
        }

        public Snobal.Utilities.Networking.WebResponse Post(string _url, string _body)
        {
            return Post(_url, _body, null);
        }

        public Snobal.Utilities.Networking.WebResponse Post(string _url, string _body, IEnumerable<Snobal.Utilities.Networking.Header> _headers)
        {
            Snobal.Utilities.Networking.WebResponse webResponse;

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _url);
                if (_headers != null)
                {
                    foreach (var header in _headers)
                    {
                        request.Headers.Add(header.key, header.value);
                    }
                }

                var content = new StringContent(_body, System.Text.Encoding.UTF8, "application/json");

                request.Content = content;

                var response = HttpClient.SendAsync(request).Result;

                var responseString = response.Content.ReadAsStringAsync().Result;

                webResponse = new Snobal.Utilities.Networking.WebResponse(response.StatusCode, responseString);
            }
            catch (WebException e)
            {
                throw new SnobalException(e.Message + " : " + _url, "Something went wrong on the network, please retry soon", e);
            }
            catch (HttpRequestException e)
            {
                throw new SnobalHttpException("OfflineException: '" + e.Message + "' : " + _url, "Something went wrong while trying to contact Snobal Cloud, please retry soon", e);
            }
            return webResponse;
        }

        /// <summary> Performs a HTTP/HEAD request </summary>
        /// <param name="_url"> Query URL </param>
        /// <returns> The collection of key-value pairs of all headers </returns>
        public Snobal.Utilities.Networking.ContentHeaderData Head(string _url)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, _url);

            var response = HttpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new SnobalException(
                    string.Format("HTTP status {0} fetching headers for '{1}'", (int)response.StatusCode, _url),
                    "Error requesting file headers"
                );
            }

            // ErrorHandling.Test(response.Content.Headers.LastModified != null, "HEAD request's response had null Last-Modified, url was: " + _url);
            ErrorHandling.Test(response.Content.Headers.ContentLength != null, "HEAD request's response had null Content-Length, url was: " + _url);

            // TODO: Get around response.Content.Headers.LastModified = null for transcode streaming video head calls. Platform to fix.
            DateTimeOffset temp = DateTimeOffset.MinValue;
            if (response.Content.Headers.LastModified != null)
            {
                temp = (DateTimeOffset)response.Content.Headers.LastModified;
            }

            Snobal.Utilities.Networking.ContentHeaderData data = new Snobal.Utilities.Networking.ContentHeaderData
            {
                etag = response.Headers.ETag,
                lastModifiedTime = temp,
                //lastModifiedTime = (DateTimeOffset)response.Content.Headers.LastModified,
                fileSize = (long)response.Content.Headers.ContentLength
            };

            // Try and get the cookie values. These may not exist if it's a public S3 location
            IEnumerable<string> cookies = null;
            if(response.Headers.TryGetValues("Set-Cookie", out cookies))
            {
                // Add any cookies into ContentHeaderData
                foreach (var cookieValue in cookies)
                {
                    if(data.cookies == null)
                    {
                        data.cookies = new List<string>();
                    }
                    data.cookies.Add(cookieValue);
                }
            }

            return data;
        }

        public Snobal.Utilities.Networking.WebResponse Delete(string _url, IEnumerable<Snobal.Utilities.Networking.Header> _headers)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, _url))
            {
                if (_headers != null)
                {
                    foreach (var header in _headers)
                    {
                        request.Headers.Add(header.key, header.value);
                    }
                }
                HttpResponseMessage response;
                try
                {
                    response = HttpClient.SendAsync(request).Result;
                }
                catch (HttpRequestException e)
                {
                    throw new SnobalException("HttpRequestException: '" + e.Message + "' : " + _url, "Something went wrong on the network, please retry soon", e);
                }
                catch (InvalidOperationException e)
                {
                    throw new SnobalException("InvalidOperationException: '" + e.Message + "' : " + _url, "Something went wrong on the network, please retry soon", e);
                }

                string responseString = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    dynamic responseBlob = Serialization.DeserializeToDynamic(responseString);

                    // No session on backend
                    if (responseBlob.error.code == API.Errors.NO_SESSION_CONFIGURED ||
                        responseBlob.error.code == API.Errors.NO_SESSION_CONFIGURED_OLD)
                        throw new SnobalNoSessionException(string.Format("No session configured, `{0}`", responseString), "No session is currently configured for this device. Please contact your administrator.");

                    throw new SnobalException(string.Format("GETting {0}, gave a bad request. Unhandled error code {1}. Additional context {2}", _url, responseBlob.error.code, responseString), "Something went wrong on the network, please retry soon");
                }

                Snobal.Utilities.Networking.WebResponse webResponse = new Snobal.Utilities.Networking.WebResponse(response.StatusCode, responseString);

                return webResponse;
            }
        }

        public void DownloadFile(string url, string destination)
        {
            var response = HttpClient.GetAsync(url).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new SnobalException(
                    string.Format("HTTP status {0} downloading '{1}'", (int)response.StatusCode, url),
                    "Error downloading file"
                );
            }
            using (var fs = new FileStream(destination, FileMode.Create))
            {
                response.Content.CopyToAsync(fs).Wait();
            }
        }

        public Snobal.Utilities.Networking.WebResponse Put(string _url, string _body, string _path, string _contentType)
        {
            return Put(_url, _body, _path, _contentType, null);
        }

        public Snobal.Utilities.Networking.WebResponse Put(string _url, string _body, string _path, string _contentType, IEnumerable<Snobal.Utilities.Networking.Header> _headers)
        {
            Snobal.Utilities.Networking.WebResponse webResponse;

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, _url);
                if (_headers != null)
                {
                    foreach (var header in _headers)
                    {
                        request.Headers.Add(header.key, header.value);
                    }
                }

                var stream = File.OpenRead(_path);

                var content = new MultipartFormDataContent();

                var file_content = new ByteArrayContent(new StreamContent(stream).ReadAsByteArrayAsync().Result);
               
                stream.Close();

                file_content.Headers.ContentType = new MediaTypeHeaderValue(_contentType);

                content.Add(file_content);

                //Add metadata also from _body.
                // var body_content = new StringContent(_body, System.Text.Encoding.UTF8, "application/json");
                // content.Add(body_content);

                request.Content = file_content;

                var response = HttpClient.SendAsync(request).Result;
          
                var responseString = response.Content.ReadAsStringAsync().Result;

                webResponse = new Snobal.Utilities.Networking.WebResponse(response.StatusCode, responseString);
            }
            catch (WebException e)
            {
                throw new SnobalException(e.Message + " : " + _url, "Something went wrong on the network, please retry soon", e);
            }
            catch (HttpRequestException e)
            {
                throw new SnobalHttpException("OfflineException: '" + e.Message + "' : " + _url, "Something went wrong while trying to contact Snobal Cloud, please retry soon", e);
            }
            finally
            {

            }

            return webResponse;
        }
    }
}

