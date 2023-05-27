using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snobal.Library
{
    /**
    * Generic for serialisation
    */
    internal interface ISerialiser<T>
    {
		/**
		 * Serialise
		 *
		 * Convert the object into a string representation
		 *
		 * @return The return serialised string
		*/
		string Serialise(T serialiseObject);

		/**
		 * Deserialise
		 *
		 * Convert the object into a string representation
		 *
		 * @param inString The string to serialise from
		 * @param serialiseObject The object to deserialise into
		*/
		T Deserialise(string inString);

		/**
		 * Serialise
		 *
		 * Convert a list of objects into a string representation
		 *
		 * @return The return serialised string
		*/
		string SerialiseList(List<T> serialiseObject);

		/**
		 * Deserialise
		 *
		 * Convert the object into a string representation
		 *
		 * @param inString The string to serialise from
		 * @param serialiseObject The object to deserialise into
		*/
		List<T> DeserialiseList(string inString);
	}
}
