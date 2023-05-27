using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

class AndroidPluginCallback : AndroidJavaProxy
{
    private Action<string> success;
    private Action fail;

    public AndroidPluginCallback(Action<string> success, Action fail) : base("com.snobal.fileretriever.PluginCallback")
    {
        this.success = success;
        this.fail = fail;
    }

    public void onSuccess(string fileName)
    {
        Snobal.Library.Logger.Log("PreparingState callback onSuccess: " + fileName);

        success?.Invoke(fileName);
    }
    public void onError(string errorMessage)
    {
        Snobal.Library.Logger.Log("PreparingState callback onError: " + errorMessage);

        fail?.Invoke();
    }
}
