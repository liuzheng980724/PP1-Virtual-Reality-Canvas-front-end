using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a fun little class to automatically log stuff to the editor's game window GUI
namespace Snobal.XR.Debugging
{
    public class EditorOnScreenLog : MonoBehaviour
    {
        static Dictionary<string, string> onScreenLog = new Dictionary<string, string>();

        private Rect rect = new Rect(0, Screen.height - (Screen.height * 0.2f), Screen.width * 0.3f, Screen.height * 0.2f);

        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInit()
        {
            if (Application.isEditor)
                new GameObject("[EDITOR ONLY] OnScreenLog").AddComponent<EditorOnScreenLog>();                            
        }

        /// <summary>
        /// Adds a line to the onscreen log based on an unique access key, if key already exists, it will overwrite the value unless the overwrite parameter is false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="log"></param>
        /// <param name="overwrite"></param>
        public static void AddOnScreenLog(string key, string log, bool overwrite = true)
        {
            if (onScreenLog.ContainsKey(key) && overwrite)
            {
                onScreenLog[key] = log;
                return;
            }
            onScreenLog.Add(key, log);
        }

        public static void RemoveOnScreenLog(string key)
        {
            if (onScreenLog.ContainsKey(key))
                onScreenLog.Remove(key);            
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (onScreenLog!=null)
            {
                string displayString = string.Empty;

                foreach(var log in onScreenLog)
                    displayString = $"{log.Key}\t:\t{log.Value}\n" + displayString;                
                
                GUIStyle style = new GUIStyle()
                {
                    alignment = TextAnchor.LowerLeft,
                    normal = new GUIStyleState()
                    {
                        textColor = Color.red
                    }
                };

                GUI.Label(rect, displayString, style);
            }
        }
#endif
    }
}
