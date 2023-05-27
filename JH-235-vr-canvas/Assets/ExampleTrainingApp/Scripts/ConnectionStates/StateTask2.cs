using System.Collections;
using TMPro;
using UnityEngine;

public class StateTask2 : StateTaskBase
{
    [SerializeField]
    private TextMeshProUGUI debugText;

    [SerializeField]
    private UITimer uiTimer;

    public override void onEnter()
    {
        base.onEnter();
        // hard code the task ID here, this is going to be
        var taskID = sceneControl.exampleAppSettings.task2ID;
        SendTaskOpenRequestToServer(taskID);
    }

    protected override void TaskFailedToOpened()
    {
        base.TaskFailedToOpened();
        debugText.text = "Task 2 failed to open.";
    }

    protected override void TaskOpenedSuccessfully()
    {
        base.TaskOpenedSuccessfully();

        debugText.text = "Task 2 opened.";

        sceneControl.sceneObjectControl.ShowTask2Objects(TaskCompleted);
        var timeToTake = 30;
        uiTimer.StartTimer(timeToTake, TimeIsUp);
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
        sceneControl.ShowTaskResult(false);
        uiTimer.StopTimer();
        uiTimer.Hide();
        debugText.text = "Task2 Failed";

        yield return new WaitForSeconds(sceneControl.exampleAppSettings.endOfTaskDelay);

        string outcome = "Fail";
        var reportID = sceneControl.currentSchemeData.reportId;
        var taskID = sceneControl.exampleAppSettings.task2ID;
        //RunThreadedTask(SC.EndSchemeTask, reportID, taskID, outcome, CloseTaskFinished);
        
        SendTaskCloseRequestToServer(taskID, outcome);
    }

    private void TaskCompleted()
    {
        Debug.Log("Task 2 completed");
        StopRunningCoroutine();
        coroutine = TaskCompletedSequence();
        StartCoroutine(coroutine);
    }

    private IEnumerator TaskCompletedSequence()
    {
        sceneControl.ShowTaskResult(true);
        uiTimer.StopTimer();
        uiTimer.Hide();

        // todo Error handling
        debugText.text = "Task2 Complete";

        yield return new WaitForSeconds(sceneControl.exampleAppSettings.endOfTaskDelay);

        string outcome = "Pass";
        //var reportID = sceneControl.currentUserData.currentSchemeData.reportId;
        var taskID = sceneControl.exampleAppSettings.task2ID;
        //RunThreadedTask(SC.EndSchemeTask, reportID, taskID, outcome, CloseTaskFinished);
        SendTaskCloseRequestToServer(taskID, outcome);
    }


    protected override void TaskFailedToClose()
    {
        base.TaskFailedToClose();

        debugText.text = $"Task 2 Failed to close";
    }

    protected override void TaskClosedSuccessfully()
    {
        base.TaskClosedSuccessfully();
        sceneControl.HideTaskResult();
        sceneControl.TriggerEvent(ExampleAppTriggers.Next);
    }


    public override void Init(SceneControl _sceneControl)
    {
        base.Init(_sceneControl);
        uiTimer.HideInstantly();
    }

    protected override void HideInstantly()
    {
        uiTimer.StopTimer();
        uiTimer.HideInstantly();
        base.HideInstantly();
    }
}