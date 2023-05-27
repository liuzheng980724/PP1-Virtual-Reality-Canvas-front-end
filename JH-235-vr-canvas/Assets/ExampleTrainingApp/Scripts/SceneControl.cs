using System.Collections.Generic;
using Snobal.CloudSDK;
using Snobal.EnumFsmNET_0_1;
using Snobal.Library.DataStructures;
using Snobal.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class SceneControl : MonoBehaviour
{
    [SerializeField]
    public ExampleAppSettings exampleAppSettings;

    [SerializeField]
    private DisplayParticipantInfo displayParticipantInfo;

    [Space(10), SerializeField]
    private Button quitButton;

    [Space(10), SerializeField]
    public SceneObjectControl sceneObjectControl;

    [Space(10), SerializeField]
    public Transform taskholder;

    [Space(20), SerializeField]
    private ResultDisplay resultDisplay;

    [Space(10), Header("Scene states"), SerializeField]
    private StateSetup setup;

    [SerializeField]
    private StateEditorPair editorPair;

    [SerializeField]
    private StateEditorLogin editorLogin;

    [SerializeField]
    private StateValidateUser validateUser;

    [SerializeField]
    private StateOpenScheme openScheme;

    [SerializeField]
    private StateTask1 task1;

    [SerializeField]
    private StateTask2 task2;

    [SerializeField]
    private StateTask3 task3;

    [SerializeField]
    private StateTask4 task4;

    [SerializeField]
    private StateTask5 task5;

    [SerializeField]
    private StateCloseScheme closeScheme;

    [SerializeField]
    private StateEndSession endSession;


    [Space(20)]
    // todo move currentSchemeID, currentTaskOutcome & currentTaskID to CurrentUserData
    public string currentSchemeID;

    public string currentTaskID;
    public bool currentTaskOutcome = false;

    public SchemeData currentSchemeData;
    private EnumFsm<ExampleAppStates, ExampleAppTriggers> fsm;

    public const string SnobalPackage = "net.snobal.spheredemo";

    private void Start()
    {
        SetupUI();
        HideParticipantInfo();
        SetupStateMachine();
        sceneObjectControl.Setup();
        fsm.Start();
    }

    private void SetupUI()
    {
        // Setup quit button
        quitButton.onClick.AddListener(UserQuitApplication);
        resultDisplay.HideInstantly();
    }

    private void UserQuitApplication()
    {
        Debug.Log("Application closed by user");
        if (Snobal.Library.Logger.isInitialised)
        {
            Snobal.Library.Logger.Log("Application closed by user");
        }

        QuitApplication();
    }

    public static void QuitApplication()
    {
        Application.Quit();

        // Pass android intents when quitting, and reopen Snobal VR 
        AndroidUtils.LaunchAndroidApp(SnobalPackage, CreateExternalAppArguments());
    }

    private void SetupStateMachine()
    {
        fsm = new EnumFsm<ExampleAppStates, ExampleAppTriggers>(ExampleAppStates.Setup);

        AddState(ExampleAppStates.Setup, setup);
        AddState(ExampleAppStates.Pair, editorPair);
        AddState(ExampleAppStates.Login, editorLogin);
        AddState(ExampleAppStates.ValidateUser, validateUser);
        AddState(ExampleAppStates.OpenScheme, openScheme);

        AddState(ExampleAppStates.Task1, task1);
        AddState(ExampleAppStates.Task2, task2);
        AddState(ExampleAppStates.Task3, task3);
        AddState(ExampleAppStates.Task4, task4);
        AddState(ExampleAppStates.Task5, task5);

        AddState(ExampleAppStates.CloseScheme, closeScheme);
        AddState(ExampleAppStates.EndSession, endSession);

        // Transitions
        // Setup
        fsm.AddTransition(ExampleAppStates.Setup, ExampleAppStates.Pair, ExampleAppTriggers.Failed);
        fsm.AddTransition(ExampleAppStates.Setup, ExampleAppStates.Login, ExampleAppTriggers.Next);
        // Pair
        fsm.AddTransition(ExampleAppStates.Pair, ExampleAppStates.Login, ExampleAppTriggers.Next);
        // Login
        fsm.AddTransition(ExampleAppStates.Login, ExampleAppStates.ValidateUser, ExampleAppTriggers.Next);
        // ValidateUser
        fsm.AddTransition(ExampleAppStates.ValidateUser, ExampleAppStates.OpenScheme, ExampleAppTriggers.Next);
        fsm.AddTransition(ExampleAppStates.ValidateUser, ExampleAppStates.ValidateUserFailed,
            ExampleAppTriggers.Failed);

        fsm.AddTransition(ExampleAppStates.OpenScheme, ExampleAppStates.Task1, ExampleAppTriggers.Next);
        //fsm.AddTransition(ExampleAppStates.OpenScheme, ExampleAppStates.Task1, ExampleAppTriggers.Failed);

        fsm.AddTransition(ExampleAppStates.Task1, ExampleAppStates.Task2, ExampleAppTriggers.Next);
        fsm.AddTransition(ExampleAppStates.Task2, ExampleAppStates.Task3, ExampleAppTriggers.Next);
        fsm.AddTransition(ExampleAppStates.Task3, ExampleAppStates.Task4, ExampleAppTriggers.Next);
        fsm.AddTransition(ExampleAppStates.Task4, ExampleAppStates.Task5, ExampleAppTriggers.Next);
        fsm.AddTransition(ExampleAppStates.Task5, ExampleAppStates.CloseScheme, ExampleAppTriggers.Next);


        fsm.AddTransition(ExampleAppStates.CloseScheme, ExampleAppStates.EndSession,
            ExampleAppTriggers.Next);
    }

    public void ShowParticipantInfo()
    {
        var participant = SnobalCloudMonobehaviour.SnobalCloudInstance.Participant;
        displayParticipantInfo.Show(participant);
    }

    public void HideParticipantInfo()
    {
        displayParticipantInfo.Hide();
    }

    private void AddState(ExampleAppStates appState, StateBase classRef)
    {
        classRef.Init(this);
        fsm.AddState(appState, classRef.onEnter, classRef.onExit);
    }

    /// <summary>
    /// Trigger state machine event
    /// </summary>
    /// <param name="trigger"></param>
    public void TriggerEvent(ExampleAppTriggers trigger)
    {
        fsm.TriggerEvent(trigger);
    }

    public void ShowTaskResult(bool correct)
    {
        resultDisplay.Show(correct);
    }

    public void HideTaskResult(bool instantly = false)
    {
        if (instantly)
        {
            resultDisplay.HideInstantly();
            return;
        }

        resultDisplay.Hide();
    }

    /// <summary>
    /// Create any arguments to pass to the app we are switching to.
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string> CreateExternalAppArguments()
    {
        var SC = SnobalCloudMonobehaviour.SnobalCloudInstance;
        var appArguments = new Dictionary<string, string>();

        var photonRoomCode = AndroidExternalParameters.GetParameter(ExternalParameterType.PhotonRoomCode);
        appArguments.Add(ExternalParameterType.PhotonRoomCode.ToString(), photonRoomCode);

        var roomRegion = AndroidExternalParameters.GetParameter(ExternalParameterType.RoomRegion);
        appArguments.Add(ExternalParameterType.RoomRegion.ToString(), roomRegion);

        var tenantURL = AndroidExternalParameters.GetParameter(ExternalParameterType.TenantURL);
        appArguments.Add(ExternalParameterType.TenantURL.ToString(), tenantURL);

        var avatarID = AndroidExternalParameters.GetParameter(ExternalParameterType.AvatarID);
        appArguments.Add(ExternalParameterType.AvatarID.ToString(), avatarID);

        var userName = AndroidExternalParameters.GetParameter(ExternalParameterType.UserName);
        // Username set to participants name if there is none supplied
        if (string.IsNullOrEmpty(userName))
        {
            userName = SC.Participant?.name;
        }

        appArguments.Add(ExternalParameterType.UserName.ToString(), userName);

        var returnToRoomName = AndroidExternalParameters.GetParameter(ExternalParameterType.ReturnToRoomName);
        appArguments.Add(ExternalParameterType.ReturnToRoomName.ToString(), returnToRoomName);

        // adding settings file
        var settings = SnobalCloudMonobehaviour.SnobalCloudInstance.Settings;
        var settingsJson = Serialization.SerializeObject(settings.Values);
        appArguments.Add(ExternalParameterType.Settings.ToString(), settingsJson);

        // Intentional logging for demonstration purposes.
        Snobal.Library.Logger.Log($"SC.HasLoginBeenGranted:{SC.HasLoginBeenGranted}");
        Snobal.Library.Logger.Log($"photonRoomCode:{photonRoomCode}");
        Snobal.Library.Logger.Log($"roomRegion:{roomRegion}");
        Snobal.Library.Logger.Log($"tenantURL:{tenantURL}");
        Snobal.Library.Logger.Log($"avatarID:{avatarID}");
        Snobal.Library.Logger.Log($"SC.HasLoginBeenGranted:{SC.HasLoginBeenGranted}");
        
        if (SC.HasLoginBeenGranted)
        {
            Snobal.Library.Logger.Log($"SC.Participant?.participantId:{SC.Participant?.participantId}");
            if (SC.Participant?.participantId != null)
            {
                var participantData = Serialization.SerializeObject(SC.Participant);
                appArguments.Add(ExternalParameterType.Participant.ToString(), participantData);
            }
        }

        return appArguments;
    }


#if UNITY_EDITOR
    /// <summary>
    /// Log out of Snobal Cloud on exit, only used in Editor while testing
    /// </summary>
    private void OnDisable()
    {
        var SC = Snobal.CloudSDK.SnobalCloudMonobehaviour.SnobalCloudInstance;
        SC.Logout();
    }
#endif
}