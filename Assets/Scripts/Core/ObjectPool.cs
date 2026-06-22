using UnityEngine;
using System.Collections.Generic;

/*
 * ObjectPool.cs
 * 
 * Purpose: Core system for object pooling to reduce garbage collection and improve performance.
 * Used by: ArrowPool, UIPoolManager, ParticleEffectPool
 * 
 * This generic pooling system manages reusable Unity Components to avoid expensive
 * instantiate/destroy operations. It's particularly important for frequently spawned objects
 * like projectiles, UI elements, and particle effects.
 * 
 * Performance Considerations:
 * - Reduces GC pressure by reusing objects instead of creating/destroying
 * - Maintains a fixed memory footprint after initialization
 * - Automatically expands pool size when needed
 * 
 * Dependencies:
 * - Requires objects with Component
 * - Works with any MonoBehaviour-derived class
 */

namespace Core.Pooling
{
    public class ObjectPool<T> where T : Component
    {
        private Queue<T> pool;
        private T prefab;
        private Transform container;
        private int defaultCapacity;

        public ObjectPool(T prefab, Transform container, int initialCapacity = 10)
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab cannot be null when creating ObjectPool!");
                return;
            }

            this.prefab = prefab;
            this.container = container;
            this.defaultCapacity = initialCapacity;
            Initialize(initialCapacity);
        }

        private void Initialize(int capacity)
        {
            pool = new Queue<T>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                CreateNewInstance();
            }
        }

        private void CreateNewInstance()
        {
            var obj = GameObject.Instantiate(prefab, container);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }

        public T Get()
        {
            if (pool.Count == 0)
            {
                CreateNewInstance();
            }

            var obj = pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            if (obj != null)
            {
                obj.gameObject.SetActive(false);
                obj.transform.SetParent(container);
                pool.Enqueue(obj);
            }
        }

        public void PrewarmPool(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                if (pool.Count < defaultCapacity * 2)
                {
                    CreateNewInstance();
                }
            }
        }

        public void ClearPool()
        {
            while (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                if (obj != null)
                {
                    GameObject.Destroy(obj.gameObject);
                }
            }
        }
    }
}