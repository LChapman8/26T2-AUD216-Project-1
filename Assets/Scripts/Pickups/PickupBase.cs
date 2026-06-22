using UnityEngine;
using System;

/*
 * PickupBase.cs
 * 
 * Purpose: Abstract base class for all collectible items
 * Used by: Pickup system, item collection
 * 
 * Key Features:
 * - Score value management
 * - Collection events
 * - Visual feedback
 * - Sound effects
 * - Clean cleanup
 * 
 * Implementation Details:
 * - Abstract sound system
 * - Component caching
 * - Event-based scoring
 * - Automatic cleanup
 * 
 * Dependencies:
 * - Requires MeshRenderer
 * - Requires Collider
 * - IPickupable interface
 * - PickupsAudioManager
 */

public abstract class PickupBase : MonoBehaviour, IPickupable
{
    [SerializeField] protected int scoreValue = 1;
    [SerializeField] protected float destroyDelay = 0.5f;

    protected MeshRenderer meshRenderer;
    protected Collider pickupCollider;

    public int ScoreValue => scoreValue;

    public static event Action<int> OnPickupCollected;

    protected virtual void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        pickupCollider = GetComponent<Collider>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    public virtual void Collect()
    {
        OnPickupCollected?.Invoke(scoreValue);
        PlaySound();
        DisableVisuals();
        DisableCollider();
        Destroy(gameObject, destroyDelay);
    }

    public abstract void PlaySound();

    protected virtual void DisableVisuals()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
    }

    protected virtual void DisableCollider()
    {
        if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }
    }

    public virtual void ResetPickup()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }
        if (pickupCollider != null)
        {
            pickupCollider.enabled = true;
        }
        gameObject.SetActive(true);
    }
}