using System;
using System.Collections.Generic;

namespace Snobal.Library.DataStructures
{
    // Tasks
    [Serializable]
    public class OutcomeData
    {
        public string type;
    }

    // Tasks
    [Serializable]
    public class TaskData
    {
        public string ID;
        public string name;
        public string outcome;
    }

    // Schemes
    [Serializable]
    public class SchemesData
    {
        public Dictionary<string, SchemeData> activities = null;
        public Dictionary<string, TaskData> tasks = null;
        public Dictionary<string, OutcomeData> outcomes = null;
    }

    [Serializable]
    public class SchemeData
    {
        public string participantId = null;
        public string reportId = null;
        public string name = null;
        public string ID = null;
        public string outcome = null;
        public TaskData[] tasks = null;       // taskID's
    }
    
    [Serializable]
    public class EndSchemeData
    {
        public string schemeId = null;
        public string participantId = null;
    }
}
