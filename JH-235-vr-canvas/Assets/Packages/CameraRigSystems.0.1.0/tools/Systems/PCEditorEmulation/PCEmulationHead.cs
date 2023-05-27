using UnityEngine;
using UnityEngine.InputSystem;

namespace Snobal.CameraRigSystems_0_0
{
    public class PCEmulationHead : MonoBehaviour
    {
        [SerializeField]
        private Transform playerCameraPivot;

        public float lookSpeed = 0.5f;
        public float lookXLimit = 45.0f;
        public float standingHeight = 1.7f;
        public float crouchingHeight = 0.85f;
        public float crouchSpeed = 10f;

        private float rotationX = 0;
        private Vector3 camHeight = Vector3.zero;
        private bool isCrouching = false;


        [SerializeField]
        InputActionProperty crouchToggleInputAction;
        [SerializeField]
        InputActionProperty lookAxisY;
        [SerializeField]
        InputActionProperty lookAxisX;

        void Awake()
        {
            Debug.Assert(playerCameraPivot != null, "PCInputEmulationVR missing reference to Transform playerCameraPivot");

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Update()
        {
            // toggle crouching
            if (crouchToggleInputAction.action.triggered)
                isCrouching = !isCrouching;

            // crouching
            if (isCrouching)
                camHeight = new Vector3(0, crouchingHeight, 0);
            else
                camHeight = new Vector3(0, standingHeight, 0);

            // Player and Camera rotation
            rotationX += -lookAxisY.action.ReadValue<float>() * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCameraPivot.localPosition = Vector3.Lerp(playerCameraPivot.localPosition, camHeight, Time.deltaTime * crouchSpeed);
            playerCameraPivot.localRotation = Quaternion.Euler(rotationX, 0, 0);
            this.transform.rotation *= Quaternion.Euler(0, lookAxisX.action.ReadValue<float>() * lookSpeed, 0);
        }
    }
}