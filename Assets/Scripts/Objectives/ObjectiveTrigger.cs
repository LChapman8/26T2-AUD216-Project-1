using UnityEngine;

/*
 * ObjectiveTrigger.cs
 * 
 * Purpose: Handles trigger-based objective completion
 * Used by: Level design, mission progression
 * 
 * Key Features:
 * - Trigger-based completion
 * - Time of day control
 * - Speed modification
 * - One-time triggers
 * - Auto-cleanup
 * 
 * Time Control Features:
 * - Time of day setting
 * - Speed multipliers
 * - Trigger validation
 * - State persistence
 * 
 * Dependencies:
 * - DayNightCycle system
 * - ObjectiveManager
 * - Collider system
 * - Time management
 */

public class ObjectiveTrigger : MonoBehaviour
{
    [Header("Objective Settings")]
    [SerializeField] private string objectiveId;
    [SerializeField] private bool destroyAfterTrigger = true;
    [SerializeField] private bool requiresObjectiveToBeActive = true;

    [Header("Time Control")]
    [SerializeField] private bool shouldSetTimeOfDay = false;
    [SerializeField, Range(0f, 1f)] 
    [Tooltip("Time of day to set when triggered (0-1)")]
    private float timeToSet = 0.5f;
    
    [SerializeField] private bool shouldModifyTimeSpeed = false;
    [SerializeField, Range(0.1f, 10f)]
    [Tooltip("Multiplier for time of day progression speed")]
    private float timeSpeedMultiplier = 1f;

    [SerializeField] private DayNightCycle dayNightCycle;

    private bool hasTriggered = false;

    private void Start()
    {
        if (shouldSetTimeOfDay || shouldModifyTimeSpeed)
        {
            if (dayNightCycle == null)
            {
                dayNightCycle = FindFirstObjectByType<DayNightCycle>();
                if (dayNightCycle == null)
                {
                    Debug.LogError($"[ObjectiveTrigger] No DayNightCycle found but time control is enabled on {gameObject.name}");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        
        if (other.CompareTag("Player"))
        {
            if (string.IsNullOrEmpty(objectiveId))
            {
                Debug.LogWarning($"[ObjectiveTrigger] No objective ID assigned to trigger on {gameObject.name}");
                return;
            }

            // Check if the objective needs to be active before it can be completed
            if (requiresObjectiveToBeActive && 
                ObjectiveManager.Instance != null && 
                !ObjectiveManager.Instance.IsObjectiveActive(objectiveId))
            {
                return;
            }

            // Complete the objective
            if (ObjectiveManager.Instance != null)
            {
                try
                {
                    ObjectiveManager.Instance.CompleteObjective(objectiveId);
                    hasTriggered = true;

                    // Handle time control
                    if (dayNightCycle != null)
                    {
                        if (shouldSetTimeOfDay)
                        {
                            Debug.Log($"[ObjectiveTrigger] Setting time of day to {timeToSet}");
                            dayNightCycle.SetTimeOfDay(timeToSet);
                        }

                        if (shouldModifyTimeSpeed)
                        {
                            Debug.Log($"[ObjectiveTrigger] Setting time speed multiplier to {timeSpeedMultiplier}");
                            dayNightCycle.SetTimeSpeedMultiplier(timeSpeedMultiplier);
                        }
                    }

                    if (destroyAfterTrigger)
                    {
                        Destroy(gameObject);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ObjectiveTrigger] Failed to complete objective {objectiveId}: {e.Message}");
                }
            }
        }
    }

    // Optional: Add this if you want to be able to reset the trigger
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
} 