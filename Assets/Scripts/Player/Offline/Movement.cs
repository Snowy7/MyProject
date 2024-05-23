using SnInput;
using UnityEngine;

namespace Player.Offline
{
    [RequireComponent(typeof(CharacterController))]
    public class Movement : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private Transform camPivot;
        [SerializeField] private Transform playerBody;
        
        [Title("Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float gravityMultiplier = 2f;
        
        private CharacterController m_controller;
        
        private Vector3 m_velocity;
        private bool m_isGrounded;
        private bool m_isJumping;
        
        #region Monobehaviour
        
        private void Awake()
        {
            m_controller = GetComponent<CharacterController>();
        }
        
        private void OnEnable()
        {
            InputManager.Instance.OnJump += OnJump;
        }
        
        private void OnDisable()
        {
            InputManager.Instance.OnJump -= OnJump;
        }
        
        private void Update()
        {
            UpdateGrounded();
            Move();
            Jump();
            ApplyGravity();
            
            ApplyVelocity();
        }
        
        #endregion
        
        private void OnJump(ButtonState state)
        {
            switch (state)
            {
                case ButtonState.Pressed:
                    m_isJumping = true;
                    break;
                case ButtonState.Released:
                    m_isJumping = false;
                    break;
            }
        }
        
        private void UpdateGrounded()
        {
            m_isGrounded = m_controller.isGrounded;
        }
        
        private void Move()
        {
            float speed = walkSpeed;
            if (InputManager.Instance.IsRunning)
            {
                speed = runSpeed;
            }

            Vector3 move = camPivot.forward * InputManager.Instance.Vertical + camPivot.right * InputManager.Instance.Horizontal;
            m_velocity.x = move.x * speed;
            m_velocity.z = move.z * speed;
            
            // PLayer body face the direction of the movement
            if (move.magnitude > 0)
            {
                Quaternion targetRot = Quaternion.LookRotation(new Vector3(m_velocity.x, 0, m_velocity.z));
                playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetRot, 15f * Time.deltaTime);
            }
        }
        
        private void Jump()
        {
            if (m_isJumping && m_isGrounded)
            {
                m_velocity.y = jumpForce;
            }
        }
        
        private void ApplyGravity()
        {
            if (!m_isGrounded)
            {
                m_velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            }
        }
        
        private void ApplyVelocity()
        {
            m_controller.Move(m_velocity * Time.deltaTime);
        }
        
    }
}