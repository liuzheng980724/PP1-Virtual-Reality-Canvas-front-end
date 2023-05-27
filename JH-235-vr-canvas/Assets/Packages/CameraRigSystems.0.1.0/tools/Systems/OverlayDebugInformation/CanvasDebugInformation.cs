using Snobal.CameraRigSystems_0_0;
using Snobal.ConstraintUtilitiesUnity_0_0;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CanvasDebugInformation : MonoBehaviour
{
    [SerializeField]
    bool initiallyActive = false;
    [SerializeField]
    Transform root;
    [SerializeField]
    private float canvasOffsetZ = 1.5f;

    private bool IsActive = false;

    [SerializeField]
    private InputActionProperty toggleWindowsDebugMenuInputAction;
    [SerializeField]
    private InputActionProperty toggleAndroidDebugMenuInputAction;

    private void Awake()
    {
        Debug.Assert(root != null);

        DontDestroyOnLoad(this.gameObject);
    }

    IEnumerator Start()
    {
        Activate(initiallyActive);

        yield return new WaitWhile(() => (CameraRig.Instance == null));

        ConstraintUtilities.ApplyParentConstraint(CameraRig.RigTransforms.Head, this.transform, Vector3.forward * canvasOffsetZ, Vector3.zero);

        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                toggleWindowsDebugMenuInputAction.action.performed += (context) =>
                {
                    Activate(!IsActive);
                };
                break;
            case RuntimePlatform.Android:
                toggleAndroidDebugMenuInputAction.action.performed += (context) => {
                    Activate(!IsActive);
                };
                break;
        }

    }

    void Activate(bool value)
    {
        root?.gameObject.SetActive(value);        
        IsActive = value;
    }
}
