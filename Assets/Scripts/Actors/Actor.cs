using Fusion;
using Game;
using UnityEngine;
using UnityEngine.Events;

namespace Actors
{
    public class Actor : NetworkBehaviour, IDamageable
    {
        [Title("Settings")]
        [SerializeField] private bool isPlayer;
        [SerializeField] private float maxHealth = 100f;
        
        [Networked, OnChangedRender(nameof(HealthChanged))]
        public float Health { get; set; } = 100;

        [Title("Events")]
        [SerializeField] private UnityEvent<float> onHealthChanged;
        [SerializeField] private UnityEvent onDeath;
        
        public override void Spawned()
        {
            if (HasStateAuthority)
                Health = maxHealth;
            base.Spawned();
        }
        
        
        public bool IsDead => Health <= 0f;
        
        protected void HealthChanged()
        {
            Debug.Log($"Health changed: {Health}");
            onHealthChanged?.Invoke(Health);
        }
        
        
        
        private void Die()
        {
            Debug.Log("Actor died");
        }

        public void TakeDamage(float damage)
        {
            if (IsDead || !CanBeDamaged()) return;
            RPC_TakeDamage(damage);
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_TakeDamage(float damage)
        {
            Health -= damage;
            if (IsDead)
            {
                onDeath?.Invoke();
                Die();
            }
        }

        public bool CanBeDamaged()
        {
            return !IsDead && Health > 0 && (TestGameManager.Instance.CanDamageOtherPlayers || !isPlayer);
        }
    }
}