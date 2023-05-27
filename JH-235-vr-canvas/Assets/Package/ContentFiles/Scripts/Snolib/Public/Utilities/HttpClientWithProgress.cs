using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Snobal.Utilities
{
    /// <summary>
    /// HttpClient which uses HttpResponseMessage, with added progress callback.
    /// Source:
    /// https://stackoverflow.com/a/43169927/11892691
    /// which in turn credits:
    /// https://stackoverflow.com/questions/21169573/how-to-implement-progress-reporting-for-portable-httpclient/21644313#21644313
    /// https://github.com/dotnet/corefx/issues/6849#issuecomment-195980023
    /// </summary>
    public class HttpClientWithProgress
    {
        private readonly Uri _downloadUrl;
        private readonly string _destinationFilePath;

        private HttpClient _httpClient;

        public delegate void ProgressChangedHandler(long totalBytesDownloaded, double? progressPercentage, long progressBytesDownloaded);

        public event ProgressChangedHandler ProgressChanged;

        public HttpClientWithProgress(HttpClient httpClient, Uri downloadUrl, string destinationFilePath)
        {
            _downloadUrl = downloadUrl;
            _destinationFilePath = destinationFilePath;
            _httpClient = httpClient;
        }

        public async Task StartDownload(Action<HttpResponseMessage> onCompleteCallback)
        {
            using (var response = await _httpClient.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                await DownloadFileFromHttpResponseMessage(response);
                onCompleteCallback(response);
            }
        }
        public async Task StartDownload()
        {
            using (var response = await _httpClient.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                await DownloadFileFromHttpResponseMessage(response);
            }
        }

        private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;

            using (var contentStream = await response.Content.ReadAsStreamAsync())
                await Library.FileIO.DownloadStreamToFile(totalBytes, contentStream, _destinationFilePath, TriggerProgressChanged);
        }

        private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead, long bytesRead = 0)
        {
            if (ProgressChanged == null)
                return;

            double? progressPercentage = null;
            if (totalDownloadSize.HasValue)
                progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

            ProgressChanged(totalBytesRead, progressPercentage, bytesRead);
        }
    }
}
