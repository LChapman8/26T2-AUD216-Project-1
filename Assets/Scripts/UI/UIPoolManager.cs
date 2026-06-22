using UnityEngine;
using System.Collections.Generic;
using Core.Pooling;

/*
 * UIPoolManager.cs
 * 
 * Purpose: Central manager for all pooled UI elements in the game.
 * Used by: AnimatedObjectiveUI, HUDManager, ObjectiveSystem
 * 
 * Manages multiple pools of different UI elements, handling their lifecycle
 * and reuse. Provides type-safe access to pooled UI components while
 * maintaining proper UI hierarchy.
 * 
 * Performance Considerations:
 * - Reduces UI instantiation overhead
 * - Maintains separate pools for different UI types
 * - Handles proper RectTransform setup
 * 
 * Dependencies:
 * - Core.Pooling.ObjectPool<T>
 * - Requires UI prefabs with RectTransform
 * - Canvas system for UI hierarchy
 */

public class UIPoolManager : MonoBehaviour
{
    public static UIPoolManager Instance { get; private set; }

    [System.Serializable]
    public class UIPoolConfig
    {
        public string poolName;
        public GameObject prefab;
        public int initialSize = 5;
        public Transform container;
    }

    [SerializeField] private UIPoolConfig[] poolConfigs;
    private Dictionary<string, ObjectPool<RectTransform>> pools = new Dictionary<string, ObjectPool<RectTransform>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePools()
    {
        foreach (var config in poolConfigs)
        {
            if (config.container == null)
            {
                config.container = new GameObject($"Pool_{config.poolName}").transform;
                config.container.SetParent(transform);
            }

            RectTransform prefabRect = config.prefab.GetComponent<RectTransform>();
            if (prefabRect == null)
            {
                Debug.LogError($"Prefab {config.prefab.name} must have a RectTransform component!");
                continue;
            }

            pools[config.poolName] = new ObjectPool<RectTransform>(
                prefabRect,
                config.container,
                config.initialSize
            );
        }
    }

    public T Get<T>(string poolName) where T : Component
    {
        if (pools.TryGetValue(poolName, out var pool))
        {
            RectTransform rectTransform = pool.Get();
            return rectTransform.GetComponent<T>();
        }
        Debug.LogError($"Pool {poolName} not found!");
        return null;
    }

    public void Return(string poolName, Component component)
    {
        if (pools.TryGetValue(poolName, out var pool))
        {
            RectTransform rectTransform = component.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                pool.Return(rectTransform);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var pool in pools.Values)
        {
            pool.ClearPool();
        }
        pools.Clear();
    }
}