using System.Collections;
using TMPro;
using UnityEngine;
using Logger = Snobal.Library.Logger;

public class StateSetup : StateBase
{
    [SerializeField]
    private TextMeshProUGUI debugText;

    public override void onEnter()
    {
        base.onEnter();

        StopRunningCoroutine();
        coroutine = Sequence();
        StartCoroutine(coroutine);
    }

    private IEnumerator Sequence()
    {
        // The app will have some data passed into it via 'intents' that will setup the users deviceToken etc
        Logger.Log($"SC.Settings.Values.DeviceToken:{SC.Settings.Values.DeviceToken}");
        // configure the connection with the tenant when loading the settings

        if (SC.Settings.Values.TenantURL != null)
        {
            SC.ConfigureTenant(SC.Settings.Values.TenantURL);
        }

        if (SC.Settings.Values.DeviceToken != string.Empty)
        {
            sceneControl.HideTaskResult(true);
            // User is logged in already
            debugText.text = "Paired";
        }
        else
        {
            // In the Editor the states will attempt to pair and login
            var errorMessage = "Error: Not paired";

#if UNITY_EDITOR
            // this will now attempt to pair
            errorMessage += "\nEditor needs valid pairing code.";

            yield return new WaitForSeconds(1.0f);
            sceneControl.TriggerEvent(ExampleAppTriggers.Failed);
#else
            errorMessage += "\nPlease run this application through the Snobal Sync app.";
            sceneControl.TriggerEvent(ExampleAppTriggers.Failed);
#endif
            debugText.text = errorMessage;
            yield break;
        }

        yield return new WaitForSeconds(1.0f);

        sceneControl.TriggerEvent(ExampleAppTriggers.Next);
    }
}