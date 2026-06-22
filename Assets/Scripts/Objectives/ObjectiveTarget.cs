using UnityEngine;
using System.Linq;

/*
 * ObjectiveTarget.cs
 * 
 * Purpose: Links world objects to objective system
 * Used by: World space markers, objective locations
 * 
 * Key Features:
 * - Target registration
 * - Location tracking
 * - Automatic cleanup
 * - Multi-target support
 * 
 * Implementation:
 * - Transform registration
 * - Target lifecycle management
 * - Dynamic ID assignment
 * - Target discovery
 * 
 * Dependencies:
 * - WorldSpaceObjectiveManager
 * - Objective system
 * - Scene hierarchy
 */

public class ObjectiveTarget : MonoBehaviour
{
    [SerializeField] private string objectiveId;
    
    public string ObjectiveId 
    { 
        get => objectiveId;
        set 
        {
            objectiveId = value;
            if (Application.isPlaying && WorldSpaceObjectiveManager.Instance != null)
            {
                WorldSpaceObjectiveManager.Instance.RegisterTarget(objectiveId, transform);
            }
        }
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(objectiveId))
        {
            Debug.LogError($"ObjectiveTarget on {gameObject.name} has no objectiveId assigned!");
            return;
        }

    //    Debug.Log($"Registering target for objective: {objectiveId} at position {transform.position}");
        if (WorldSpaceObjectiveManager.Instance != null)
        {
            WorldSpaceObjectiveManager.Instance.RegisterTarget(objectiveId, transform);
        }
        else
        {
            Debug.LogError("WorldSpaceObjectiveManager.Instance is null!");
        }
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(objectiveId) && WorldSpaceObjectiveManager.Instance != null)
        {
            WorldSpaceObjectiveManager.Instance.UnregisterTarget(objectiveId);
        }
    }

    // Helper method to find all targets for a specific objective
    public static ObjectiveTarget[] FindTargetsForObjective(string objectiveId)
    {
        return GameObject.FindObjectsByType<ObjectiveTarget>(FindObjectsSortMode.None)
            .Where(target => target.ObjectiveId == objectiveId)
            .ToArray();
    }
} 