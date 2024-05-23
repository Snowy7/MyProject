using System.Collections;
using UnityEngine;

namespace Actors.Test
{
    public class TestHoveringBall : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform target;
        
        [Title("Settings")]
        [SerializeField] private float movingForce = 5f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float maxVelocity = 10f;
        [SerializeField, Range(0, 1)] private float redirectAmount = 0.5f;
        [SerializeField] private float bounceForce = 5f;
        Transform m_transform;
        
        bool m_gotHit;
        
        private void Awake()
        {
            m_transform = transform;
        }

        private void Start()
        {
            if (!rb)
            {
                Debug.LogError("No rigidbody reference found!");
                enabled = false;
            }
            
            rb.useGravity = false;
        }

        private void FixedUpdate()
        {
            if (!target) return;
            
            if (!m_gotHit)
            {
                Quaternion lookRot = Quaternion.LookRotation(target.position - m_transform.position);
                m_transform.rotation = Quaternion.Slerp(m_transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
                
                // Get the redirection amount
                float redirectVel = Mathf.Max(1, rb.linearVelocity.magnitude * redirectAmount);
                
                // If the ball is moving, redirect the velocity
                if (rb.linearVelocity.magnitude > 0)
                {
                    rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, m_transform.forward * redirectVel, Time.fixedDeltaTime);
                }
                
                // Limit the velocity
                if (rb.linearVelocity.magnitude > maxVelocity)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
                }
                
                // Use force to move the ball
                rb.AddForce(m_transform.forward * movingForce * Time.fixedDeltaTime, ForceMode.Impulse);
            }
        }
        
        private void OnCollisionEnter(Collision other)
        {
            StartCoroutine(Bounce(other));
        }

        IEnumerator Bounce(Collision other)
        {
            m_gotHit = true;
            // Get the direction of the hit
            Vector3 dir = other.contacts[0].point - m_transform.position;
            // Add force to bounce the ball
            rb.AddForce(-dir.normalized * bounceForce, ForceMode.Impulse);
            
            yield return new WaitForSeconds(0.5f);
            
            m_gotHit = false;
        }
    }
}