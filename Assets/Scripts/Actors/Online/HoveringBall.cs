using System.Collections;
using System.Threading.Tasks;
using Fusion;
using Game;
using Networking.UI;
using TMPro;
using UnityEngine;

namespace Actors.Online
{
    public class HoveringBall : NetworkBehaviour, IDamageable
    {
        [Title("References")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform target;
        
        [Title("UI")]
        [SerializeField] private TMP_Text countDownText;
        
        [Title("Settings")]
        [SerializeField] private int firstTargetDelay = 3;
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

        public override void Spawned()
        {
            base.Spawned();
            if (HasStateAuthority) CountDown();
        }
        
        async void CountDown()
        {
            if (HasStateAuthority)
            {
                var loader = new Loader(firstTargetDelay.ToString());
                using (loader)
                {
                    for (int i = firstTargetDelay; i > 0; i--)
                    {
                        loader.SetMessage(i.ToString());
                
                        // Wait for a second
                        await Task.Delay(1000);
                    }
                    
                    loader.SetMessage("GO!");
                    SwitchTarget();

                    // Wait for a second
                    await Task.Delay(1000);
                }
            }
        }

        public void FixedUpdate()
        {
            base.FixedUpdateNetwork();
            if (HasStateAuthority) Follow();
        }

        private void Follow()
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
                rb.AddForce(m_transform.forward * (movingForce * Time.fixedDeltaTime), ForceMode.Impulse);
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

        public void TakeDamage(float damage)
        {
            // Check which player is most aligned with the ball and set him as the target
            SwitchTarget();
        }

        void SwitchTarget()
        {
            var players = TestGameManager.Instance.GetPlayers();
            float minAngle = 180;
            Transform closestPlayer = null;
            foreach (var player in players)
            {
                if (player == null) continue;
                
                float angle = Vector3.Angle(player.transform.position - m_transform.position, m_transform.forward);
                if (angle < minAngle)
                {
                    minAngle = angle;
                    closestPlayer = player.GetPlayerBody();
                }
            }
            
            target = closestPlayer;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var actor = other.GetComponent<Actor>();
                if (actor)
                {
                    actor.TakeDamage(999);
                }
                
                TestGameManager.Instance.BallHit();
            }
        }
    }
}