using UnityEngine;
using UnityEngine.UI;

public class ExampleTask_2_1 : ExampleTaskBase
{
    [SerializeField]
    private Button finishedButton;

    public override void StartTask()
    {
        base.StartTask();

        Debug.Log("starting task 2-1");
        // todo here would be where the user would interact
        finishedButton.onClick.RemoveAllListeners();
        finishedButton.onClick.AddListener(EndTask);
    }
}