using System;
using UnityEngine;

public class MutliChoiceTest : MonoBehaviour
{
    [SerializeField]
    private GameObject holder;

    [SerializeField]
    private MultiChoiceButton[] multiChoiceButtons;

    [SerializeField]
    private MultiChoiceButtonData[] data;

    private Action<bool> finishedCallback;
    private bool answerSubmitted;

    public void Show(Action<bool> _finishedCallback)
    {
        // animate in?
        answerSubmitted = false;
        finishedCallback = _finishedCallback;
        holder.SetActive(true);
        var index = 0;
        foreach (var button in multiChoiceButtons)
        {
            var buttonData = data[index];
            button.Setup(buttonData, ClickCallBack);
            index++;
        }
    }

    private void ClickCallBack(MultiChoiceButtonData data)
    {
        if (answerSubmitted)
        {
            return;
        }

        answerSubmitted = true;
        Debug.Log($"Player clicked:{data.buttonText}");
        finishedCallback.Invoke(data.isCorrectAnswer);
        DisableButtons();
    }

    private void DisableButtons()
    {
        foreach (var button in multiChoiceButtons)
        {
            button.enabled = false;
        }
    }

    public void Hide()
    {
        HideInstantly();
    }

    public void HideInstantly()
    {
        holder.SetActive(false);
    }
}