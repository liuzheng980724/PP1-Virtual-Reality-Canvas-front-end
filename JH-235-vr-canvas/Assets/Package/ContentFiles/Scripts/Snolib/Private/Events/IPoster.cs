using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snobal.Library
{
    /**
     * Event posting interface
    */
    internal interface IPoster<T>
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
		 * SetEnabled
		 *
		 * @param enabled Flag to set enabled or not
		*/
		bool Enabled { get; set; }

		/**
		 * Post an individual item
		 *
		 * @param item A single item to post
		 * @returns Returns if post was successful
		 * @throws Throws SnolibException on error
		*/
		bool Post(T item);

		/**
		 * Post an item list
		 *
		 * @param events An ItemList of items
		 * @returns Returns if post was successful
		 * @throws Throws SnolibException on error
		*/
		bool Post(List<T> items);
    }
}
