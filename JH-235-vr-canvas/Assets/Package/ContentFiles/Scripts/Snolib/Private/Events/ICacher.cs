using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snobal.Library
{
	/**
	 * Caching interface
	*/
	internal interface ICacher<T>
	{
		/**
		 * Init
		 *
		 * @throws Throws SnolibException on error
		*/
		void Init();

		/**
		 * Shutdown
		 *
		 * @throws Throws SnolibException on error
		*/
		void Shutdown();

		/**
		 * HasItems
		 *
		 * @return Return if the cache has items or not
		*/
		bool HasItems();

		/**
		 * AddItems
		 *
		 * Add items to the cache
		 *
		 * @param itemList The list of items to add to the cache
		*/
		void AddItems(List<T> itemList);

		/**
		 * GetItems
		 *
		 * Retrieve items from the cache
		 *
		 * @param maxItems The maximum number of items to return
		 * @return The list populated with the retrieved items
		*/
		List<T> GetItems(int maxItems);

		/**
		* ClearItems
		*
		* Clear the given number of items from the cache
		*
		* @param count The number of items to clear from the cache
		*/
		void ClearItems(int count);

	};
}
