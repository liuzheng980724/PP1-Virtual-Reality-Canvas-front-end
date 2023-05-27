using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Snobal.Library
{
	/**
	 * Event posting class to first cache and batch send over internet periodically
	*/
	internal class CachedPoster<T> : IPoster<T>
	{
		// DATA

		private ICacher<T> cacher;
		private IPoster<T> poster;

		private int postFrequency;
		private int postBatchSize;

		private int endPostThread;			// 0 false, 1 true
		private int postThreadAlive;		// 0 false, 1 true

		private bool enabled;

		private Thread thread;

		private ISerialiser<T> serialiser;

		private BlockingCollection<T> postQueue;

		private bool isInitialised;

		// PUBLIC

		/**
		 * Constructor
		 *
		 * @param pCacher The type of cache to use (will delete)
		 * @param pPoster The poster to dispatch items from the cache (will delete)
		 * @param postFrequency The time in milliseconds betwen batch posts
		 * @param postBatchSize The maximum size of a batch to post
		 */
		public CachedPoster(ICacher<T> cacher, IPoster<T> poster, int postFrequency, int postBatchSize, ISerialiser<T> serialiser)
		{
			this.cacher = cacher;
			this.poster = poster;
			this.postFrequency = postFrequency;
			this.postBatchSize = postBatchSize;
			this.serialiser = serialiser;
		}

		/**
		 * Finaliser
		 */
		~CachedPoster()
        {
			Shutdown();
        }

		/**
		 * Init
		 *
		 * @throws Throws SnolibException on error
		*/
		public void Init()
		{
			ErrorHandling.Test(!isInitialised, "CachedPoster already initialised");

			postQueue = new BlockingCollection<T>();

			if (cacher != null)
			{
				cacher.Init();
			}

			if (poster != null)
			{
				poster.Init();
			}

			thread = new Thread(PostFromCacheLoop);
			thread.IsBackground = true;
			thread.Start();

			isInitialised = true;
		}

		/**
		 * Shutdown
		 *
		 * @throws Throws SnolibException on error
		*/
		public void Shutdown()
		{
			Console.WriteLine("Shutdown");

			if (isInitialised)
			{
				// Kill the thread
				Interlocked.Exchange(ref endPostThread, 1);

				// Post all remaining items. Don't wait for the thread todo this because in Unity the thread doesn't receive anymore updates.
				PostAllCacheItems();

				if (poster != null)
				{
					poster.Shutdown();
				}

				if (cacher != null)
				{
					cacher.Shutdown();
				}

				try
				{
					postQueue.Dispose();
				}
				catch (Exception e)
				{
					ErrorHandling.Fail(e.Message);
				}

				isInitialised = false;
			}
		}

		/**
		 * SetEnabled
		 *
		 * @param enabled Flag to set enabled or not
		*/
		public bool Enabled 
		{ 
			get 
			{ 
				return enabled; 
			}
			set
            {
				ErrorHandling.Test(isInitialised, "CachedPoster not initialised");

				enabled = value;

				if (poster != null)
                {
					poster.Enabled = enabled;
				}
			}
		}

        /**
		 * Post an individual item
		 *
		 * @param item A single item to post
		 * @throws Throws SnolibException on error
		*/
        public bool Post(T item)
		{
			ErrorHandling.Test(isInitialised, "CachedPoster not initialised");

			return Post(new List<T>() { item });
		}

        /**
		 * Post an item list
		 *
		 * @param items An ItemList of item data
		 * @throws Throws SnolibException on error
		*/
        public bool Post(List<T> items)
		{
			ErrorHandling.Test(isInitialised, "CachedPoster not initialised");

			foreach (var i in items)
            {
				postQueue.Add(i);
			}

			return true;
		}

		// PRIVATE


		/// <summary>
		/// Post all remaining items. NOTE: Could cause stalls. Only use when performance isn't an issue. ie Shutdown, during Scene load, etc.
		/// </summary>
		private void PostAllCacheItems()
		{
			// Do the work
			if (cacher != null && poster != null && poster.Enabled)
			{
				bool success = true;
				while(cacher.HasItems() && success)
				{
					success = PostNextBatchedItems();
				}
			}
		}

		private bool PostNextBatchedItems()
		{
			// Do the work
			if (cacher != null && poster != null && poster.Enabled)
			{
				// If there are events in the cache
				if (cacher.HasItems())
				{
					// Retreive up to postBatchSize events from the cache
					List<T> itemList = null;
					try
                    {
						itemList = cacher.GetItems(postBatchSize);
					}
					catch (Exception e)
                    {
						Logger.Log(e.Message, Logger.LogLevels.Error);
					}

					if (itemList != null)
					{
						// Attempt to post the events
						bool postSuccess = false;
						try
						{
							postSuccess = poster.Post(itemList);
						}
						catch (SnobalException e)
						{
							StringBuilder sb = new StringBuilder();
							sb.Append("Failed to post batch, reason: ");
							sb.Append(e.Message);
							sb.Append(Environment.NewLine);
							sb.Append(serialiser.SerialiseList(itemList));

							Logger.Log(sb.ToString(), Logger.LogLevels.Error);
						}

						// Post was confirmed, so it is now safe to remove events from the cache
						// If failed to post, then loop and try again
						if (postSuccess)
						{
							cacher.ClearItems(itemList.Count);
							return true;
						}
						return false;
					}
				}
			}
			return false;
		}

		private void PostFromCacheLoop()
		{
			Interlocked.Exchange(ref postThreadAlive, 1);
			
			const int checkFreq = 100;
			int numChecks = postFrequency / checkFreq;

			while (endPostThread == 0)
			{
				PostNextBatchedItems();

				// Sleep at the end of the loop so that the exit flag may be checked immediately on awake
				// Make a frequent check to see if the termination request has been made
				for (int i = 0; i < numChecks; ++i)
				{
					if (postQueue.Count > 0)
                    {
						int count = postQueue.Count;
						List<T> items = new List<T>(count);
						for (int q = 0; q < count; ++q)
						{
							try
                            {
								items.Add(postQueue.Take());
							}
							catch (Exception e)
                            {
								ErrorHandling.Fail(e.Message);
                            }
						}

						if (cacher != null)
						{
							cacher.AddItems(items);
						}
					}

					Thread.Sleep(checkFreq);
					
					if (endPostThread == 1)
					{
						break;
					}
				}
			}

			Interlocked.Exchange(ref postThreadAlive, 0);
		}
	}
}
