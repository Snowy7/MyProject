using System.Collections.Generic;
using Actors;
using Fusion;
using Game;
using Player.Offline;
using SnInput;
using UnityEngine;
using Utils.Attributes;

namespace Player.Online
{
    public class Player : NetworkBehaviour, IAfterSpawned
    {
        [Title("References")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private CharacterAnimator anim;
        [SerializeField] private Actor actor;
        [SerializeField, Tooltip("Player components that should be disabled when the player is not local")]
        List<MonoBehaviour> playerComponents = new List<MonoBehaviour>();
        
        [Title("Attack Settings")]
        [CharacterAnimatorState("anim"), SerializeField] private string attackState;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float attackRadius = 1f;
        [SerializeField] private float attackDamage = 10f;
        
        private float lastAttackTime;
        
        # if UNITY_EDITOR
        private void OnValidate()
        {
            if (!anim)
            {
                anim = GetComponentInChildren<CharacterAnimator>();
            }
        }
        
        private void OnDrawGizmos()
        {
            if(!playerBody) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerBody.position, attackRadius);
        }
        
        #endif
        
        private void Start()
        {
            if (!anim)
            {
                anim = GetComponentInChildren<CharacterAnimator>();
            }
            
            if (!stats) stats = GetComponent<PlayerStats>();
            if (!actor) actor = GetComponent<Actor>();
        }

        public void AfterSpawned()
        {
            // Do something after the player has been spawned
            foreach (var component in playerComponents)
            {
                component.enabled = HasStateAuthority;
            }
            
            TestGameManager.Instance.AddPlayer(this);
        }

        public override void Spawned()
        {
            Debug.Log($"Player spawned: {HasInputAuthority}");
            if (HasInputAuthority) InputManager.Instance.OnAttack += OnAttack;
            
            base.Spawned();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (HasInputAuthority) InputManager.Instance.OnAttack -= OnAttack;
            
            base.Despawned(runner, hasState);
        }

        private void OnAttack(ButtonState state)
        {
            // Attack over the network.
            if (state == ButtonState.Pressed)
            {
                Attack();
            }
        }

        void Attack()
        {
            // Implement attack logic
            // 1. Check for the attack cooldown
            // 2. Damage everything in the attack radius
            
            if (!CanAttack()) return;
            
            lastAttackTime = Time.time;
            
            // Check for the attack radius
            var colliders = Physics.OverlapSphere(playerBody.position, attackRadius);
            foreach (var coll in colliders)
            {
                var damageable = coll.GetComponentInParent<IDamageable>();
                if (damageable != null && !ReferenceEquals(damageable, actor))
                {
                    damageable.TakeDamage(attackDamage);
                }
            }
            
            
            // Run the synced effects
            RPC_Attack();
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_Attack()
        {
            // Attack on all clients.
            // Trigger the animation
            anim.PlayAnimation(attackState, true, 0.1f);
        }
        
        private bool CanAttack()
        {
            return Time.time > lastAttackTime + attackCooldown && HasInputAuthority && playerBody;
        }

        public Transform GetPlayerBody()
        {
            return playerBody;
        }
        
        public PlayerStats GetStats()
        {
            return stats;
        }
        
        public void SetPosition(Vector3 position)
        {
            playerBody.position = position;
        }
    }
}