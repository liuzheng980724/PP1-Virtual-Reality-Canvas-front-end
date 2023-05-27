using System;
using System.Collections.Generic;
using Snobal.CameraRigSystems_0_0;
using Snobal.CloudSDK;
using Snobal.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class StateEndSession : StateBase
{
    [SerializeField]
    private Button backToSnobalVRButton;

    protected override void Show()
    {
        base.Show();
        backToSnobalVRButton.gameObject.SetActive(true);

        backToSnobalVRButton.onClick.RemoveListener(OpenSnobalVR);
        backToSnobalVRButton.onClick.AddListener(OpenSnobalVR);
    }

    protected override void Hide()
    {
        base.Hide();

        backToSnobalVRButton.onClick.RemoveListener(OpenSnobalVR);
    }

    private void OpenSnobalVR()
    {
        // this will take the user back to the main app
        // might want to put a confirm dialog in here
        FadeAndLoadApp();
    }

    private void FadeAndLoadApp()
    {
        CameraFadeOpaque(() =>
        {
            backToSnobalVRButton.gameObject.SetActive(false);
            SceneControl.QuitApplication();

#if UNITY_EDITOR
            // Not on android? carry on
            CameraFadeClear(() => { });
#endif
        });
    }

    void CameraFadeOpaque(Action callback) => CameraRig.FadeTransitionControl.StartFadeOpaque(callback);
    void CameraFadeClear(Action callback) => CameraRig.FadeTransitionControl.StartFadeClear(callback);

   
}