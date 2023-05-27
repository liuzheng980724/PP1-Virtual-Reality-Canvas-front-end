using System.Collections;
using TMPro;
using UnityEngine;

public class StateTask3 : StateTaskBase
{
    [SerializeField]
    private TextMeshProUGUI debugText;

    public override void onEnter()
    {
        base.onEnter();
        var taskID = sceneControl.exampleAppSettings.task3ID;
        SendTaskOpenRequestToServer(taskID);
    }

    protected override void TaskFailedToOpened()
    {
        base.TaskFailedToOpened();
        debugText.text = "Task 3 failed to open.";
    }

    protected override void TaskOpenedSuccessfully()
    {
        debugText.text = "Task 3 opened";
        sceneControl.sceneObjectControl.ShowTask3Objects(TaskCompleted);
    }

    private void TaskCompleted()
    {
        StopRunningCoroutine();
        coroutine = TaskOpenedSequence();
        StartCoroutine(coroutine);
    }

    private IEnumerator TaskOpenedSequence()
    {
        sceneControl.ShowTaskResult(true);
        // todo Error handling
        debugText.text = "Task3 Complete";

        yield return new WaitForSeconds(sceneControl.exampleAppSettings.endOfTaskDelay);

        string outcome = "Pass";
        //var reportID = sceneControl.currentUserData.currentSchemeData.reportId;
        var taskID = sceneControl.exampleAppSettings.task3ID;
        SendTaskCloseRequestToServer(taskID, outcome);
    }

    protected override void TaskFailedToClose()
    {
        base.TaskFailedToClose();

        debugText.text = $"Task 3 Failed to close";
    }

    protected override void TaskClosedSuccessfully()
    {
        base.TaskClosedSuccessfully();
        sceneControl.HideTaskResult();
        sceneControl.TriggerEvent(ExampleAppTriggers.Next);
    }
}