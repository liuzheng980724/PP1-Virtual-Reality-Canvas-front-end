using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Snobal.Utilities
{
    public static class Networking
    {
        public struct Header
        {
            public string key;
            public string value;
        }

        public class WebResponse
        {
            public WebResponse(HttpStatusCode httpStatusCode, string response = "", bool failedConnection = false)
            {
                this.HttpStatusCode = httpStatusCode;
                this.Response = response;
                NoInternetConnection = failedConnection;
            }
            public bool NoInternetConnection { get; }
            public HttpStatusCode HttpStatusCode { get; }
            public string Response { get; }
            public bool Success { get { return this.HttpStatusCode == HttpStatusCode.OK; } }
        }

        public class ContentHeaderData
        {
            public System.Net.Http.Headers.EntityTagHeaderValue etag;
            public DateTimeOffset lastModifiedTime;
            public long fileSize;
            public List<string> cookies = null;
        }

        /// <summary> Returns all IP addresses (both IPV4 and IPV6) </summary>
        public static IPAddress[] GetAllIPAddresses()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList;
        }

        /// <summary> Returns all IP addresses (both IPV4 and IPV6) as strings, formatted to their standard notation </summary>
        public static string[] GetAllIPAddressStrings()
        {
            IPAddress[] ips = GetAllIPAddresses();

            string[] ipStrings = new string[ips.Length];
            for (int i = 0; i < ips.Length; ++i)
                ipStrings[i] = ips[i].ToString();

            return ipStrings;
        }

        /// <summary>
        /// Return an Uri from a given relative or absolute URL
        /// </summary>
        public static Uri GetAbsoluteUri(string baseURL, string relativeOrAbsoluteURL)
        {
            // Is this already an absolute URL?
            if (IsAbsoluteUrl(relativeOrAbsoluteURL))
            {
                return CreateUriFromString(relativeOrAbsoluteURL);
            }

            // Check for valid baseURL
            if (!IsAbsoluteUrl(baseURL))
            {
                throw new Library.SnobalException(string.Format("Invalid base URL address {0}", baseURL), "Something went wrong on the network, please retry soon");
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

        public static Uri GetAbsoluteUri(string relativeUrl, SnobalCloud snobalCloud)
        {
            return GetAbsoluteUri(snobalCloud.Settings.Values.TenantURL, relativeUrl);
        }

        /// <summary>
        /// Try to create a Uri from the given string. Throws an exception if url is malformed.
        /// </summary>
        public static Uri CreateUriFromString(string url)
        {
            try
            {
                // Make Uri, if syntax is incorrect this will throw an exception. Ie must be a full valid address without invalid characters etc.
                return new Uri(url);
            }
            catch
            {
                throw new Library.SnobalException(string.Format("Invalid URL address {0}", url), "Something went wrong on the network, please retry soon");
            }
        }

        /// <summary>
        /// Check if this is an absolute Url
        /// </summary>
        public static bool IsAbsoluteUrl(string url)
        {
            if (!String.IsNullOrEmpty(url) && (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return false;
        }
    }
}
