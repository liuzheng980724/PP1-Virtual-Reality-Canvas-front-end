using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Display the console output into the given scroll rect view as lines of text
/// </summary>
public class ConsoleToUI : MonoBehaviour
{
    [SerializeField]
    ScrollRect scrollRect = null;

    [SerializeField]
    Text consoleOutTxt = null;

    List<string> newLogs = new List<string>();
    float textHeight = 20;

    void OnEnable()
    {
        Application.logMessageReceived += LogConsoleOut;
        Application.logMessageReceivedThreaded += LogConsoleOut;

        RectTransform rectTransform = consoleOutTxt.gameObject.transform.GetComponentInChildren<RectTransform> ();
        textHeight = rectTransform.sizeDelta.y;
    }
     
    void OnDisable()
    {
        Application.logMessageReceived -= LogConsoleOut;
        Application.logMessageReceivedThreaded -= LogConsoleOut;
    }
     
    public void LogConsoleOut(string logString, string stackTrace, LogType type)
    {
        if(!newLogs.Contains(logString))
            newLogs.Add(logString);
    }

    int countOccurences(string find, string source)
    {
        return (source.Length - source.Replace(find,"").Length) / find.Length;
    }

    void Update()
    {
        if(newLogs.Count > 0)
        {
            string newLine;
            RectTransform rectTransform = consoleOutTxt.gameObject.transform.GetComponentInChildren<RectTransform> ();

            // Create lines of text for each log
            for(int i=0; i < newLogs.Count; i++)
            {
                newLine = newLogs[i] + System.Environment.NewLine;
                consoleOutTxt.text = consoleOutTxt.text + newLine;

                int numNewLines = countOccurences(System.Environment.NewLine, newLine);
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y + (numNewLines * textHeight));
            }
            newLogs.Clear();

            // Force scroll down to bottom of scroll rect
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalScrollbar.value=0f;
            Canvas.ForceUpdateCanvases();
        }
    }
}
