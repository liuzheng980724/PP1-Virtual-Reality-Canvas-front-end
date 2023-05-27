using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snobal.Library
{
	/**
	 * Posting class to send events to a log
	*/
	internal class LogPoster<T> : IPoster<T>
	{
		// DATA

		private uint logs;

		private ISerialiser<T> serialiser;

		// PUBLIC

		/**
		 * Constructor
		 *
		 * @param postUrl URL to post events to
		*/
		public LogPoster(uint logs, ISerialiser<T> serialiser)
		{
			this.logs = logs;
			this.serialiser = serialiser;
		}

		/**
		* Finaliser
		*/
		~LogPoster()
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

				Logger.Log(serialisedItems, Logger.LogLevels.Info, logs);

				return true;
			}

			return false;
        }

		#endregion
	}
}
