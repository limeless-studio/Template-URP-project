using UnityEngine;

namespace Core
{
    public class Actor : MonoBehaviour, IDamageable
    {
        [HideInInspector] public float maxHealth = 100f;
        [HideInInspector] public bool startWithMaxHealth = true;
        [HideInInspector] public float startingHealth = 100f;
        
        public float Health { get; protected set; }
        public bool IsAlive => Health > 0;
        
        protected virtual void Awake()
        {
            Health = startWithMaxHealth ? maxHealth : startingHealth;
        }

        public virtual void TakeDamage(float damage)
        {
            TakeDamage(damage, DamageCause.DamagedByOther);
        }
        
        public virtual void ServerTakeDamage(float damage) {}
        
        
        public virtual void TakeDamage(float damage, bool onlyIfOwned)
        {
            TakeDamage(damage, DamageCause.DamagedByOther);
        }

        public virtual void TakeDamage(float damage, DamageCause cause)
        {
            Health = Mathf.Clamp(Health - damage, 0, maxHealth);
            if (Health <= 0) Die();
        }

        public virtual void Heal(float amount)
        {
            Health = Mathf.Clamp(Health + amount, 0, maxHealth);
        }

        public virtual void Die()
        {
            // TODO: Generic Death
        }

        public virtual void SetHealth(float amount)
        {
            Health = Mathf.Clamp(amount, 0, maxHealth);
        }

        public virtual float GetHealth() => Health;
        
        public virtual Transform GetTransform() => transform;
    }
}