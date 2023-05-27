using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StateTask1 : StateTaskBase
{
    [SerializeField]
    private TextMeshProUGUI debugText;

    [SerializeField]
    private NumericKeyboard numericKeyboard;

    [SerializeField]
    private VRInputField inputField;

    [SerializeField]
    private Button submitCodeButton;

    [SerializeField]
    private KeyboardSFX keyboardSFX;

    public override void onEnter()
    {
        base.onEnter();
        var reportID = sceneControl.currentSchemeData.reportId;

        submitCodeButton.onClick.RemoveListener(SubmitButtonClickedEvent);
        submitCodeButton.onClick.AddListener(SubmitButtonClickedEvent);

        var taskID = sceneControl.exampleAppSettings.task1ID;
        SendTaskOpenRequestToServer(taskID);
    }

    public override void onExit()
    {
        base.onExit();
        submitCodeButton.onClick.RemoveListener(SubmitButtonClickedEvent);
    }

    private void SubmitButtonClickedEvent()
    {
        TaskCompleted();
    }

    protected override void TaskOpenedSuccessfully()
    {
        base.TaskOpenedSuccessfully();
        debugText.text = "Task 1 opened";
        sceneControl.sceneObjectControl.ShowTask1Objects();
        ShowInputUserCodeField();
    }

    protected override void TaskFailedToOpened()
    {
        base.TaskFailedToOpened();
        debugText.text = "Task 1 failed to open.";
    }

    private void TaskCompleted()
    {
        StopRunningCoroutine();
        coroutine = TaskOpenedSequence();
        StartCoroutine(coroutine);
    }

    private IEnumerator TaskOpenedSequence()
    {
        // todo Error handling
        HideInputUserCodeField();

        keyboardSFX.PlayKeyPressSFX(0.0f);
        string outcome = "Fail";
        // has the user entered the correct value?
        if (inputField.text == sceneControl.exampleAppSettings.extinguisherExpiryDate)
        {
            outcome = "Pass";
            sceneControl.ShowTaskResult(true);
        }
        else
        {
            sceneControl.ShowTaskResult(false);
        }

        debugText.text = $"Task1 complete: Result - {outcome}";

        yield return new WaitForSeconds(1.0f);

        //var reportID = sceneControl.currentUserData.currentSchemeData.reportId;
        var taskID = sceneControl.exampleAppSettings.task1ID;
        SendTaskCloseRequestToServer(taskID, outcome);
    }

    protected override void TaskFailedToClose()
    {
        base.TaskFailedToClose();

        debugText.text = $"Task 1 Failed to close";
    }

    protected override void TaskClosedSuccessfully()
    {
        base.TaskClosedSuccessfully();
        sceneControl.HideTaskResult();
        sceneControl.TriggerEvent(ExampleAppTriggers.Next);
    }


    private void Update()
    {
        // Check if submit button should be enabled
        if (inputField.text.Length > 1)
        {
            submitCodeButton.interactable = true;
        }
        else
        {
            submitCodeButton.interactable = false;
        }
    }

    private void HideInputUserCodeField()
    {
        numericKeyboard.oskfocus = null;
        numericKeyboard.gameObject.SetActive(false);

        submitCodeButton.gameObject.SetActive(false);
        submitCodeButton.enabled = false;

        inputField.gameObject.SetActive(false);
    }

    private void ShowInputUserCodeField()
    {
        numericKeyboard.oskfocus = inputField;
        numericKeyboard.gameObject.SetActive(true);

        submitCodeButton.gameObject.SetActive(true);
        submitCodeButton.interactable = false;

        inputField.gameObject.SetActive(true);
    }
}