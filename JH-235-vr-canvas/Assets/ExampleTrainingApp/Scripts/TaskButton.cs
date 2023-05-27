using System;
using Snobal.Library.DataStructures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskButton : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private TextMeshProUGUI buttonText;

    [SerializeField]
    private Toggle completedToggle;

    private Action<string> pressedAction;
    private string taskID;

    public void UpdateButton(TaskData data, Action<string> _onPress, bool completedState)
    {
        pressedAction = _onPress;
        taskID = data.ID;
        completedToggle.isOn = completedState;
        var displayText = $"{data.name} outcome:{data.outcome}";

        buttonText.text = displayText;
        if (completedState)
        {
            // task has been completed, disable interactivity
            button.interactable = false;
            return;
        }

        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Pressed);
    }

    private void Pressed()
    {
        pressedAction?.Invoke(taskID);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}