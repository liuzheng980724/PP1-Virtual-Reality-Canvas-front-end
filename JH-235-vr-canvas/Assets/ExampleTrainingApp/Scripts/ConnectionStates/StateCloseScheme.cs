using System.Collections;
using UnityEngine;

public class StateCloseScheme : StateBase
{
    public override void onEnter()
    {
        base.onEnter();

        var reportID = sceneControl.currentSchemeData.reportId;
        // Reaching this far is considered a 'pass' of the whole scheme. The user completed the scheme.
        const string finishedWholeTest = "pass";
        RunThreadedTask(SC.EndScheme, reportID, sceneControl.currentTaskID, finishedWholeTest,
            SchemeClosed);
    }

    private void SchemeClosed()
    {
        StopRunningCoroutine();
        coroutine = SchemeClosedSequence();
        StartCoroutine(coroutine);
    }

    private IEnumerator SchemeClosedSequence()
    {
        yield return new WaitForSeconds(sceneControl.exampleAppSettings.endOfTaskDelay);
        sceneControl.TriggerEvent(ExampleAppTriggers.Next);
    }
}