using System;
using UnityEngine;

namespace Player.Offline
{
    // ignore this field is assigned but its value is never used
    # pragma warning disable 414
    public class CharacterAnimator : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform playerBody;
        
        [Title("Settings")]
        [SerializeField] private float animationSpeed = 1f;
        [SerializeField] private float runSpeed = 5f;
        
        private bool m_falseBool = false;
        [DisableIf(nameof(m_falseBool), false)]
        [SerializeField] private Vector3 velocity;
        [DisableIf(nameof(m_falseBool), false)]
        [SerializeField] private float speed;
        
        // Hashes
        private static readonly int ANIMATION_SPEED = Animator.StringToHash("Animation Speed");
        private static readonly int MOVING = Animator.StringToHash("Moving");
        private static readonly int VELOCITY = Animator.StringToHash("Velocity");
        
        Vector3 m_lastPosition;

        private void Start()
        {
            if (!animator)
            {
                Debug.LogError("No animator reference found!");
                enabled = false;
            }
            
            
            animator.SetFloat(ANIMATION_SPEED, animationSpeed);
        }

        private void LateUpdate()
        {
            if (!animator) return;
            
            Vector3 pos = playerBody.position;
            // Check if moving
            velocity = (pos - m_lastPosition) / Time.deltaTime;
            speed = new Vector2(velocity.x, velocity.z).magnitude;
            
            animator.SetBool(MOVING, speed > 0.1f);
            float lerp = Mathf.Lerp(animator.GetFloat(VELOCITY), speed / runSpeed, Time.deltaTime * 10);
            animator.SetFloat(VELOCITY, lerp);
            
            m_lastPosition = pos;
        }
        
        # region API
        
        public void PlayAnimation(string stateName, bool crossFade = false, float crossFadeTime = 0.2f)
        {
            if (crossFade) animator.CrossFade(stateName, crossFadeTime);
            else animator.Play(stateName);
        }
        
        // Get all clips
        public AnimationClip[] GetClips()
        {
            if (!animator) return Array.Empty<AnimationClip>();
            var clips = animator.runtimeAnimatorController.animationClips;
            return clips;
        }
        
        #endregion
    }
}