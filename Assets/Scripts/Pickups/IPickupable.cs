using UnityEngine;

/*
 * IPickupable.cs
 * 
 * Purpose: Interface defining pickup behavior contract
 * Used by: Pickup system, collectible items
 * 
 * Key Features:
 * - Collection behavior
 * - Sound effect triggers
 * - Reset functionality
 * - Score value access
 * 
 * Required Implementation:
 * - Collect() method
 * - PlaySound() method
 * - ResetPickup() method
 * - ScoreValue property
 * 
 * Dependencies:
 * - None (interface definition)
 */

public interface IPickupable
{
    int ScoreValue { get; }
    void Collect();
    void PlaySound();
}