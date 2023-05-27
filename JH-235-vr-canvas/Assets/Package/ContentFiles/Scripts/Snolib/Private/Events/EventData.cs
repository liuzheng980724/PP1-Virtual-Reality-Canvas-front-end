using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Snobal.Library
{
    [Serializable]
    internal class EventData
	{
        public string uuid;
        public long timestamp;
        public string category;
        public string body;
    }

	/**
 	* Template serialiser specialisation
	*/
	internal class EventDataSerialiser : ISerialiser<EventData>
	{
		public const string USER_ERROR_MESSAGE = "An unexpected error has occurred.";

		/**
		 * Serialise
		 *
		 * Convert the object into a string representation
		 *
		 * @return The return serialised string
		*/
		public string Serialise(EventData serialiseObject)
		{
			JObject jsonObject = new JObject();
			jsonObject.Add(new JProperty("uuid", serialiseObject.uuid));
			jsonObject.Add(new JProperty("timestamp", serialiseObject.timestamp));
			jsonObject.Add(new JProperty("category", serialiseObject.category));

			try
			{
				jsonObject.Add(new JProperty("body", JsonConvert.DeserializeObject(serialiseObject.body)));
			}
			catch (Newtonsoft.Json.JsonException e)
			{
				throw new SnobalException("Json Serialize Exception: '" + e.Message + "' in object '" + serialiseObject.ToString() + "'", USER_ERROR_MESSAGE, e);
			}

			return jsonObject.ToString(Formatting.None);
		}

		/**
		 * Deserialise
		 *
		 * Convert the object into a string representation
		 *
		 * @param inString The string to serialise from
		 * @param serialiseObject The object to deserialise into
		*/
		public EventData Deserialise(string inString)
		{
			EventData data;
			try
			{
				data = JsonConvert.DeserializeObject<EventData>(inString);
			}
			catch (Newtonsoft.Json.JsonException e)
			{
				throw new SnobalException("Json Deserialize Exception: '" + e.Message + "' in string '" + inString + "'", USER_ERROR_MESSAGE, e);
			}

			return data;
		}

		/**
		 * Serialise
		 *
		 * Convert a list of objects into a string representation
		 *
		 * @return The return serialised string
		*/
		public string SerialiseList(List<EventData> serialiseObject)
		{
			JArray jsonArray = new JArray();

			foreach (var d in serialiseObject)
            {
				JObject jsonObject = new JObject();
				jsonObject.Add(new JProperty("uuid", d.uuid));
				jsonObject.Add(new JProperty("timestamp", d.timestamp));
				jsonObject.Add(new JProperty("category", d.category));

				try
				{
					jsonObject.Add(new JProperty("body", JsonConvert.DeserializeObject(d.body)));
				}
				catch (Newtonsoft.Json.JsonException e)
				{
					throw new SnobalException("Json Serialize Exception: '" + e.Message + "' in object '" + d.ToString() + "'", USER_ERROR_MESSAGE, e);
				}

				jsonArray.Add(jsonObject);
			}

			JObject rootObj = new JObject(new JProperty("events", jsonArray));

			return rootObj.ToString(Formatting.None);
		}

		/**
		 * Deserialise
		 *
		 * Convert the object into a string representation
		 *
		 * @param inString The string to serialise from
		 * @param serialiseObject The object to deserialise into
		*/
		public List<EventData> DeserialiseList(string inString)
		{
			List<EventData> serialiseObject;
			try
			{
				serialiseObject = JsonConvert.DeserializeObject<List<EventData>>(inString);
			}
			catch (Newtonsoft.Json.JsonException e)
			{
				throw new SnobalException("Json Deserialize Exception: '" + e.Message + "' in string '" + inString + "'", USER_ERROR_MESSAGE, e);
			}

			return serialiseObject;
		}
	}

	/**
	 * Specialised Sqlite command generator
	*/
	internal class EventDataSqliteCommands : ISqliteCommands<EventData>
	{

		/**
		 * CreateTable
		 *
		 * Create a table for the given template type
		 *
		 * @param tableName The table to create
		 * @return The return sql statement
		*/
		public string CreateTable(string tableName)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("CREATE TABLE IF NOT EXISTS ");
			sb.Append(tableName);
			sb.Append(" (");
			sb.Append("[ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,");
			sb.Append("[uuid] VARCHAR(36) NULL,");
			sb.Append("[timestamp] INTEGER NULL,");
			sb.Append("[category] VARCHAR(32)  NULL,");
			sb.Append("[body] VARCHAR(2048)  NULL );");
			return sb.ToString();
		}


		/**
		 * Insert
		 *
		 * Insert the given data object into the table
		 *
		 * @param tableName The table to insert into
		 * @return The return sql statement
		*/
		public string Insert(string tableName, EventData data)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("INSERT INTO ");
			sb.Append(tableName);
			sb.Append(" (uuid, timestamp, category, body) VALUES (");
			sb.Append("'");
			sb.Append(data.uuid);
			sb.Append("',");
			sb.Append(data.timestamp);
			sb.Append(",");
			sb.Append("'");
			sb.Append(data.category);
			sb.Append("',");
			sb.Append("'");
			sb.Append(data.body);
			sb.Append("');");
			return sb.ToString();
		}

		/**
		 * Count
		 *
		 * Count records in a table
		 *
		 * @param tableName The table to count from
		 * @param outString The return sql statement
		*/
		public string Count(string tableName)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT COUNT(*) FROM ");
			sb.Append(tableName);
			sb.Append(";");
			return sb.ToString();
		}

		/**
		 * Select
		 *
		 * Select records from table
		 *
		 * @param tableName The table to select from
		 * @param maxItems The maximum number of statements to cap the select at
		 * @return The return sql statement
		*/
		public string Select(string tableName, int maxItems)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT * FROM ");
			sb.Append(tableName);
			sb.Append(" WHERE ID IN (SELECT ID FROM ");
			sb.Append(tableName);
			sb.Append(" ORDER BY timestamp LIMIT ");
			sb.Append(maxItems);
			sb.Append(");");
			return sb.ToString();
		}

		/**
		 * Remove
		 *
		 * Remove a given number of records from table
		 *
		 * @param tableName The table to remove from
		 * @param itemCount The desired number of items to remove
		 * @return The return sql statement
		*/
		public string Delete(string tableName, int itemCount)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("DELETE FROM ");
			sb.Append(tableName);
			sb.Append(" WHERE ID IN (SELECT ID FROM ");
			sb.Append(tableName);
			sb.Append(" ORDER BY timestamp LIMIT ");
			sb.Append(itemCount);
			sb.Append(");");
			return sb.ToString();
		}
	}
}
