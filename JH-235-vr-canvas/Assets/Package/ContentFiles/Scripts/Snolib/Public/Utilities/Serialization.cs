using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Snobal.Utilities
{
    public static class Serialization
    {
        const string userErrroMsg = "An unexpected error has occurred.";

        /// <summary> Deserialises a chunk of json to a dynamic variable </summary>
        public static dynamic DeserializeToDynamic(string json)
        {
            object o;
            try
            {
                o = JsonConvert.DeserializeObject(json);
            }
            catch (Newtonsoft.Json.JsonException e)
            {
                throw new Library.SnobalException("Json Deserialize Exception: '" + e.Message + "' in string '" + json +"'", userErrroMsg, e);
            }
            return o;
        }

        /// <summary> Deserialises a chunk of json to a specified type variable </summary>
        public static T DeserializeToType<T>(string json)
        {
            T t = default(T);
            try
            {
                t = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Newtonsoft.Json.JsonException e)
            {
                throw new Library.SnobalException("Json Deserialize Exception: '" + e.Message + "' in string '" + json +"'", userErrroMsg, e);
            }
            return t;
        }

        public static bool AttemptDeserializeToType<T>(string json, out T _object)
        {
            _object = default(T);
            try
            {
                _object = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return false;
            }
            return true;
        }

        /// <summary> Serialises a object to a string </summary>
        public static string SerializeObject(object _object)
        {
            string s;
            
            try
            {
                s = JsonConvert.SerializeObject(_object);
            }
            catch (Newtonsoft.Json.JsonException e)
            {
                throw new Library.SnobalException("Json Serialize Exception: '" + e.Message + "' in object '" + _object.ToString() +"'", userErrroMsg, e);
            }
            return s;
        }
    }
}
