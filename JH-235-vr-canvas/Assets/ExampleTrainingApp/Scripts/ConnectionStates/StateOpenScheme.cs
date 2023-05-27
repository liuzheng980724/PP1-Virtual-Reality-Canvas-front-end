using System.Collections;
using Snobal.DesignPatternsUnity_0_0.Extensions;
using TMPro;
using UnityEngine;

public class StateOpenScheme : StateBase
{
    [SerializeField]
    private TextMeshProUGUI debugText;

    private int appIDIndex;

    public override void onEnter()
    {
        base.onEnter();

        debugText.text = "Opening Scheme";

        // This will attempt to open the scheme by trying each of the application IDs
        // within sceneControl.exampleAppSettings.productionAppID

        // This is done in order to smooth the development process when using multiple
        // tenants without having to do specific builds for each tenant/application ID
        // eg: a development and production tenant.

        appIDIndex = 0;
        OpenNextAppID();
    }


    private void OpenNextAppID()
    {
        if (appIDIndex >= sceneControl.exampleAppSettings.productionAppIDs.Length)
        {
            FailedToOpenAnyScheme();
            return;
        }

        var appID = sceneControl.exampleAppSettings.productionAppIDs[appIDIndex];
        appIDIndex++;

        if (appID != null)
        {
            OpenScheme(appID);
        }
        else
        {
            OpenNextAppID();
        }
    }

    private void FailedToOpenAnyScheme()
    {
        // failed to open any
        Snobal.Library.Logger.Log($"Scheme Error response:{SC.RecievedSchemeData.reportId}");
        debugText.text = $"Scheme Error response:{SC.RecievedSchemeData.reportId}";
        sceneControl.TriggerEvent(ExampleAppTriggers.Failed);
    }

    private void OpenScheme(string appID)
    {
        RunThreadedTask(SC.StartScheme, sceneControl.exampleAppSettings.schemeID, appID, SchemeOpened);
    }

    private void SchemeOpened()
    {
        Debug.Log($"SC.RecievedSchemeData.reportId:{SC.RecievedSchemeData.reportId}");
        var reportID = SC.RecievedSchemeData.reportId;
        if (string.IsNullOrEmpty(reportID) || reportID.Contains("Error"))
        {
            debugText.text = "trying next App ID";
            // try the next scheme
            OpenNextAppID();
            return;
        }

        StopRunningCoroutine();
        coroutine = SchemeOpenedSequence();
        StartCoroutine(coroutine);
    }

    private IEnumerator SchemeOpenedSequence()
    {
        debugText.text = "Scheme Loaded";
        Debug.Log($"SC.RecievedSchemeData.reportId:{SC.RecievedSchemeData.reportId}");

        yield return new WaitForSeconds(sceneControl.exampleAppSettings.endOfTaskDelay);

        Snobal.Library.Logger.Log($"REPORT ID:{SC.RecievedSchemeData.reportId}".Colorize(Color.yellow));

        sceneControl.currentSchemeData = SC.RecievedSchemeData;

        sceneControl.TriggerEvent(ExampleAppTriggers.Next);
    }
}