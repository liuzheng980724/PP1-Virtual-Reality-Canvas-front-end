
using UnityEngine;
using UnityEngine.UI;

public class ExampleTask_1_1 : ExampleTaskBase
{
    [SerializeField]
    private Button finishedButton;

    public override void StartTask()
    {
        base.StartTask();

        Debug.Log("starting ExampleTask 1-1");
        // todo here would be where the user would interact
        finishedButton.onClick.RemoveAllListeners();
        finishedButton.onClick.AddListener(EndTask);

        // todo display what is required of the task
        // todo progress through what it takes to complete the task
        // todo call end task when complete
    }


    public override void EndTask()
    {
        currentResult = Random.Range(0, 10) < 5;
        base.EndTask();
    }
}