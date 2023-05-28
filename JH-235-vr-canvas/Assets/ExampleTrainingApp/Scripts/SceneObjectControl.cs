using System;
using System.Collections;
using UnityEngine;

public class SceneObjectControl : MonoBehaviour
{
    [SerializeField]
    private FireExtinguisher fireExtinguisher;

    [SerializeField]
    private Bin bin;

    [SerializeField]
    private Table table;

    [SerializeField]
    private CanvasLoginForm canvasLoginForm;

    [SerializeField]
    private SceneTaskHelper sceneTaskHelper1;

    [SerializeField]
    private SceneTaskHelper sceneTaskHelper2;

    [SerializeField]
    private SceneTaskHelper sceneTaskHelper3;

    [SerializeField]
    private SceneTaskHelper sceneTaskHelper4;

    [SerializeField]
    private SceneTaskHelper sceneTaskHelper5;

    [SerializeField]
    private PercentComplete testSprayPercent;

    private IEnumerator coroutine;

    public void Setup()
    {
        table.Setup();
        fireExtinguisher.Setup();
        bin.Setup();
        canvasLoginForm.Setup();

        table.ShowInstantly();
        testSprayPercent.HideInstantly();
        testSprayPercent.DisplayPercent(0.0f);
        sceneTaskHelper1.Setup();
        sceneTaskHelper2.Setup();
        sceneTaskHelper3.Setup();
        sceneTaskHelper4.Setup();
        sceneTaskHelper5.Setup();
        canvasLoginForm.Hide();    //Hide Login Form before Login Snobal. --ZHENG LIU
        //canvasLoginForm.DisplayPercent(0.0f);

    }

    public void ShowTask1Objects()
    {
        // find the extinguisher
        table.Show();
        //fireExtinguisher.Show();
        //fireExtinguisher.EnableExpiryLabelLogic();
        //sceneTaskHelper1.Show();
        canvasLoginForm.Show(); //Show Login Form --ZHENG LIU

    }

    public void ShowTask2Objects(Action callbackAction)
    {
        // todo the hide function should probably go somewhere else
        sceneTaskHelper1.Hide();
        sceneTaskHelper2.Show();
        fireExtinguisher.DisableExpiryLabelLogic();
        // activate the extinguisher, breaking the seal
        fireExtinguisher.CallBackOnUnLock(callbackAction);
    }

    private void StopRunningCoroutine()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    public void ShowTask3Objects(Action callbackAction)
    {
        sceneTaskHelper2.Hide();
        sceneTaskHelper3.Show();
        fireExtinguisher.ShowContentTask3();
        // wait until fireExtinguisher.IsSpraying has gone over 2 seconds

        StopRunningCoroutine();
        coroutine = Task3Sequence(callbackAction);
        StartCoroutine(coroutine);
    }

    private IEnumerator Task3Sequence(Action callbackAction)
    {
        fireExtinguisher.UnlockFireExtinguisher();
        testSprayPercent.Show();

        // todo this logic might need to go somewhere else
        var sprayTimePassed = 0.0f;
        var totalSprayTIme = 2.0f;
        while (sprayTimePassed < totalSprayTIme)
        {
            if (fireExtinguisher.isSpraying)
            {
                sprayTimePassed += Time.deltaTime;
            }

            var passedPercent = sprayTimePassed / totalSprayTIme;
            testSprayPercent.DisplayPercent(passedPercent);
            yield return null;
        }

        fireExtinguisher.HideContentTask3();

        testSprayPercent.Hide();

        sceneTaskHelper3.Hide();
        callbackAction.Invoke();
    }

    public void ShowTask4Objects(Action callbackAction, float timeToPutOutFire)
    {
        // put the fire in the bin out
        bin.Show();
        sceneTaskHelper4.Show();
        fireExtinguisher.ShowInstantly();
        bin.StartMonitoringSpray(callbackAction, timeToPutOutFire);
        bin.StartFire();
    }

    public void ShowTask5Objects()
    {
        sceneTaskHelper4.Hide();
        sceneTaskHelper5.Show();
        bin.Hide();
        fireExtinguisher.Hide();
        table.Hide();
    }
}