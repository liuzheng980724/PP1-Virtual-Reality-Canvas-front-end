using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SchemeButton : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private TextMeshProUGUI buttonText;

    [SerializeField]
    private Toggle completedToggle;

    private Action<string> pressedAction;
    private string schemeID;

    public void UpdateButton(string key, Action<string> _onPress, string schemeID, bool schemeCompleted)
    {
        pressedAction = _onPress;
        this.schemeID = schemeID;
        var displayText = $"    Name: {key}\n SchemeID:{schemeID}";
        buttonText.text = displayText;

        completedToggle.isOn = schemeCompleted;
        if (schemeCompleted)
        {
            button.interactable = false;
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Pressed);
    }

    private void Pressed()
    {
        pressedAction?.Invoke(schemeID);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}