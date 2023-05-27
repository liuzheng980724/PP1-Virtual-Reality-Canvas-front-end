using System.Collections;
using TMPro;
using UnityEngine;

public class StateTask4 : StateTaskBase
{
    [SerializeField]
    private TextMeshProUGUI debugText;

    [SerializeField]
    private UITimer uiTimer;

    public override void onEnter()
    {
        base.onEnter();
        var taskID = sceneControl.exampleAppSettings.task4ID;
        SendTaskOpenRequestToServer(taskID);
    }

    protected override void TaskOpenedSuccessfully()
    {
        base.TaskOpenedSuccessfully();
        debugText.text = "Task4 opened";
        var timeToPutOutFire = sceneControl.exampleAppSettings.timeToPutOutFire;
        sceneControl.sceneObjectControl.ShowTask4Objects(TaskCompleted, timeToPutOutFire);
        var timeToTake = 60;
        uiTimer.StartTimer(timeToTake, TimeIsUp);
    }

    protected override void TaskFailedToOpened()
    {
        base.TaskFailedToOpened();
        debugText.text = "Task2 failed to open.";
    }

    private void TimeIsUp()
    {
        Debug.Log("TIME IS UP");
        StopRunningCoroutine();
        coroutine = TaskFailedSequence();
        StartCoroutine(coroutine);
    }

    private IEnumerator TaskFailedSequence()
    {
        uiTimer.StopTimer();
        uiTimer.Hide();
        debugText.text = "Task4 Failed";
        sceneControl.ShowTaskResult(false);

        yield return new WaitForSeconds(sceneControl.exampleAppSettings.endOfTaskDelay);

        string outcome = "Fail";
        var taskID = sceneControl.exampleAppSettings.task4ID;
        SendTaskCloseRequestToServer(taskID, outcome);
    }


    private void TaskCompleted()
    {
        StopRunningCoroutine();
        coroutine = TaskOpenedSequence();
        StartCoroutine(coroutine);
    }

    private IEnumerator TaskOpenedSequence()
    {
        // todo Error handling
        debugText.text = "Task4 Complete";
        sceneControl.ShowTaskResult(true);

        yield return new WaitForSeconds(sceneControl.exampleAppSettings.endOfTaskDelay);

        string outcome = "Pass";
        var taskID = sceneControl.exampleAppSettings.task4ID;
        SendTaskCloseRequestToServer(taskID, outcome);
    }

    protected override void TaskFailedToClose()
    {
        base.TaskFailedToClose();

        debugText.text = $"Task 4 Failed to close";
    }

    protected override void TaskClosedSuccessfully()
    {
        base.TaskClosedSuccessfully();
        sceneControl.HideTaskResult();
        sceneControl.TriggerEvent(ExampleAppTriggers.Next);
    }
}