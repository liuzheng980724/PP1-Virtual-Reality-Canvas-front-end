using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityDebugLogger : Snobal.Library.ILogger
{
    private bool disposedValue;

    public void Log(string message, string logLevel)
    {
        switch (logLevel)
        {
            case "Debug": Debug.Log(message); break;
            case "Info": Debug.Log(message); break;
            case "Warning": Debug.LogWarning(message); break;
            case "Error": Debug.LogError(message); break;
            case "Fatal": Debug.LogError(message); break;
        }
    }

    public void Dispose()
    {
        // This is optional it seems
    }
}
