using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/*
 * ObjectiveManager.cs
 * 
 * Purpose: Core manager for all objective functionality
 * Used by: Mission system, game progression
 * 
 * Key Features:
 * - Objective lifecycle management
 * - UI element creation/cleanup
 * - Completion tracking
 * - Prerequisite validation
 * - World marker integration
 * 
 * System Features:
 * - Dynamic objective creation
 * - Progress tracking
 * - Event notifications
 * - Auto-completion logic
 * - UI synchronization
 * 
 * Dependencies:
 * - ObjectiveUI2 prefab
 * - ObjectiveData system
 * - WorldSpaceObjectiveManager
 * - UIAudioManager
 * - FirstTimeInteractionTracker
 */

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private List<ObjectiveData> allObjectives;
    [SerializeField] private ObjectiveUI2 objectiveUIPrefab;
    [SerializeField] private Transform objectivesContainer;

    private Dictionary<string, ObjectiveUI2> activeObjectives = new Dictionary<string, ObjectiveUI2>();
    private HashSet<string> completedObjectives = new HashSet<string>();
    private HashSet<string> hiddenObjectives = new HashSet<string>();

    // Add these to track firestick progress
    private Dictionary<string, int> firestickProgress = new Dictionary<string, int>();
    private Dictionary<string, int> requiredFiresticks = new Dictionary<string, int>()
    {
        { "LightFiresticksGrasslands", 2 },
        { "LightFiresticks", 3 }
    };

    public delegate void ObjectiveAddedHandler(string objectiveId);
    public event ObjectiveAddedHandler OnObjectiveAdded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Load prefab if not assigned
        if (objectiveUIPrefab == null)
        {
            objectiveUIPrefab = Resources.Load<ObjectiveUI2>("Prefabs/UI/ObjectiveUI");
            if (objectiveUIPrefab == null)
            {
                Debug.LogWarning("ObjectiveUI Prefab not found in Resources folder. Some UI features will be disabled.");
            }
        }

        // Create container if not assigned
        if (objectivesContainer == null)
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                objectivesContainer = new GameObject("ObjectivesContainer").transform;
                objectivesContainer.SetParent(canvas.transform, false);
            }
            else
            {
                Debug.LogWarning("No Canvas found in scene. Some UI features will be disabled.");
            }
        }
    }

    private void Start()
    {
        // Initialize progress counters
        firestickProgress["LightFiresticksGrasslands"] = 0;
        firestickProgress["LightFiresticks"] = 0;
    }

    public void AddObjective(string objectiveId)
    {
        if (!HasObjective(objectiveId))
        {
            Debug.LogWarning($"[Objective] Tried to add unknown objective: {objectiveId}");
            return;
        }

        // Remove the objective from completedObjectives when re-adding it
        if (completedObjectives.Contains(objectiveId))
        {
            completedObjectives.Remove(objectiveId);
        }

        if (activeObjectives.ContainsKey(objectiveId))
        {
            return;
        }

        ObjectiveData objectiveData = GetObjectiveData(objectiveId);
        if (objectiveData != null)
        {
            // Create UI element
            if (objectiveUIPrefab != null && objectivesContainer != null)
            {
                ObjectiveUI2 objectiveUI = Instantiate(objectiveUIPrefab, objectivesContainer);
                objectiveUI.gameObject.SetActive(true);
                objectiveUI.Initialize(objectiveData);
                activeObjectives.Add(objectiveId, objectiveUI);
                OnObjectiveAdded?.Invoke(objectiveId);

                if (FirstTimeInteractionTracker.Instance != null)
                {
                    bool shouldAutoComplete = 
                        (objectiveId == "PickupFishingRod" && FirstTimeInteractionTracker.Instance.HasPickedUpFishingRod()) ||
                        (objectiveId == "CatchAFish" && FirstTimeInteractionTracker.Instance.HasCaughtFirstFish()) ||
                        // Add auto-completion check for PickupAxe
                        (objectiveId == "PickupAxe" && FirstTimeInteractionTracker.Instance.HasPickedUpAxe()) ||
                        // Add auto-completion check for LightCampfire
                        (objectiveId == "LightCampfire" && FirstTimeInteractionTracker.Instance.HasLitCampfire());

                    if (shouldAutoComplete)
                    {
                        completedObjectives.Add(objectiveId);
                        StartCoroutine(AutoCompleteObjectiveWithDelay(objectiveId, 0.5f));
                    }
                    else if (objectiveData.showWorldMarker && WorldSpaceObjectiveManager.Instance != null)
                    {
                        List<Transform> targets = WorldSpaceObjectiveManager.Instance.GetTargetLocations(objectiveId);
                        if (targets != null && targets.Count > 0)
                        {
                            WorldSpaceObjectiveManager.Instance.CreateObjectiveMarkers(
                                objectiveId,
                                targets,
                                objectiveData.markerIcon
                            );
                        }
                    }
                }
            }
        }
    }

    private IEnumerator AutoCompleteObjectiveWithDelay(string objectiveId, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (activeObjectives.ContainsKey(objectiveId))
        {
            CompleteObjective(objectiveId);
        }
    }

    public void CompleteObjective(string objectiveId)
    {
        if (activeObjectives.ContainsKey(objectiveId))
        {
            completedObjectives.Add(objectiveId);
            activeObjectives[objectiveId].PlayCompletionAnimation();
            
            // Find and handle all ObjectiveTargetLocators for this objective
            var targetLocators = ObjectiveTargetLocator.FindLocatorsForTarget(objectiveId);
            foreach (var locator in targetLocators)
            {
                if (locator != null)
                {
                    locator.enabled = false;
                }
            }

            // Play completion sound
            if (UIAudioManager.Instance != null)
            {
                UIAudioManager.Instance.PlayObjectiveCompleted();
            }

            StartCoroutine(RemoveObjectiveAfterDelay(objectiveId, 1f));
        }
    }

    private IEnumerator RemoveObjectiveAfterDelay(string objectiveId, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (activeObjectives.ContainsKey(objectiveId))
        {
            Destroy(activeObjectives[objectiveId].gameObject);
            activeObjectives.Remove(objectiveId);
        }
    }

    public void HideObjective(string objectiveId)
    {
        if (activeObjectives.ContainsKey(objectiveId))
        {
            Destroy(activeObjectives[objectiveId].gameObject);
            activeObjectives.Remove(objectiveId);
            hiddenObjectives.Add(objectiveId);
        }
    }

    public void ShowObjective(string objectiveId)
    {
        ObjectiveData objectiveData = GetObjectiveData(objectiveId);
        if (objectiveData == null) return;

        // Check prerequisites
        if (objectiveData.prerequisites != null && objectiveData.prerequisites.Length > 0)
        {
            foreach (ObjectiveData prerequisite in objectiveData.prerequisites)
            {
                if (prerequisite == null) continue;
                
                if (!IsObjectiveCompleted(prerequisite.objectiveId))
                {
                    return;
                }
            }
        }

        hiddenObjectives.Remove(objectiveId);
        AddObjective(objectiveId);
    }

    public bool HasObjective(string objectiveId)
    {
        return allObjectives.Exists(obj => obj.objectiveId == objectiveId);
    }

    public ObjectiveData GetObjectiveData(string objectiveId)
    {
        return allObjectives.Find(obj => obj.objectiveId == objectiveId);
    }

    public bool IsObjectiveActive(string objectiveId)
    {
        return activeObjectives.ContainsKey(objectiveId) && !completedObjectives.Contains(objectiveId);
    }

    public bool IsObjectiveCompleted(string objectiveId)
    {
        return completedObjectives.Contains(objectiveId);
    }

    public void IncrementFirestickProgress(string objectiveId)
    {
        if (firestickProgress.ContainsKey(objectiveId))
        {
            firestickProgress[objectiveId]++;
            
            if (firestickProgress[objectiveId] >= requiredFiresticks[objectiveId])
            {
                CompleteObjective(objectiveId);
                
                if (WorldSpaceObjectiveManager.Instance != null)
                {
                    WorldSpaceObjectiveManager.Instance.RemoveObjectiveMarker(objectiveId);
                }
            }
        }
    }
}