using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;     // Was using System.Data.SQLite, but didn't work on Android. Using Mono works on both
using System.IO;
using Newtonsoft.Json.Linq;

namespace Snobal.Library
{
	/**
	 * Sqlite caching implementation
	*/
	class SqliteCacher<T> : ICacher<T>
	{
		// DATA

		private bool isInitialised;

		private string databaseName;
		private string tableName;

		private SqliteConnection database;
		private ISerialiser<T> serialiser;
		private ISqliteCommands<T> commands;

		// PRIVATE

		/**
		 * Constructor
		 * 
		 * @param databaseName Full path and name to the database
		 * @param tableName The table to use in the database
		 */
		public SqliteCacher(string databaseName, string tableName, ISqliteCommands<T> commands, ISerialiser<T> serialiser)
		{
			this.databaseName = databaseName;
			this.tableName = tableName;
			this.commands = commands;
			this.serialiser = serialiser;
		}

		/**
		 * Finaliser
		 */
		~SqliteCacher()
		{
			Shutdown();
		}

		#region ICacher

		/**
		 * Init
		 *
		 * @throws Throws SnobalException on error
		*/
		public void Init()
        {
			ErrorHandling.Test(!isInitialised, "SqliteCacher already initialised");

			if (!File.Exists(databaseName))
			{
				SqliteConnection.CreateFile(databaseName);
			}

			database = new SqliteConnection("data source=" + databaseName);

			try
			{
				database.Open();
			}
			catch (DllNotFoundException e)
			{
				throw new SnobalException("Couldn't create an sqlite connection, see inner exception", "There was an issue trying to connect to the internal database", e);
			}

			using (var command = new SqliteCommand(database))
			{
				// Prevent accessing a non-existant table
				command.CommandText = commands.CreateTable(tableName);
				command.ExecuteNonQuery();
			}

			isInitialised = true;
		}

		/**
		 * Shutdown
		 *
		 * @throws Throws SnolibException on error
		*/
		public void Shutdown()
        {
			if (isInitialised)
			{
				// Close connection to db
				try
				{
					database.Close();
				}
				catch (Exception e)
				{
					throw new SnobalException("Couldn't close an sqlite connection, see inner exception", "There was an issue trying to close the internal database", e);
				}

				isInitialised = false;
			}
		}

		/**
		 * HasItems
		 *
		 * @return Return if the cache has items or not
		*/
		public bool HasItems()
        {
			ErrorHandling.Test(isInitialised, "SqliteCacher not initialised");

			int count = 0;

			// Querry db table for items
			using (var command = new SqliteCommand(database))
			{
				command.CommandText = commands.Count(tableName);

				var countObj = command.ExecuteScalar();
				if (countObj != null)
                {
					count = Convert.ToInt32(countObj);
                }
			}

			return count > 0;
		}

		/**
		 * AddItems
		 *
		 * Add items to the cache
		 *
		 * @param itemList The list of items to add to the cache
		*/
		public void AddItems(List<T> itemList)
        {
			ErrorHandling.Test(isInitialised, "SqliteCacher not initialised");

			// Add items to the db table
			foreach (var item in itemList)
			{
				using (var command = new SqliteCommand(database))
				{
					command.CommandText = commands.Insert(tableName, item);
					command.ExecuteNonQuery();
				}
			}
		}

		/**
		 * GetItems
		 *
		 * Retrieve items from the cache
		 *
		 * @param maxItems The maximum number of items to return
		 * @return The list populated with the retrieved items
		*/
		public List<T> GetItems(int maxItems)
        {
			ErrorHandling.Test(isInitialised, "SqliteCacher not initialised");

			List<T> outItemList;

			// Retrieve items from the db table
			using (var command = new SqliteCommand(database))
			{
				command.CommandText = commands.Select(tableName, maxItems);

				using (var reader = command.ExecuteReader())
				{
					JArray jsonItems = new JArray();
					while (reader.Read())
					{
						JObject jsonItem = new JObject();

						for (int i = 0; i < reader.FieldCount; ++i)
                        {
							jsonItem.Add(new JProperty(reader.GetName(i), reader[i].ToString()));
						}

						jsonItems.Add(jsonItem);
					}

					outItemList = serialiser.DeserialiseList(jsonItems.ToString());
				}
			}

			return outItemList;
		}

		/**
		* ClearItems
		*
		* Clear the given number of items from the cache
		*
		* @param count The number of items to clear from the cache
		*/
		public void ClearItems(int count)
        {
			ErrorHandling.Test(isInitialised, "SqliteCacher not initialised");

			// Remove items from the db table
			using (var command = new SqliteCommand(database))
			{
				command.CommandText = commands.Delete(tableName, count);
				command.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
