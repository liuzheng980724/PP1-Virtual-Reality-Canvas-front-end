using UnityEngine;
using UnityEngine.InputSystem;

namespace AdvancedCharacterController.Movement.InputManager
{
    [RequireComponent(typeof(Core.AdvancedCharacterController))]
    public class PCEmulationMovement : MonoBehaviour
    {
        public InputActionProperty horizontalInputAction;
        public InputActionProperty verticalInputAction;
        public InputActionProperty jumpInputAction;        

        [Header("Properties")]
        public float moveSpeed = 7f;
        public float jumpSpeed = 15f;


        bool jumpScheduled = false;
        private Core.AdvancedCharacterController _characterController;

        private void Awake()
        {
            _characterController = GetComponent<Core.AdvancedCharacterController>();
        }

        private void Update()
        {
            if (jumpInputAction.action.triggered)
                jumpScheduled = true;            
        }

        private void FixedUpdate()
        {
            Vector3 velocity = CalculateMovement() * moveSpeed;

            if (jumpScheduled && _characterController.IsGrounded)
            {
                _characterController.Move(transform.up * jumpSpeed);
                jumpScheduled = false;
            }

            _characterController.Move(velocity);
        }

        private Vector3 CalculateMovement()
        {
            Vector3 velocity = Vector3.zero;

            float horizontal = horizontalInputAction.action.ReadValue<float>();
            float vertical = verticalInputAction.action.ReadValue<float>();

            velocity += transform.right * horizontal;
            velocity += transform.forward * vertical;

            if (velocity.sqrMagnitude > 1f)
                velocity.Normalize();

            return velocity;
        }
    }
}