using UnityEngine;
using UnityEngine.UI;

public class ExampleTask_1_2 : ExampleTaskBase
{
    [SerializeField]
    private Button finishedButton;

    public override void StartTask()
    {
        base.StartTask();

        Debug.Log("starting task 1-2");
        // todo here would be where the user would interact
        finishedButton.onClick.RemoveAllListeners();
        finishedButton.onClick.AddListener(EndTask);
    }
}