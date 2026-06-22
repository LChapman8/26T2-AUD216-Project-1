using UnityEngine;

/*
 * ChestPickup.cs
 * 
 * Purpose: Handles special chest pickup behavior
 * Used by: Pickup system, treasure chests
 * 
 * Key Features:
 * - Special pickup effects
 * - Unique sound handling
 * - Animation integration
 * - Score management
 * - Visual feedback
 * 
 * Interactions:
 * - Player detection
 * - Collection events
 * - Sound triggers
 * - Particle effects
 * 
 * Dependencies:
 * - Extends PickupBase
 * - PickupsAudioManager
 * - ParticleBurst system
 * - Animation system
 */

public class ChestPickup : PickupBase
{
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject chest;

    public override void Collect()
    {
        base.Collect();
        if (endScreen != null)
        {
            endScreen.SetActive(true);
        }
        if (chest != null)
        {
            chest.SetActive(false);
        }
    }

    public override void PlaySound()
    {
        if (PickupsAudioManager.Instance != null)
        {
            PickupsAudioManager.Instance.PlayChestPickupSound();
        }
        else
        {
            Debug.LogWarning("PickupsAudioManager instance not found!");
        }
    }
}