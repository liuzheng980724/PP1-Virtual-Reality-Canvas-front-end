using UnityEngine;
using Snobal.DesignPatternsUnity_0_0;
using Snobal.DesignPatternsUnity_0_0.Extensions;

namespace Snobal.CameraRigSystems_0_0
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RigPositionControl))]
    [RequireComponent(typeof(ContextualPointerControl))]
    [RequireComponent(typeof(FadeTransitionControl))]
    [RequireComponent(typeof(FreeRaycastTeleport))]
    public class CameraRig : SingletonMonoBehaviour<CameraRig>
    {
        [Header("Rig Transform References")]
        [SerializeField]
        private RigTransformReference rigTransforms;

        [Header("Required Controls")]
        [SerializeField]
        private RigPositionControl rigPositionControl;
        [SerializeField]
        private ContextualPointerControl contextualPointerControl;
        [SerializeField]
        private FadeTransitionControl fadeTransitionControl;        
        [SerializeField]
        private FreeRaycastTeleport freeRaycastTeleport;

        /// <summary>
        /// This class will allow you to access head, hand and pointer transforms for the current rig
        /// </summary>
        public static RigTransformReference RigTransforms { get => Instance.rigTransforms; }

        /// <summary>
        /// This class will allow you to toggle on and off the UI lazer pointers emanating from the users hands.
        /// </summary>
        public static ContextualPointerControl ContextualPointerControl { get => Instance.contextualPointerControl; }
        
        /// <summary>
        /// This class will allow you to manage the fade to and from black used for scene transitions.
        /// </summary>
        public static FadeTransitionControl FadeTransitionControl { get => Instance.fadeTransitionControl; }
        
        /// <summary>
        /// This class will allow you to control the position of the rig in scene.
        /// </summary>
        public static RigPositionControl RigPositionControl { get => Instance.rigPositionControl; }        

        /// <summary>
        /// this class will allow you to control free raycast teleporting
        /// </summary>
        public static FreeRaycastTeleport FreeRaycastTeleport { get => Instance.freeRaycastTeleport; }

        private void OnValidate()
        {
            SetCachedReference(ref contextualPointerControl);
            SetCachedReference(ref fadeTransitionControl);
            SetCachedReference(ref rigPositionControl);
            SetCachedReference(ref freeRaycastTeleport);
        }

        private void SetCachedReference<T>(ref T cachedComponent) where T : MonoBehaviour {
            // to stop this from being run everytime OnValidate runs
            if (cachedComponent == null)
                cachedComponent = this.GetComponent<T>();
        }

        [ContextMenu("Fetch Rig Transforms by Standard Names")]
        private void FetchRigTransformsByStandardNames()
        {
            var trans1 = TransformExtensions.FindDeepChildDepthFirst(this.transform, "Main Camera");
            var trans2 = TransformExtensions.FindDeepChildDepthFirst(this.transform, "LeftHand Controller");
            var trans3 = TransformExtensions.FindDeepChildDepthFirst(this.transform, "RightHand Controller");
            var trans4 = TransformExtensions.FindDeepChildDepthFirst(this.transform, "LeftHand Pointer");
            var trans5 = TransformExtensions.FindDeepChildDepthFirst(this.transform, "RightHand Pointer");

            rigTransforms = new RigTransformReference(trans1, trans2, trans3, trans4, trans5);
        }
    }

    [System.Serializable]
    public class RigTransformReference
    {
        public RigTransformReference (Transform _head, Transform _controllerLeft, Transform _controllerRight, Transform _PointerLeft, Transform _PointerRight)
        {
            Head = _head;
            ControllerLeft = _controllerLeft;
            ControllerRight = _controllerRight;
            PointerLeft = _PointerLeft;
            PointerRight = _PointerRight;
        }

        public Transform Head { 
            get => head ?? throw new UnityException("Missing reference in for head transfrom"); 
            private set => head = value; 
        }

        public Transform ControllerLeft { 

            get => handLeft ?? throw new UnityException("Missing reference in for handLeft transfrom"); 
            private set => handLeft = value; 
        }

        public Transform ControllerRight { 

            get => handRight ?? throw new UnityException("Missing reference in for handRight transfrom"); 
            private set => handRight = value; 
        }

        public Transform PointerLeft { 

            get => pointerLeft ?? throw new UnityException("Missing reference in for pointerLeft transfrom"); 
            private set => pointerLeft = value; 
        }

        public Transform PointerRight { 

            get => pointerRight ?? throw new UnityException("Missing reference in for pointerRight transfrom"); 
            private set => pointerRight = value; 
        }

        [SerializeField]
        private Transform head;
        [SerializeField]
        private Transform handLeft;
        [SerializeField]
        private Transform handRight;
        [SerializeField]
        private Transform pointerLeft;
        [SerializeField]
        private Transform pointerRight;
    }
}