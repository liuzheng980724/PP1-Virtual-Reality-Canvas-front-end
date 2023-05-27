using System.Collections;
using Snobal.CloudSDK;
using Snobal.DesignPatternsUnity_0_0.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StateValidateUser : StateBase
{
    [SerializeField]
    private TextMeshProUGUI feedbackText;

    [SerializeField]
    private VRInputField inputField;

    [SerializeField]
    private Button submitCodeButton;

    [SerializeField]
    private Button guestButton;

    [SerializeField]
    private NumericKeyboard numericKeyboard;

    [SerializeField]
    private KeyboardSFX keyboardSFX;

    private string usersPasscode = "";

    public override void Init(SceneControl _sceneControl)
    {
        base.Init(_sceneControl);
    }

    public override void onExit()
    {
        base.onExit();
        submitCodeButton.onClick.RemoveListener(SubmitButtonClickedEvent);
        guestButton.onClick.RemoveListener(GuestButtonClicked);
    }


    private void SubmitButtonClickedEvent()
    {
        // test if code is valid before sending
        SendUserCode();
    }

    public override void onEnter()
    {
        base.onEnter();
        submitCodeButton.onClick.RemoveListener(SubmitButtonClickedEvent);
        submitCodeButton.onClick.AddListener(SubmitButtonClickedEvent);
        guestButton.onClick.RemoveListener(GuestButtonClicked);
        guestButton.onClick.AddListener(GuestButtonClicked);
        // check if the participant data exists already
        Snobal.Library.Logger.Log($"SC.Participant?.participantId:{SC.Participant?.participantId}");
        if (SC.Participant?.participantId != null)
        {
            Snobal.Library.Logger.Log("Participant data found");
            feedbackText.text = $"User {SC.Participant.name} found";
            HideInputUserCodeField();
            sceneControl.ShowParticipantInfo();

            sceneControl.TriggerEvent(ExampleAppTriggers.Next);
            return;
        }

        Snobal.Library.Logger.Log("Showing user login");
        sceneControl.HideParticipantInfo();
        ShowInputUserCodeField();
    }

    private void GuestButtonClicked()
    {
        HideInputUserCodeField();
        sceneControl.TriggerEvent(ExampleAppTriggers.Next);
    }

    private void HideInputUserCodeField()
    {
        numericKeyboard.oskfocus = null;
        numericKeyboard.gameObject.SetActive(false);

        submitCodeButton.gameObject.SetActive(false);
        submitCodeButton.interactable = false;

        inputField.gameObject.SetActive(false);

        guestButton.interactable = false;
        guestButton.gameObject.SetActive(false);
    }

    private void ShowInputUserCodeField()
    {
        feedbackText.text = "Input User Passcode";
        numericKeyboard.oskfocus = inputField;
        numericKeyboard.gameObject.SetActive(true);

        submitCodeButton.gameObject.SetActive(true);
        submitCodeButton.interactable = false;

        inputField.gameObject.SetActive(true);

        guestButton.gameObject.SetActive(true);
        guestButton.interactable = true;
    }

    private void Update()
    {
        // Check if submit button should be enabled
        if (inputField.text.Length > 7)
        {
            submitCodeButton.interactable = true;
        }
        else
        {
            submitCodeButton.interactable = false;
        }
    }

    /// <summary>
    /// Send user passcode to server and await response
    /// </summary>
    private void SendUserCode()
    {
        keyboardSFX.PlayKeyPressSFX(0.0f);
        numericKeyboard.gameObject.SetActive(false);
        feedbackText.text = "Sending ...";

        submitCodeButton.gameObject.SetActive(false);
        usersPasscode = inputField.text;

        SnobalCloudMonobehaviour.TaskWatcher.Watch(
            System.Threading.Tasks.Task.Run
            (
                () =>
                {
                    // Seperate thread
                    SC.ValidatePasscode(usersPasscode);
                }
            ),
            () => { ValidateParticipantFinished(); }
        );
    }

    /// <summary>
    /// Evaluate the result of the validate task
    /// </summary>
    private void ValidateParticipantFinished()
    {
        StopRunningCoroutine();
        if (SC.Participant == null)
        {
            coroutine = PasscodeInvalid();
        }
        else
        {
            coroutine = ParticipantLoaded();
        }

        StartCoroutine(coroutine);
    }

    /// <summary>
    ///  Passcode is valid, give user feedback then progress to next state
    /// </summary>
    /// <returns></returns>
    private IEnumerator ParticipantLoaded()
    {
        Debug.Log("User Passcode valid. Participant name: " + SC.Participant.name.Colorize(Color.magenta));
        Debug.Log($"ParticipantId:{SC.Participant.participantId.Colorize(Color.magenta)}");

        sceneControl.ShowParticipantInfo();

        feedbackText.text = $"Passcode Valid.\nWelcome {SC.Participant.name}";
        yield return new WaitForSeconds(sceneControl.exampleAppSettings.endOfTaskDelay);

        keyboardSFX.PlayKeyPressSFX(1.0f);
        sceneControl.TriggerEvent(ExampleAppTriggers.Next);
    }

    /// <summary>
    /// Passcode is invalid, give the user another attempt after displaying failure.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PasscodeInvalid()
    {
        yield return new WaitForSeconds(0.25f);

        feedbackText.text = "User Passcode invalid.";

        yield return new WaitForSeconds(1.0f);

        ShowInputUserCodeField();
    }
}