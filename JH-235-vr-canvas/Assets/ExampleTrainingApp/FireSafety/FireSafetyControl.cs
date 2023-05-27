using System;
using Snobal.EnumFsmNET_0_1;
using UnityEngine;

public class FireSafetyControl : MonoBehaviour
{
    private Action schemeFinishedCallback;
    
    private EnumFsm<FireSafetyStates, FireSafetyStateTriggers> fsm;
    public void Setup()
    {
        // get ready to run through all tasks
        // setup state machine
        // setup all states
        fsm = new EnumFsm<FireSafetyStates, FireSafetyStateTriggers>(FireSafetyStates.Setup);
        
       /* AddState(FireSafetyStates.Setup, setup);
        AddState(FireSafetyStates.Pair, editorPair);
        AddState(FireSafetyStates.Login, editorLogin);
        AddState(FireSafetyStates.ValidateUser, validateUser);
        AddState(FireSafetyStates.BuildDatabase, buildDatabase);
        */
    }
    private void AddState(FireSafetyStates appState, FireSafetyStateBase classRef)
    {
        classRef.Init(this);
        fsm.AddState(appState, classRef.onEnter, classRef.onExit);
    }
    public void StartScheme(Action schemeFinished)
    {
        schemeFinishedCallback = schemeFinished;
        // when the all tasks within the scheme are finished, invoke the call back
        
    }
}

public enum FireSafetyStates
{
    Setup,
    Intro,
    Start,
    Task1,
    Task2,
    Task3,
    Task4,
    Task5,
    Finished,
    Exit
}
public enum FireSafetyStateTriggers
{
    Next,
    Failed
}