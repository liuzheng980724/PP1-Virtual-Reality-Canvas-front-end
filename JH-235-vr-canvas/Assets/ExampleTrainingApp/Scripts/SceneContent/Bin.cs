using System;
using UnityEngine;

public class Bin : SceneObject
{
    [SerializeField]
    private ParticleSystem fireParticles;

    [SerializeField]
    private PercentComplete percentComplete;

    [SerializeField]
    private Collider fireCollisionCollider;

    private float sprayTimePassed = 0.0f;
    private float timeToPutOutFire = 2.0f;
    private int TotalFlamesAmount = 15;
    private Action fireExtinguishedCallback;

    public override void Setup()
    {
        base.Setup();
        StopFireInstantly();
        percentComplete.HideInstantly();
        HideInstantly();
    }


    public void StartFire()
    {
        sprayTimePassed = 0.0f;
        percentComplete.Show();
        percentComplete.DisplayPercent(0.0f);
        fireParticles.Play();
        fireCollisionCollider.enabled = true;
    }

    public void StopFire()
    {
        fireParticles.Stop();
        fireCollisionCollider.enabled = false;
    }

    private void StopFireInstantly()
    {
        fireParticles.Stop();
        fireParticles.Clear();
        fireCollisionCollider.enabled = false;
    }


    public void StartMonitoringSpray(Action finishedCallBack, float timeToTakeToPutOutFire)
    {
        fireExtinguishedCallback = finishedCallBack;
        timeToPutOutFire = timeToTakeToPutOutFire;
    }

    private void FireHasBeenExtinguished()
    {
        StopFire();
        // task has been finished
        percentComplete.Hide();
        fireExtinguishedCallback.Invoke();
        StopMonitoringSpray();
    }

    public void StopMonitoringSpray()
    {
        fireExtinguishedCallback = null;
    }

    public void ApplySpray()
    {
        sprayTimePassed += Time.deltaTime;
        sprayTimePassed = Mathf.Clamp(sprayTimePassed, 0.0f, timeToPutOutFire);
        var percent = sprayTimePassed / timeToPutOutFire;
        percentComplete.DisplayPercent(percent);
        //Debug.Log($"sprayTimePassed:{sprayTimePassed} percent:{percent}");
        // may want to reduce the intensity of the fire as exposure time progresses
        var fireParticlesEmission = fireParticles.emission;
        fireParticlesEmission.rateOverTime = TotalFlamesAmount * (1.0f - (percent * 0.9f));
        //Debug.Log("fireParticlesEmission.rateOverTime:"+fireParticlesEmission.rateOverTime);
        if (sprayTimePassed >= timeToPutOutFire)
        {
            FireHasBeenExtinguished();
        }
    }

    public override void Show()
    {
        base.Show();

        sceneContent.SetActive(true);
        sceneContent.transform.localScale = Vector3.one * 0.1f;

        LeanTween.cancel(sceneContent);
        LeanTween.scale(sceneContent, Vector3.one, 1.0f)
            .setEase(LeanTweenType.easeOutBack);
    }

    public override void Hide()
    {
        base.Hide();
        percentComplete.Hide();

        LeanTween.cancel(sceneContent);
        LeanTween.scale(sceneContent, Vector3.one, 1.0f)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(HideInstantly);
        StopFire();
    }

    public override void HideInstantly()
    {
        base.HideInstantly();
        LeanTween.cancel(sceneContent);
    }
}