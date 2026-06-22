using UnityEngine;

/*
 * Zombie.cs
 * 
 * Purpose: Individual zombie behavior and management component
 * Used by: Enemy prefabs, ZombieManager
 * 
 * Key Features:
 * - Registers/unregisters with ZombieManager for global enemy tracking
 * - Sets up compass tracking through WorldSpaceObjectiveManager
 * - Creates unique objective targets for quest/mission system
 * 
 * Dependencies:
 * - Requires ZombieManager in scene
 * - Uses WorldSpaceObjectiveManager for compass tracking
 * - Needs ObjectiveTarget component
 * - Requires compass icon sprite assignment
 */

public class Zombie : MonoBehaviour
{
    [SerializeField] private Sprite enemyCompassIcon;  // Icon displayed on player compass for this enemy
    private ZombieManager zombieManager;               // Reference to global zombie management
    private ObjectiveTarget objectiveTarget;           // Component for quest/objective system integration


    void Start()
    {
        zombieManager = FindFirstObjectByType<ZombieManager>();
        if (zombieManager != null)
        {
            zombieManager.RegisterZombie(gameObject);
        }


        // Add ObjectiveTarget component dynamically
        objectiveTarget = gameObject.AddComponent<ObjectiveTarget>();
        string uniqueId = $"Enemy_{GetInstanceID()}";
        objectiveTarget.ObjectiveId = uniqueId;  // You'll need to add a setter for this


        // Register with WorldSpaceObjectiveManager for compass tracking
        if (WorldSpaceObjectiveManager.Instance != null)
        {
            WorldSpaceObjectiveManager.Instance.CreateObjectiveMarker(uniqueId, transform, enemyCompassIcon);
        }
    }


    void OnDestroy()
    {
        if (zombieManager != null)
        {
            zombieManager.UnregisterZombie(gameObject);
        }
    }
}
