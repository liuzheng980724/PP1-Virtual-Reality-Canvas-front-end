using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Snobal.DesignPatternsUnity_0_0.Extensions;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

namespace Snobal.CameraRigSystems_0_0
{
    public class FreeRaycastTeleport : PointerInteractionModule
    { 
        enum ThumstickState
        {
            NONE,
            UP,
            DOWN
        }
        
        [SerializeField]
        TeleportationProvider teleportationProvider;
        [SerializeField]
        private float pointSearchRange = 0.1f;
        [SerializeField]
        private LayerMask teleportLayers;
        [SerializeField]
        private string TeleportIndicatorPrefabResourceName = "TeleportIndicatorPrefab";
        [SerializeField]
        private string AudioTeleportSoundPrefabResourceName = "AudioTeleportSoundPrefab";
                
        [SerializeField]
        private InputActionProperty TeleportInputLeftAction;
        [SerializeField]
        private InputActionProperty TeleportInputRightAction;
        
        [FormerlySerializedAs("thumstickInputRightAction")]
        [SerializeField]
        private InputActionProperty thumbstickInputRightAction;
        [FormerlySerializedAs("thumstickInputLeftAction")]
        [SerializeField]
        private InputActionProperty thumbstickInputLeftAction;

        private ThumstickState leftThumbStickState = ThumstickState.NONE;
        private  ThumstickState rightThumbStickState = ThumstickState.NONE;
        
        private GameObject indicatorLeft;
        private GameObject indicatorRight;
        private AudioSource teleportAudioSource;
        private bool isTeleporting = false;
        protected override void Start()
        {
            base.Start();
            switch (Application.platform)
            {
                //actionType =  1 -> Pressed, 0-> left
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    thumbstickInputRightAction.action.started += (context) =>
                    {
                        rightThumbStickState = ThumstickState.UP;
                        ChangePointerToCurved(1);
                    };
                    thumbstickInputLeftAction.action.started += (context) =>
                    {
                        leftThumbStickState = ThumstickState.UP;
                        ChangePointerToCurved(0);
                    };
                    break;
            }
        }
        private void OnValidate()
        {
            if (teleportationProvider == null)
                teleportationProvider = this.GetComponentInChildren<TeleportationProvider>();
        }

        protected override void Awake()
        {
            base.Awake();

            InitTeleportIndicators();

            InitTeleportAudio();

            Debug.Assert(teleportationProvider != null);
            Debug.Assert(teleportAudioSource != null);
            Debug.Assert(indicatorRight != null);
            Debug.Assert(indicatorLeft != null);
        }
      
        
        private void LateUpdate()
        {
            isTeleporting = false;
        }

        void InitTeleportIndicators()
        {
            var indicatorPrefab = Resources.Load<GameObject>(TeleportIndicatorPrefabResourceName);

            if (indicatorPrefab == null)
                throw new UnityException($"Cannot find {TeleportIndicatorPrefabResourceName} gameobject resource");

            indicatorLeft = GameObject.Instantiate(indicatorPrefab, this.transform);
            indicatorLeft.name = "TELEPORT INDICATOR LEFT";
            indicatorLeft.SetActive(false);

            indicatorRight = GameObject.Instantiate(indicatorPrefab, this.transform);
            indicatorRight.name = "TELEPORT INDICATOR RIGHT";
            indicatorRight.SetActive(false);
        }

        void InitTeleportAudio()
        {
            var audioSourcePrefab = Resources.Load<GameObject>(AudioTeleportSoundPrefabResourceName);

            if (audioSourcePrefab == null)
                throw new UnityException($"Cannot find {AudioTeleportSoundPrefabResourceName} gameobject resource");

            var temp = GameObject.Instantiate(audioSourcePrefab, this.transform);
            temp.name = "TELEPORT AUDIO SOURCE";

            teleportAudioSource = temp.GetComponent<AudioSource>();

            if (teleportAudioSource == null)
                throw new UnityException("audioSourcePrefab does not contain AudioSource component");
        }


        protected override void UpdatePointers(PointerSource hand, Transform source, XRRayInteractor rayInteractor)
        {
            if (!isTeleporting)
                base.UpdatePointers(hand, source, rayInteractor);
            
            if (Application.platform == RuntimePlatform.Android)
            {
                if (thumbstickInputRightAction.action.IsPressed())
                {
                    Vector2 y_axis = thumbstickInputRightAction.action.ReadValue<Vector2>();
                    Snobal.Library.Logger.Log("y_axis :" + y_axis);
                    if (y_axis.y == 1 && rightThumbStickState != ThumstickState.UP)
                    {
                        ChangePointerToCurved(1);
                        rightThumbStickState = ThumstickState.UP;
                    }
                }
                if (thumbstickInputLeftAction.action.IsPressed())
                {
                    Vector2 y_axis = thumbstickInputLeftAction.action.ReadValue<Vector2>();
                    if (y_axis.y == 1 && leftThumbStickState != ThumstickState.UP)
                    {
                        ChangePointerToCurved(0);
                        leftThumbStickState = ThumstickState.UP;
                    }
                }
                
            }
        }

        protected override void Process(PointerSource hand, Transform pointerSource, bool hit, RaycastHit? hitInfo)
        {
            switch (hand)
            {
                case PointerSource.Left:
                    if (leftThumbStickState == ThumstickState.UP && !thumbstickInputLeftAction.action.IsPressed())
                    {
                        ChangePointerToStraightLine(0);
                        leftThumbStickState = ThumstickState.DOWN;
                        if (hitInfo != null)
                        {
                            CalculateTeleport(true, hit, hitInfo.Value, hand, indicatorLeft);
                        }
                    }
                    else
                    {
                        if (hitInfo != null)
                        {
                            CalculateTeleport(false, (leftThumbStickState == ThumstickState.UP), hitInfo.Value, hand,
                                indicatorLeft);
                        }
                    }
                    break;
                case PointerSource.Right:
                    if (rightThumbStickState == ThumstickState.UP && !thumbstickInputRightAction.action.IsPressed())
                    {
                        Snobal.Library.Logger.Log("thumbstick dropped....");
                        ChangePointerToStraightLine(1);
                        rightThumbStickState = ThumstickState.DOWN;
                        CalculateTeleport(true, hit, hitInfo.Value, hand, indicatorRight);
                    }
                    else
                    {
                        if (hitInfo != null)
                        {
                            CalculateTeleport(false, (rightThumbStickState == ThumstickState.UP), hitInfo.Value, hand,
                                indicatorRight);
                        }
                    }
                    break;
            }
        }

        private void CalculateTeleport(bool inputRecieved, bool hit, RaycastHit hitInfo, PointerSource hand, GameObject indicator)
        {
            if (indicator == null)
                throw new UnityException("FreeRaycastTeleporting missing indicator object");

            if (!hit)
            {
                DisableIndicator();
                return;
            }

            if (!teleportLayers.IsInLayerMask(hitInfo.collider.gameObject))
            {
                DisableIndicator();
                return;
            }

            if (NavMesh.SamplePosition(hitInfo.point, out NavMeshHit navHit, pointSearchRange, NavMesh.AllAreas))
            {
                // nav mesh sits off the floor mesh, need to do a secondary raycast to find the actual floor position
                if (Physics.Raycast(navHit.position, Vector3.down, out RaycastHit floorHit))
                {
                    indicator.SetActive(true);

                   // indicator.transform.position = floorHit.point;
                    indicator.transform.position = hitInfo.point;
                
                    Vector3 lookRot = Quaternion.LookRotation(indicator.transform.position - CameraRig.RigTransforms.Head.position, Vector3.up).eulerAngles;
                    lookRot.x = 0;
                    lookRot.z = 0;

                    indicator.transform.rotation = Quaternion.Euler(lookRot);

                    if (inputRecieved)
                    {
                        isTeleporting = true;

                        teleportationProvider?.QueueTeleportRequest(new TeleportRequest() { destinationPosition = floorHit.point });
                        
                        teleportAudioSource?.Play();
                        
                        indicator.SetActive(false);
                    }

                    return;
                }
            }

            DisableIndicator();

            void DisableIndicator()
            {
                indicator.SetActive(false);
                indicator.transform.localPosition = Vector3.zero;
            }
        }

        public static bool IsInLayerMask(int layer, LayerMask layermask)
        {
            return layermask == (layermask | (1 << layer));
        }


        public override void ActivateModule(bool value)
        {
            base.ActivateModule(value);
            indicatorLeft?.SetActive(value);
            indicatorRight?.SetActive(value);
        }
    }
}