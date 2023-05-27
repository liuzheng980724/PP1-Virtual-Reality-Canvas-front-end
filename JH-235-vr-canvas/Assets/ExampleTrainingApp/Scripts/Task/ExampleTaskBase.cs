using System;
using UnityEngine;

public class ExampleTaskBase : MonoBehaviour
{
    protected string taskID;
    protected Action<bool> taskEndedCallback;
    protected bool currentResult = false;


    public virtual void Setup(string _taskID, Action<bool> _taskEndedCallback)
    {
        taskID = _taskID;
        taskEndedCallback = _taskEndedCallback;
    }

    public virtual void StartTask(){
    }


    public virtual void EndTask()
    {
        Debug.Log("End task");


        taskEndedCallback.Invoke(currentResult);
    }
}