using UnityEngine;
using System.Net;
using System;

// We require a local HTTP server to decrypt audio, image & video downloaded media files. Live m3u8 (HLS) files dont use this.
//
// You can only download a ogg file & convert into a AudioClip via Unity web request, Unity doesn't expose the functions to Decode ogg files or convert into AudioClips.
// This means we dont have any place to decryt the file before this happens.
// Similar issue with videos, you can only provide a URL and can't decrypt the file before usage.
// So by creating a local Http listener i can provide Unity with a local server for it's Unity web requests & URL requests, & it also provides me a place to decryt the files before it trys to use them.

namespace Snobal.CloudSDK
{
	public class SnobalCloudHTTPFileServer
	{
		static private HttpListener httpFileListener = null;
		static public readonly string localHTTPurl = "http://127.0.0.1:8080/";

		static public void Create()
		{
			// Create local HTTP listener so we can decrypt files before use.
			if (httpFileListener == null)
			{
				httpFileListener = new HttpListener();
				httpFileListener.Prefixes.Add(localHTTPurl);
				httpFileListener.Start();
				httpFileListener.IgnoreWriteExceptions = true;
				httpFileListener.BeginGetContext(new AsyncCallback(httpFileListenerCallback), httpFileListener);
			}
		}

		/// <summary>
		/// Call to get a audio or video file. We do things this way so we can use Snobal.Library.FileIO.ReadAllBytes which will decryt if nesscary.
		/// </summary>
		/// <param name="result"></param>
		static private void httpFileListenerCallback(IAsyncResult result)
		{
			HttpListener listener = (HttpListener)result.AsyncState;
			HttpListenerContext context = listener.EndGetContext(result);
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;

			// Get a file name & path from the URL. Remove any leading file prefix for use in C# file functions
			string filePath = context.Request.RawUrl.TrimStart('/');    // remove any leading /
			filePath = filePath.Replace("file:///", "");

			byte[] buffer = null;

			Library.Logger.Log("SnobalCloudHTTPFileServer, loading - " + filePath, Library.Logger.LogLevels.Debug);

			// Get file data, whole file
			try
			{
				buffer = Library.FileIO.ReadAllBytes(filePath);  // This will decrypt if required
				Library.Logger.Log("SnobalCloudHTTPFileServer, loaded - " + filePath + ", size " + buffer.Length, Library.Logger.LogLevels.Debug);
			}
			catch (Exception e)
			{
				Library.Logger.Log(e);
			}

			// Setup for our next request
			httpFileListener.BeginGetContext(new AsyncCallback(httpFileListenerCallback), httpFileListener);

			response.ContentLength64 = buffer.Length;

			// This can sometimes sliently fail on the Pico (works fine on the PC). Everything seems to work ie the output stream is fine & works, but code under this won't be executed.
			// No idea why. No exceptions, no error message etc.
			response.OutputStream.Write(buffer, 0, buffer.Length);
			response.OutputStream.Close();

			//listener.Close();
			//listener.Stop();
		}


		static public void Stop()
		{
			if (httpFileListener != null)
			{
				httpFileListener.Stop();
				httpFileListener = null;
			}
		}
	}
}
