using UnityEngine;
using Core.Pooling;

/*
 * ArrowPool.cs
 * 
 * Purpose: Manages a pool of arrow projectiles for the crossbow weapon system.
 * Used by: CrossbowController, Arrow
 * 
 * Maintains a singleton instance that handles arrow instantiation and recycling.
 * Provides methods for getting new arrows and returning used ones to the pool.
 * 
 * Performance Considerations:
 * - Pre-instantiates arrows to avoid runtime allocation
 * - Handles proper cleanup of physics components
 * - Manages arrow lifecycle and state reset
 * 
 * Dependencies:
 * - Core.Pooling.ObjectPool<T>
 * - Arrow.cs (for projectile behavior)
 * - Requires arrow prefab with proper components
 */

public class ArrowPool : MonoBehaviour
{
    public static ArrowPool Instance { get; private set; }

    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private Transform poolContainer;

    private ObjectPool<Arrow> pool;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        if (poolContainer == null)
        {
            poolContainer = new GameObject("Arrow Pool").transform;
            poolContainer.SetParent(transform);
        }

        pool = new ObjectPool<Arrow>(arrowPrefab, poolContainer, initialPoolSize);
    }

    public Arrow GetArrow()
    {
        Arrow arrow = pool.Get();
        arrow.Initialize(this);
        return arrow;
    }

    public void ReturnArrow(Arrow arrow)
    {
        pool.Return(arrow);
    }

    public void PrewarmPool(int amount)
    {
        pool.PrewarmPool(amount);
    }

    private void OnDestroy()
    {
        if (pool != null)
        {
            pool.ClearPool();
        }
    }
} 