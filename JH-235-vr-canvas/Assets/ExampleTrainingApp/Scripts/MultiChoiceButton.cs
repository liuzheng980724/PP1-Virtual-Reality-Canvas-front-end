using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiChoiceButton : MonoBehaviour
{
    [SerializeField]
    private Button buttonRef;

    [SerializeField]
    private TextMeshProUGUI buttonText;

    private Button buttons;
    private MultiChoiceButtonData buttonData;
    private Action<MultiChoiceButtonData> callbackClick;
    private void OnEnable()
    {
        buttonRef.onClick.AddListener(Clicked);
    }

    private void Clicked()
    {
        callbackClick.Invoke(buttonData);
    }

    public void Setup(MultiChoiceButtonData _buttonData, Action<MultiChoiceButtonData> _callbackClick)
    {
        callbackClick = _callbackClick;
        buttonData = _buttonData;
        buttonText.text = buttonData.buttonText;
    }
}