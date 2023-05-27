using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snobal.Library
{
	/**
	 * Posting class to send events over the internet
	*/
	internal class HttpPoster<T> : IPoster<T>
	{
		// DATA

		private string postUrl;

		private ISerialiser<T> serialiser;

        private IRestClient restClient;

		// PUBLIC

		/**
		 * Constructor
		 *
		 * @param postUrl URL to post events to
		*/
		public HttpPoster(string postUrl, ISerialiser<T> serialiser, IRestClient restClient)
		{
			this.postUrl = postUrl;
			this.serialiser = serialiser;
            this.restClient = restClient;
		}

		/**
		* Finaliser
		*/
		~HttpPoster()
		{
			Shutdown();
		}


		#region IPoster

		/**
		 * Init
		 *
		 * @throws Throws SnolibException on error
		*/
		public void Init()
		{

		}

		/**
 		 * Shutdown
		 *
		 * @throws Throws SnolibException on error
		*/
		public void Shutdown()
		{

		}

		/**
		 * SetEnabled
		 *
		 * @param enabled Flag to set enabled or not
		*/
		public bool Enabled { get; set; }

		/**
		 * Post an individual item
		 *
		 * @param item A single item to post
		 * @returns Returns if post was successful
		 * @throws Throws SnolibException on error
		*/
		public bool Post(T item)
        {
            if (Enabled)
            {
                return Post(new List<T>() { item });
            }
            return false;
        }

		/**
		 * Post an item list
		 *
		 * @param events An ItemList of items
		 * @returns Returns if post was successful
		 * @throws Throws SnolibException on error
		*/
		public bool Post(List<T> items)
        {
            if (Enabled)
            {
				string serialisedItems = null;

				try
				{
					serialisedItems = serialiser.SerialiseList(items);
				}
				catch (Exception e)
                {
					ErrorHandling.Fail("JSON serialisation exception during Post attempt: " + e.Message);
				}

				Utilities.Networking.WebResponse response = null;
				try
				{
					response = restClient.Post(postUrl, serialisedItems);
				}
				catch (Exception e)
				{
					ErrorHandling.Fail("Http request exception during Post attempt: " + e.Message);
				}

				if (response.Success)
                {
					return true;
                }
				else
                {
					StringBuilder sb = new StringBuilder();
					sb.Append("Http Post attempt returned an unhandled status code:");
					sb.Append(response.HttpStatusCode.ToString());
					sb.Append(", with response: ");
					sb.Append(response.Response);

					Logger.Log(sb.ToString(), Logger.LogLevels.Warning);

					return false;
				}
			}

			return false;
        }

		#endregion
	}
}
