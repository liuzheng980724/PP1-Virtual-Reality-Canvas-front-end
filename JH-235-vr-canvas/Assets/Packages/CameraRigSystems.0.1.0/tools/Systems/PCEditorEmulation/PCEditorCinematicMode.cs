using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Snobal.CameraRigSystems_0_0
{
    public class PCEditorCinematicMode : MonoBehaviour
    {
        [SerializeField]
        bool initiallyActive = false;
        [SerializeField]
        private InputActionProperty toggleCinematicModeInput;

        bool modeActive = false;
        private Vector3 origPosLeft;
        private Vector3 origPosRight;
        private Vector3 origScaleLeft;
        private Vector3 origScaleRight;

        void Start()
        {
            if (initiallyActive)
                ActivateCinematicMode(true);
        }

        void Update()
        {
            if (toggleCinematicModeInput.action.triggered)
            {
                Debug.Log($"Editor cinematic mode toggled {((!modeActive) ? "on" : "off")}");
                ActivateCinematicMode(!modeActive);
            }
        }

        void ActivateCinematicMode(bool value)
        {
            if (value == true)
            {
                origPosLeft = CameraRig.RigTransforms.ControllerLeft.localPosition;
                origPosRight = CameraRig.RigTransforms.ControllerRight.localPosition;
                origScaleLeft = CameraRig.RigTransforms.ControllerLeft.localScale;
                origScaleRight = CameraRig.RigTransforms.ControllerRight.localScale;

                HideController(CameraRig.RigTransforms.ControllerLeft);
                HideController(CameraRig.RigTransforms.ControllerRight);
            }
            else
            {
                ShowController();
            }

            modeActive = value;
        }

        void HideController(Transform controller)
        {
            controller.localPosition = Vector3.up * 100;
            controller.localPosition = Vector3.up * 100;
            controller.localScale = Vector3.zero;
            controller.localScale = Vector3.zero;
        }

        void ShowController()
        {
            CameraRig.RigTransforms.ControllerLeft.localPosition = origPosLeft;
            CameraRig.RigTransforms.ControllerLeft.localScale = origScaleLeft;

            CameraRig.RigTransforms.ControllerRight.localPosition = origPosRight;
            CameraRig.RigTransforms.ControllerRight.localScale = origScaleRight;
        }
    }
}