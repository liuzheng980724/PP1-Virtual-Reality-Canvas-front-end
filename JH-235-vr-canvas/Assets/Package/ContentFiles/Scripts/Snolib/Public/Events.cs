using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mono.Data.Sqlite;     // Was using System.Data.SQLite, but didn't work on Android. Using Mono works on both
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Snobal.Library
{
    public static class Events
    {
        // CLASSES

        public class Event
        {
            public Categories category;
            public object body;
            public Event(Categories _category, object _body)
            {
                category = _category;
                body = _body;
            }
        }

        // ENUMS
        public enum Categories { system, application, experience}

        // DATA

        private static bool isInitialised;

        private static IPoster<EventData> poster;

        private static string storedAppType;

        // PUBLIC

        public static bool IsInitialised => isInitialised;

        public static void Init(string appType)
        {
            ErrorHandling.Test(FileIO.isInitialised, "Must initialise FileIO before initialising Logging");

            storedAppType = appType;

            poster = CreateEventCachedPoster(storedAppType);
            poster.Init();
            poster.Enabled = true;

            isInitialised = true;
        }

        public static void InitLiveConnection(SnobalCloud sc)
        {
            ErrorHandling.Test(isInitialised, "Tried to init a live connection before Event has been initialised");

            if (poster != null)
            {
                poster.Shutdown();
            }

            poster = CreateEventLivePoster(storedAppType, sc.Tenant.BuildTenantAPIURL(API.eventDatabaseExtension), sc.RestClient);
            poster.Init();
            poster.Enabled = true;
        }

        public static void Shutdown()
        {
            if (isInitialised)
            {
                poster.Shutdown();

                isInitialised = false;
            }
        }

        public static void Post(string message, string level)
        {
            ErrorHandling.Test(isInitialised, string.Format("Tried to post {0} before Event Post is initialised", message));

            // ' and " chars inside json cause sql write errors. Lets remove them to be safe.
            string safeMsg = message.Replace("'", "").Replace("\"", "");

            JObject bodyJson = new JObject();
            bodyJson.Add(new JProperty("level", level));
            bodyJson.Add(new JProperty("message", safeMsg));

            poster.Post(new EventData
            {
                uuid = Guid.NewGuid().ToString(),
                body = bodyJson.ToString(),
                timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                category = Categories.application.ToString()
            });
        }

        // PRIVATE

        private static IPoster<EventData> CreateEventCachedPoster(string appType)
        {
            var cacher = new SqliteCacher<EventData>(FileIO.GetDatabaseFilePath(appType), "Events", new EventDataSqliteCommands(), new EventDataSerialiser());
            //var poster = new LogPoster<EventData>((uint)Logger.LogTypeFlags.NetworkDebug, new EventDataSerialiser());
            return new CachedPoster<EventData>(cacher, null, 5000, 10, new EventDataSerialiser());
        }

        private static IPoster<EventData> CreateEventLivePoster(string appType, string databaseURL, IRestClient restClient)
        {
            var cacher = new SqliteCacher<EventData>(FileIO.GetDatabaseFilePath(appType), "Events", new EventDataSqliteCommands(), new EventDataSerialiser());
            var poster = new HttpPoster<EventData>(databaseURL, new EventDataSerialiser(), restClient);
            return new CachedPoster<EventData>(cacher, poster, 5000, 10, new EventDataSerialiser());
        }
    }
}
