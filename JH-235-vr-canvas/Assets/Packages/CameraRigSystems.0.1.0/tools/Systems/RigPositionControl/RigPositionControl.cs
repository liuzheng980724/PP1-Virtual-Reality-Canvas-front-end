using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Snobal.CameraRigSystems_0_0
{
    public class RigPositionControl: MonoBehaviour
    {
        public static event Action<Vector3> OnRigTeleportPosition;
        
        [SerializeField]
        private TeleportationProvider teleportationProvider;

        private void OnValidate()
        {
            if (teleportationProvider == null)
                teleportationProvider = this.GetComponentInChildren<TeleportationProvider>();
        }

        private void Awake()
        {
            Debug.Assert(teleportationProvider != null);
        }

        public void MoveRig(Vector3 _position)
        {
            var request = new TeleportRequest()
            {
                destinationPosition = _position,
            };

            teleportationProvider?.QueueTeleportRequest(request);

            OnRigTeleportPosition?.Invoke(_position);
        }

        public void MoveRig(Vector3 _position, Quaternion _rotation)
        {
            var request = new TeleportRequest() {                
                destinationPosition = _position,
                destinationRotation = _rotation                
            };

            teleportationProvider?.QueueTeleportRequest(request);

            OnRigTeleportPosition?.Invoke(_position);
        }     
    }
}