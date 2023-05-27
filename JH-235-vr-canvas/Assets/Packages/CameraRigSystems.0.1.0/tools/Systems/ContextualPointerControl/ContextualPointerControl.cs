using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Snobal.CameraRigSystems_0_0
{
    public class ContextualPointerControl : MonoBehaviour
    {
        public bool IsActive { get; private set; }

        [SerializeField]
        private bool InitiallyActivated = true;              

        public void Awake()
        {
            ActivatePointers(InitiallyActivated);
        }

        public void ActivatePointers(bool enable)
        {
            foreach (var pointer in this.GetComponentsInChildren<XRRayInteractor>())
                pointer.gameObject.SetActive(enable);

            IsActive = enable;
        }
    }
}