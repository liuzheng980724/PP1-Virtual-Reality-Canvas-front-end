using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class FireExtinguisher : SceneObject
{
    // detect if something is in view of the extinghuiser for x period
    // give the extinghuisher a target, and a time it must look at it for

    [SerializeField]
    private XRGrabInteractable interactable;

    [SerializeField]
    private Rigidbody rigidBody;

    [SerializeField]
    private Transform rayCastSource;

    [SerializeField]
    private ParticleSystem sprayParticles;

    [Space(10), SerializeField]
    private InputActionProperty triggerInputActionWindows;

    [SerializeField]
    private InputActionProperty lefttriggerInputActionAndroid;

    [SerializeField]
    private InputActionProperty righttriggerInputActionAndroid;

    [SerializeField]
    private FireExtinguisherLock fireExtinguisherLock;

    [SerializeField]
    private ExpiryDateObject expiryDateObject;

    [SerializeField]
    private GameObject task3Object;

    [SerializeField]
    private AudioSource audioSource;

    private bool sprayEnabled = false;
    private float pressedValue = 0.0f;
    private Action callBackOnUnlock;
    private float timeSprayed = 0.0f;
    public bool isSpraying;
    private int fireLayerMask;

    private float sprayRadius = 0.25f;
    private float raycastDistance = 3.0f;
    private bool isGrabbed;
    private bool useExpiryLabel;

    public override void Setup()
    {
        base.Setup();
        rigidBody.isKinematic = true;
        LockFireExtinguisher();
        isSpraying = false;
        fireLayerMask = LayerMask.NameToLayer("Fire");
        // expiry date is only shown when the user picks up the fire extinguisher
        expiryDateObject.Hide();
        fireExtinguisherLock.Init();
        HideContentTask3();
        HideInstantly();
    }

    private void OnEnable()
    {
        RemoveListeners();
        AddListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    public void LockFireExtinguisher()
    {
        fireExtinguisherLock.ShowLocked();
        sprayEnabled = false;
    }

    public void UnlockFireExtinguisher()
    {
        fireExtinguisherLock.ShowUnLocked();
        sprayEnabled = true;
    }

    public override void Hide()
    {
        // animate away
        base.Hide();

        // when finished hideInstantly
        rigidBody.isKinematic = true;
        sprayEnabled = false;
        HideInstantly();
    }

    public override void HideInstantly()
    {
        sceneContent.SetActive(false);
        rigidBody.isKinematic = true;
        sprayEnabled = false;
        base.HideInstantly();
    }

    public override void Show()
    {
        base.Show();
        sceneContent.SetActive(true);
        rigidBody.isKinematic = false;
    }


    private void LateUpdate()
    {
        if (!isGrabbed || !sprayEnabled)
        {
            return;
        }

        if (IsButtonPressed())
        {
            Spray();

            // is sprayinh
            // constantly raycast, if there
            RayCastSpray();
        }
        else
        {
            StopSpray();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!isGrabbed || !sprayEnabled)
        {
            return;
        }

        Vector3 p1 = rayCastSource.position;
        // Start raycasting from behind the extinguisher to allow the user to put the nozzle really close
        p1 -= rayCastSource.forward * 0.25f;
        int totalSteps = Mathf.RoundToInt(raycastDistance / sprayRadius);
        var stepping = raycastDistance / totalSteps;
        var color = Color.green;
        color.a = 0.2f;
        Handles.color = color;
        for (int i = 0; i < totalSteps; ++i)
        {
            Handles.SphereHandleCap(0, p1, rayCastSource.rotation, sprayRadius, EventType.Repaint);
            p1 += rayCastSource.forward * stepping;
        }
    }
#endif
    private void RayCastSpray()
    {
        // every frame we cant to ray cast out and tell any fire we hit
        Vector3 p1 = rayCastSource.position;
        // Start raycasting from behind the source position as the user can move very close and lead
        // to the target and the raycast point can be inside the target
        p1 -= rayCastSource.forward * 0.25f;
        // move the raycast to behind the users head
        Debug.DrawRay(p1, rayCastSource.forward, Color.magenta, raycastDistance, true);

        fireLayerMask = ~fireLayerMask;

        var hits = Physics.SphereCastAll(p1, 0.01f, rayCastSource.forward, raycastDistance,
            fireLayerMask);
        foreach (RaycastHit hit in hits)
        {
            var binRef = hit.collider.gameObject.GetComponent<Bin>();
            if (binRef != null)
            {
//                Debug.Log(hit.collider.name);
                binRef.ApplySpray();
            }
        }
    }


    public override void ShowInstantly()
    {
        base.ShowInstantly();
        sceneContent.SetActive(true);
        rigidBody.isKinematic = false;
    }


    public void Spray()
    {
        if (!isSpraying)
        {
            audioSource.Play();
            isSpraying = true;
        }
        
        // if grip control is held down and holding this
        //sprayCollider.enabled = true;
        //sprayCollider.gameObject.SetActive(true);
        // todo quantity to emit could be based on amount button is pressed
        var amount = Mathf.RoundToInt(10 + 30 * pressedValue);
        sprayParticles.Emit(amount);
//        Debug.Log($"pressedValue:{pressedValue}");
        var sprayValue = Mathf.Clamp(pressedValue, 0.1f, 0.8f);
        audioSource.volume = sprayValue;
    }

    private void StopSpray()
    {
        isSpraying = false;
        sprayParticles.Stop();
        audioSource.Stop();
    }


    private bool IsButtonPressed()
    {
        // is it even held?

        pressedValue = 0.0f;
        var value = false;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                return triggerInputActionWindows.action.IsPressed();

            case RuntimePlatform.Android:
                // if the right or the left button is pressed
                pressedValue = triggerInputActionWindows.action.ReadValue<float>();
                Debug.Log(pressedValue);

                if (righttriggerInputActionAndroid.action.IsPressed())
                {
                    pressedValue = righttriggerInputActionAndroid.action.ReadValue<float>();
                    return true;
                }

                pressedValue = lefttriggerInputActionAndroid.action.ReadValue<float>();
                return lefttriggerInputActionAndroid.action.IsPressed();
        }

        return false;
    }

    public void CallBackOnUnLock(Action callbackAction)
    {
        callBackOnUnlock = callbackAction;
        fireExtinguisherLock.SetupLock(UnlockedExtinguisher);

        // now we need to wait for the collider on the head to become activated
    }

    private void UnlockedExtinguisher()
    {
        sprayEnabled = true;
        callBackOnUnlock.Invoke();
    }

    private void AddListeners()
    {
        interactable.selectEntered.AddListener(Grabbed);
        interactable.selectExited.AddListener(UnGrabbed);
    }

    private void RemoveListeners()
    {
        interactable.selectEntered.RemoveListener(Grabbed);
        interactable.selectExited.RemoveListener(UnGrabbed);
    }

    private void Grabbed(SelectEnterEventArgs selectEnterEventArgs)
    {
        isGrabbed = true;
        if (useExpiryLabel)
        {
            expiryDateObject.Show();
        }
    }

    private void UnGrabbed(SelectExitEventArgs selectExitEventArgs)
    {
        isGrabbed = false;
        isSpraying = false;
        expiryDateObject.Hide();
    }


    public void EnableExpiryLabelLogic()
    {
        useExpiryLabel = true;
    }

    public void DisableExpiryLabelLogic()
    {
        useExpiryLabel = false;
    }

    public void ShowContentTask3()
    {
        task3Object.SetActive(true);
    }

    public void HideContentTask3()
    {
        task3Object.SetActive(false);
    }
}