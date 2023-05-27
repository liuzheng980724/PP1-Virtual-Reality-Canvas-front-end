using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snobal.Library
{
	/**
     * Generic for Sqlite commands
     */
	internal interface ISqliteCommands<T>
	{
		/**
         * CreateTable
         *
         * Create a table for the given template type
         *
         * @param tableName The table to create
         * @return The return sql statement
        */
		string CreateTable(string tableName);


		/**
         * Insert
         *
         * Insert the given data object into the table
         *
         * @param tableName The table to insert into
         * @return The return sql statement
        */
		string Insert(string tableName, T data);

		/**
         * Count
         *
         * Count records in a table
         *
         * @param tableName The table to count from
         * @param outString The return sql statement
        */
		string Count(string tableName);

		/**
         * Select
         *
         * Select records from table
         *
         * @param tableName The table to select from
         * @param maxItems The maximum number of statements to cap the select at
         * @return The return sql statement
        */
		string Select(string tableName, int maxItems);

		/**
         * Remove
         *
         * Remove a given number of records from table
         *
         * @param tableName The table to remove from
         * @param itemCount The desired number of items to remove
         * @return The return sql statement
        */
		string Delete(string tableName, int itemCount);
    }
}
