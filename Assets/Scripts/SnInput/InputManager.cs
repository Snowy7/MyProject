// Disable warning for unused variables & unused events
#pragma warning disable 0414
#pragma warning disable 67

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SnInput
{
    public enum ButtonState
    {
        Pressed,
        Released,
        Held,
        None
    }
    
    public class InputManager : MonoBehaviour 
    {
        public static InputManager Instance { get; private set; }
        
        [SerializeField] private PlayerInput playerInput;
        public bool isInputEnabled = true;

        #region Move & Look
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public float Horizontal => MoveInput.x;
        public float Vertical => MoveInput.y;

        #endregion
        
        #region Private Action bools
        private bool m_jump;
        private bool m_attack;
        private bool m_run;
        #endregion
        
        #region Public Action States & bools
        public bool IsJump => m_jump;
        public bool IsAttack => m_attack;
        public bool IsRunning => m_run;
        public ButtonState JumpState { get; private set; }
        public ButtonState AttackState { get; private set; }
        public ButtonState RunState { get; private set; }
        
        public event Action<ButtonState> OnJump;
        public event Action<ButtonState> OnAttack;
        public event Action<ButtonState> OnRun;
        #endregion
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }
            
            if (playerInput == null)
            {
                Debug.LogError("PlayerInput component not found on GameObject");
            }
            
            playerInput.onActionTriggered += OnActionTriggered;
        }
        
        private void Update()
        {
            JumpState = ButtonUpdate(m_jump, JumpState, OnJump);
            AttackState = ButtonUpdate(m_attack, AttackState, OnAttack);
            RunState = ButtonUpdate(m_run, RunState, OnRun);
        }

        private void OnActionTriggered(InputAction.CallbackContext context)
        {
            if (!isInputEnabled)
            {
                return;
            }

            switch (context.action.name)
            {
                case "Move":
                    // Move
                    MoveInput = context.ReadValue<Vector2>();
                    break;
                case "Look":
                    // Look
                    LookInput = context.ReadValue<Vector2>();
                    break;
                case "Jump":
                    // Jump
                    m_jump = context.ReadValueAsButton();
                    break;
                case "Attack":
                    // Attack
                    m_attack = context.ReadValueAsButton();
                    break;
                case "Run":
                    m_run = context.ReadValueAsButton();
                    break;
            }
        }

        private ButtonState ButtonUpdate(bool button, ButtonState stateState, Action<ButtonState> action = null)
        {
            if (button)
            {
                if (stateState == ButtonState.None || stateState == ButtonState.Released)
                {
                    stateState = ButtonState.Pressed;
                }
                else
                {
                    stateState = ButtonState.Held;
                }
                
                action?.Invoke(stateState);
            }
            else
            {
                if (stateState == ButtonState.Pressed || stateState == ButtonState.Held)
                {
                    stateState = ButtonState.Released;
                    
                    action?.Invoke(stateState);
                }
                else
                {
                    stateState = ButtonState.None;
                }
            }
            
            return stateState;
        }
        
        public string GetCurrentControlScheme()
        {
            return playerInput.currentControlScheme;
        }
        
        public void ListenToSchemeChange(Action<string> action)
        {
            playerInput.onControlsChanged += context => action?.Invoke(context.currentControlScheme);
        }
    }
}