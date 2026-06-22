using UnityEngine;
using System.Collections;

/*
 * Arrow.cs
 * 
 * Purpose: Controls arrow projectile behavior and physics
 * Used by: Crossbow weapon system, projectile management
 * 
 * Key Features:
 * - Projectile physics
 * - Collision detection
 * - Damage application
 * - Pool integration
 * - Impact effects
 * 
 * Physics Features:
 * - Trajectory calculation
 * - Velocity management
 * - Hit detection
 * - Surface interaction
 * 
 * Performance Considerations:
 * - Object pooling integration
 * - Efficient physics checks
 * - Smart cleanup
 * - Resource management
 * 
 * Dependencies:
 * - ArrowPool system
 * - Physics system
 * - Impact effect system
 * - Damage system
 */
public class Arrow : MonoBehaviour
{
    [SerializeField] private float damage = 50f;
    [SerializeField] private float stickDuration = 10f;
    [SerializeField] private ParticleSystem hitEffect;
    
    private bool hasHit = false;
    private Rigidbody rb;
    private Collider arrowCollider;
    private ArrowPool pool;

    public void Initialize(ArrowPool pool)
    {
        this.pool = pool;
        hasHit = false;
        
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (arrowCollider == null) arrowCollider = GetComponent<Collider>();
        
        // Reset state
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        if (arrowCollider != null)
        {
            arrowCollider.enabled = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasHit)
        {
            hasHit = true;
            
            // Handle damage
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                ReturnToPool();
            }
            else
            {
                StickToSurface(collision);
            }
        }
    }

    private void StickToSurface(Collision collision)
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (arrowCollider != null)
        {
            arrowCollider.enabled = false;
        }

        StartCoroutine(CleanupArrow());
    }

    private IEnumerator CleanupArrow()
    {
        yield return new WaitForSeconds(stickDuration);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        StopAllCoroutines();
        if (pool != null)
        {
            pool.ReturnArrow(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
} 