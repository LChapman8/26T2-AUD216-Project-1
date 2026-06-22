using UnityEngine;

public class DamageableObject : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle death (e.g., play animation, destroy object, etc.)
        Debug.Log($"{gameObject.name} has been destroyed!");
        Destroy(gameObject);
    }
} 