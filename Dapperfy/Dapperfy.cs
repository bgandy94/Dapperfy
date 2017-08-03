using System;
using System.Collections.Generic;
using System.Reflection;

using Dapperfy.Attributes;

namespace Dapperfy
{
    public static class Dapperfy
    {

        #region Public Methods

        public static string FindById(int id, string tableName, string pkName)
        {
            return GenerateFindByIdSelectQuery(id, tableName, pkName);
        }

        /// <summary>
        /// Generates a SELECT query that selects all records with a table name of obj with an "s" at the end.
        /// For example, if you have a Person class, this method assumes the table name is Persons.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetAll<T>() where T : class
        {
            return GenerateStarSelectQuery($"{typeof(T)}s");
        }

        /// <summary>
        /// Generates a SELECT query from the provided Table name
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string GetAll(string tableName)
        {
            return GenerateStarSelectQuery(tableName);
        }

        /// <summary>
        /// Generates a SELECT query that selects the number of rows equal to the recordCount parameter being passed in.
        /// </summary>
        /// <param name="tableName">Name of the table to select from</param>
        /// <param name="recordCount">Number of records to select</param>
        /// <returns>query string</returns>
        public static string GetAll(string tableName, int recordCount)
        {
            return GenerateStarSelectQuery(tableName, recordCount);
        }

        /// <summary>
        /// Generates a SELECT query that only selects the properties passed into the properties parameter
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static string Get(string tableName, params string[] properties)
        {
            var query = $"SELECT {String.Join(",", properties)} FROM {tableName}";
            return query;

        }

        /// <summary>
        /// Generates an INSERT script with the provided table name and the provided object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string Add<T>(T obj, string tableName) where T : class
        {
            return GenerateAddScript(obj, tableName);
        }

        /// <summary>
        /// Generates an insert script without a table name, assuming the table name is equal to the pluralized version of the 
        /// model name. IE Model: Person, TableName: Persons
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Add<T>(T obj) where T : class
        {
            return GenerateAddScript(obj);
        }

        /// <summary>
        /// Generates an UPDATE script without a table name, assuming the table name is equal to the pluralized version of the 
        /// model name. IE Model: Person, TableName: Persons
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Update<T>(T obj) where T : class
        {
            return GenerateUpdateScript(obj, $"{obj.GetType().Name}s");
        }

        /// <summary>
        /// Generates an UPDATE Script to update the provided tableName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string Update<T>(T obj, string tableName) where T : class
        {
            return GenerateUpdateScript(obj, tableName);
        }

        /// <summary>
        /// Generates a DELETE script with the tableName, id, and primaryKeyName provided
        /// </summary>
        /// <param name="primaryKeyName">Name of Primary key in the DB</param>
        /// <param name="id"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string Delete(int id, string primaryKeyName, string tableName)
        {
            return GenerateDeleteScript(id, (object)null, tableName, primaryKeyName);
        }

        /// <summary>
        /// Generates a DELETE script with the table name as plural obj
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string Delete<T>(int id, T obj) where T : class
        {
            return GenerateDeleteScript(id, obj, $"{obj.GetType().Name}s");
        }

        /// <summary>
        /// Generates a DELETE script with the table name as plural obj
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string Delete<T>(T obj) where T : class
        {
            return GenerateDeleteScript(null, obj, $"{obj.GetType().Name}s");
        }

        /// <summary>
        /// Generates a DELETE script with the table name as param tableName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="id"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string Delete<T>(T obj, int id, string tableName) where T : class
        {
            return GenerateDeleteScript(id, obj, tableName);
        }

        #endregion

        #region Private Methods
        private static KeyValuePair<string, dynamic> FindPrimaryKeyAttributeName<T>(T obj) where T : class
        {
            KeyValuePair<string, dynamic> pk;
            foreach (var prop in obj.GetType().GetTypeInfo().DeclaredProperties)
            {
                if (prop.GetCustomAttribute(typeof(PrimaryKeyAttribute), false) != null)
                {
                    pk = new KeyValuePair<string, dynamic>(prop.Name, prop.GetValue(obj));
                    break;
                }
            }
            return pk;
        }

        private static string GenerateAddScript<T>(T obj, string tableName = null) where T : class
        {
            var table = tableName == null ? obj.GetType().Name + 's' : tableName;
            var propertyList = new List<string>();
            var valueList = new List<string>();
            GetPropertiesAndValuesOfObject(obj, out propertyList, out valueList);

            var query = $@"INSERT INTO {table}({String.Join(", ", propertyList)})
                           OUTPUT Inserted.Id
                           VALUES({String.Join(", ", valueList)})";

            return query;
        }

        private static string GenerateDeleteScript<T>(int? id, T obj, string tableName, string pkName = null) where T : class
        {
            KeyValuePair<string, dynamic> pk;
            pk = obj == null ? new KeyValuePair<string, dynamic>(pkName, id) : FindPrimaryKeyAttributeName(obj);

            var script = $@"DELETE FROM {tableName}
                            WHERE {pk.Key} = {pk.Value}";
            return script;
        }

        private static string GenerateFindByIdSelectQuery(int id, string tableName, string pkName)
        {
            var query = $@"SELECT * FROM {tableName} 
                           WHERE {pkName} = {id}";
            return query;
        }

        private static string GenerateStarSelectQuery(string tableName, int recordCount = -1)
        {

            return recordCount == -1 ? $"SELECT * FROM {tableName}" :
                                       $"SELECT TOP {recordCount} * FROM {tableName}";
        }

        private static void GetPropertiesAndValuesOfObject<T>(T obj, out List<string> propList, out List<string> valList) where T : class
        {
            var pk = FindPrimaryKeyAttributeName(obj);
            var currentValue = "";
            propList = new List<string>();
            valList = new List<string>();
            foreach (var prop in obj.GetType().GetTypeInfo().DeclaredProperties)
            {
                if (prop.Name == pk.Key) continue;
                propList.Add(prop.Name);

                currentValue = WrapInSingleQuotes(prop.PropertyType) ? $"'{prop.GetValue(obj)}'" : $"{prop.GetValue(obj)}";
                valList.Add(currentValue);
            }
        }

        private static string GenerateUpdateScript<T>(T obj, string tableName) where T : class
        {
            var pk = FindPrimaryKeyAttributeName(obj);
            var propList = new List<string>();
            var valList = new List<string>();
            var setList = new List<string>();
            GetPropertiesAndValuesOfObject(obj, out propList, out valList);
            for (var i = 0; i < propList.Count; i++)
            {
                var isLast = i == propList.Count - 1;
                setList.Add($"{propList[i]} = {valList[i]}");
            }

            var script = $@"UPDATE {tableName}
                            SET {string.Join(",", setList)}
                            WHERE {pk.Key} = {pk.Value}";
            return script;
        }

        private static bool WrapInSingleQuotes(Type propType)
        {
            return propType == typeof(string) ||
                    propType == typeof(DateTime);
        }
        #endregion
    }
}