using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Snobal.Library
{
    [System.Serializable]
    public class ApplicationData
    {
        public string name;
        public string assetId;
        public string version;
        public string url;
        [JsonIgnore ]
        public string checksum;
        public string packageName;
        public bool autorun;
    }
}