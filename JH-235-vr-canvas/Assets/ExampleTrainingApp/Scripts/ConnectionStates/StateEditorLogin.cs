using System;
using System.Collections;
using Snobal.CloudSDK;
using TMPro;
using UnityEngine;

public class StateEditorLogin : StateBase
{
    [SerializeField]
    private TextMeshProUGUI debugText;

    public override void onEnter()
    {
        base.onEnter();

        debugText.text = "Logging in";

        // Create a login data struct to record details about this device. This is optional, but useful to track device details on every device logging in. Must be run on main thread.
        var data = SnobalCloudInitialization.CreateLoginData();
        RunThreadedTask(SC.Login, data, LoggedIn);
    }

    /// <summary>
    /// Called when login routine has finished
    /// </summary>
    private void LoggedIn()
    {
        debugText.text = "LoggedIn sequence finished";
        if (SC.LoginScope.HasFailed)
        {
            debugText.text = "Login failed\nPlease run this application through a Snobal app.";
            // Application will now stay 'stuck' in this state
            // todo better error handling
            return;
        }

        StopRunningCoroutine();
        coroutine = LoginTaskComplete();
        StartCoroutine(coroutine);
    }

    /// <summary>
    /// Display login success before progressing to next state
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoginTaskComplete()
    {
        debugText.text = "Login success!";
        yield return new WaitForSeconds(sceneControl.exampleAppSettings.endOfTaskDelay);
        sceneControl.TriggerEvent(ExampleAppTriggers.Next);
    }
}