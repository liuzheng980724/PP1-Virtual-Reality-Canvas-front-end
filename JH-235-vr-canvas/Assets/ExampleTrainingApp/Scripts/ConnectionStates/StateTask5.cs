using System.Collections;
using Snobal.CloudSDK;
using TMPro;
using UnityEngine;

public class StateTask5 : StateTaskBase
{
    [SerializeField]
    private TextMeshProUGUI debugText;

    [SerializeField]
    private MutliChoiceTest multiChoiceTest;

    public override void onEnter()
    {
        base.onEnter();
        var taskID = sceneControl.exampleAppSettings.task5ID;
        SendTaskOpenRequestToServer(taskID);
    }

    protected override void TaskOpenedSuccessfully()
    {
        base.TaskOpenedSuccessfully();
        SetupDummyTest();
        sceneControl.sceneObjectControl.ShowTask5Objects();
    }

    protected override void TaskFailedToOpened()
    {
        base.TaskFailedToOpened();
        debugText.text = "Task 5 failed to open.";
    }

    private void SetupDummyTest()
    {
        debugText.text = "Task 5 opened";
        multiChoiceTest.Show(TaskCompleted);
    }

    private void TaskCompleted(bool passed)
    {
        StopRunningCoroutine();
        coroutine = TaskCompletedSequence(passed);
        StartCoroutine(coroutine);
    }

    private IEnumerator TaskCompletedSequence(bool passed)
    {
        multiChoiceTest.Hide();

        var outcome = "Pass";
        if (passed)
        {
            debugText.text = "Task5 complete - RIGHT ANSWER";
            sceneControl.ShowTaskResult(true);
        }
        else
        {
            outcome = "Fail";
            debugText.text = "Task5 complete - WRONG ANSWER";
            sceneControl.ShowTaskResult(false);
        }

        yield return new WaitForSeconds(sceneControl.exampleAppSettings.endOfTaskDelay);

        var taskID = sceneControl.exampleAppSettings.task5ID;
        SendTaskCloseRequestToServer(taskID, outcome);
    }

    protected override void TaskFailedToClose()
    {
        base.TaskFailedToClose();

        debugText.text = $"Task 5 Failed to close";
    }

    protected override void TaskClosedSuccessfully()
    {
        base.TaskClosedSuccessfully();
        sceneControl.HideTaskResult();
        sceneControl.TriggerEvent(ExampleAppTriggers.Next);
    }
}