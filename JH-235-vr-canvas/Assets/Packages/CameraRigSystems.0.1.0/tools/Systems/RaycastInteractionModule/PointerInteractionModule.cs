using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Snobal.CameraRigSystems_0_0
{
    public enum PointerSource
    {
        Left,
        Right,
        Gaze // Gaze is not yet fully implemented yet
    }

    public abstract class PointerInteractionModule : MonoBehaviour
    {
        [SerializeField]
        private bool InitiallyActive = true;

        [SerializeField]
        private float castDistance = 15f;

        private Transform pointerLeft;
        private Transform pointerRight;
        public XRRayInteractor rayInteractorLeftHand;
        public XRRayInteractor rayInteractorRightHand;

        protected virtual void Awake()
        {
            ActivateModule(InitiallyActive);
        }

        protected virtual void Start()
        {
            pointerLeft = CameraRig.RigTransforms.PointerLeft;
            pointerRight = CameraRig.RigTransforms.PointerRight;
            rayInteractorLeftHand = pointerLeft.GetComponent<XRRayInteractor>();
            rayInteractorRightHand = pointerRight.GetComponent<XRRayInteractor>();
        }

        protected virtual void Update()
        {
            if (pointerLeft == null)
                return;
            if (pointerRight == null)
                return;

            UpdatePointers(PointerSource.Left, pointerLeft, rayInteractorLeftHand);
            UpdatePointers(PointerSource.Right, pointerRight, rayInteractorRightHand);
        }

        protected virtual void UpdatePointers(PointerSource hand, Transform source, XRRayInteractor rayInteractor)
        {
            if (rayInteractor == null)
                return;
            if (!rayInteractor.TryGetCurrentRaycast(out RaycastHit? hit, out int raycastHitIndex,
                out UnityEngine.EventSystems.RaycastResult? uiRaycastHit, out int uiRaycastHitIndex,
                out bool isUIHitCloset))
            {
                return;
            }

            Process(hand, source, true, hit);
        }

        public virtual void ActivateModule(bool enable)
        {
            this.enabled = enable;
        }

        public virtual void ChangePointerToCurved(int pointerType) //pointerType = 1 (Right), 0 (Left)
        {
            if (pointerType == 1)
                rayInteractorRightHand.lineType = XRRayInteractor.LineType.ProjectileCurve;
            else
                rayInteractorLeftHand.lineType = XRRayInteractor.LineType.ProjectileCurve;
        }

        public virtual void ChangePointerToStraightLine(int pointerType) //pointerType = 1 (Right), 0 (Left)
        {
            if (pointerType == 1)
                rayInteractorRightHand.lineType = XRRayInteractor.LineType.StraightLine;
            else
                rayInteractorLeftHand.lineType = XRRayInteractor.LineType.StraightLine;
        }

        protected abstract void Process(PointerSource hand, Transform pointerSource, bool hit, RaycastHit? hitInfo);
    }
}