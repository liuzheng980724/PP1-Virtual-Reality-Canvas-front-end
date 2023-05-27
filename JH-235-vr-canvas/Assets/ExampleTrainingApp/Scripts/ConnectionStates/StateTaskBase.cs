using Snobal.CloudSDK;

public class StateTaskBase : StateBase
{
    protected bool startTaskSuccess;
    protected bool endTaskSuccess;

    protected void SendTaskOpenRequestToServer(string taskID)
    {
        var reportID = sceneControl.currentSchemeData.reportId;
        SnobalCloudMonobehaviour.TaskWatcher.Watch(
            System.Threading.Tasks.Task.Run
            (
                () =>
                {
                    // Seperate thread
                    startTaskSuccess = SC.StartSchemeTask(reportID, taskID);
                }
            ),
            () => { TaskOpened(); }
        );
    }

    private void TaskOpened()
    {
        if (!startTaskSuccess)
        {
            TaskFailedToOpened();
            return;
        }

        TaskOpenedSuccessfully();
    }

    protected virtual void TaskFailedToOpened()
    {
    }

    protected virtual void TaskOpenedSuccessfully()
    {
    }

    protected void SendTaskCloseRequestToServer(string taskID, string outcome)
    {
        var reportID = sceneControl.currentSchemeData.reportId;
        SnobalCloudMonobehaviour.TaskWatcher.Watch(
            System.Threading.Tasks.Task.Run
            (
                () =>
                {
                    // Seperate thread
                    endTaskSuccess = SC.EndSchemeTask(reportID, taskID, outcome);
                }
            ),
            () => { TaskClosedResponse(); }
        );
    }

    private void TaskClosedResponse()
    {
        if (!endTaskSuccess)
        {
            TaskFailedToClose();
            return;
        }

        TaskClosedSuccessfully();
    }


    protected virtual void TaskFailedToClose()
    {
    }

    protected virtual void TaskClosedSuccessfully()
    {
    }
}