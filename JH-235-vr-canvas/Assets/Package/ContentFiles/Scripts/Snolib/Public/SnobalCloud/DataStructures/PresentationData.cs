using System;
using System.Collections.Generic;

namespace Snobal.Library.DataStructures
{
    // Presentations
    [Serializable]
    public class PresentationData
    {
        public string presentationId;
        public string creator;
        public long createTime;
        public string modifier;
        public long modifyTime;
        public string name;
        public string environmentId;
        public string[] languages;
        public Dictionary<string, string[]> voices = null;
    }

    // Voices
    [Serializable]
    public class VoicesData
    {
        public string[] voiceGUID;
    }
}
