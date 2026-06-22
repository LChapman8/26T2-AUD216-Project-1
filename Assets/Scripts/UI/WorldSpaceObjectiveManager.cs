using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

/*
 * WorldSpaceObjectiveManager.cs
 * 
 * Purpose: Manages 3D objective markers in the game world.
 * Used by: ObjectiveSystem, UI system
 * 
 * Controls the creation, positioning, and lifecycle of world-space
 * markers that indicate objective locations. Handles visibility,
 * distance checking, and marker cleanup.
 * 
 * Performance Considerations:
 * - Uses object pooling for markers
 * - Implements distance-based culling
 * - Optimizes marker updates
 * 
 * Dependencies:
 * - ObjectiveSystem
 * - UI prefabs for markers
 * - Requires proper layer setup
 */

public class WorldSpaceObjectiveManager : MonoBehaviour
{
    public static WorldSpaceObjectiveManager Instance { get; private set; }

    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private Transform markerContainer;

    private Dictionary<string, WorldSpaceObjectiveMarker> activeMarkers = 
        new Dictionary<string, WorldSpaceObjectiveMarker>();
    private Dictionary<string, List<Transform>> targetLocations = new Dictionary<string, List<Transform>>();

    public IReadOnlyDictionary<string, WorldSpaceObjectiveMarker> ActiveMarkers => activeMarkers;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Only log errors for missing critical components
        if (markerPrefab == null)
        {
            Debug.LogError("Marker Prefab not assigned in WorldSpaceObjectiveManager!");
        }
        if (markerContainer == null)
        {
            Debug.LogError("Marker Container not assigned in WorldSpaceObjectiveManager!");
        }
    }

    public void CreateObjectiveMarker(string objectiveId, Transform target, Sprite icon)
    {
        if (markerPrefab == null || markerContainer == null)
        {
            Debug.LogError("Cannot create marker: missing required references!");
            return;
        }

        if (activeMarkers.ContainsKey(objectiveId))
        {
            RemoveObjectiveMarker(objectiveId);
        }

        GameObject markerObj = Instantiate(markerPrefab, markerContainer);
        WorldSpaceObjectiveMarker marker = markerObj.GetComponent<WorldSpaceObjectiveMarker>();
        
        if (marker != null)
        {
            marker.Initialize(target, icon);
            activeMarkers.Add(objectiveId, marker);
        }
        else
        {
            Debug.LogError("Created marker object but couldn't find WorldSpaceObjectiveMarker component!");
        }
    }

    public void RemoveObjectiveMarker(string objectiveId, Transform target = null)
    {
        if (target != null)
        {
            // Remove specific marker for this target
            string uniqueMarkerId = $"{objectiveId}_{target.GetInstanceID()}";
            //Debug.Log($"[WorldSpaceObjectiveManager] Attempting to remove marker: {uniqueMarkerId}");
            if (activeMarkers.ContainsKey(uniqueMarkerId))
            {
                if (activeMarkers[uniqueMarkerId] != null)
                {
                    Destroy(activeMarkers[uniqueMarkerId].gameObject);
                }
                activeMarkers.Remove(uniqueMarkerId);
                //Debug.Log($"[WorldSpaceObjectiveManager] Successfully removed marker: {uniqueMarkerId}");
            }
        }
        else
        {
            // Remove all markers for this objective ID
            var markersToRemove = activeMarkers.Keys
                .Where(k => k.StartsWith(objectiveId + "_"))
                .ToList();

            foreach (var markerId in markersToRemove)
            {
//                Debug.Log($"[WorldSpaceObjectiveManager] Attempting to remove marker: {markerId}");
                if (activeMarkers[markerId] != null)
                {
                    Destroy(activeMarkers[markerId].gameObject);
                }
                activeMarkers.Remove(markerId);
  //              Debug.Log($"[WorldSpaceObjectiveManager] Successfully removed marker: {markerId}");
            }
        }
    }

    public void SetMarkerPulse(string objectiveId, bool active)
    {
        if (activeMarkers.TryGetValue(objectiveId, out WorldSpaceObjectiveMarker marker))
        {
            marker.SetPulseActive(active);
        }
    }

    public void SetMarkerGlow(string objectiveId, bool active)
    {
        if (activeMarkers.TryGetValue(objectiveId, out WorldSpaceObjectiveMarker marker))
        {
            marker.SetGlowActive(active);
        }
    }

    public void RegisterTarget(string targetId, Transform target)
    {
        if (!targetLocations.ContainsKey(targetId))
        {
            targetLocations[targetId] = new List<Transform>();
        }
        targetLocations[targetId].Add(target);
    }

    public List<Transform> GetTargetLocations(string targetId)
    {
        if (targetLocations.TryGetValue(targetId, out List<Transform> targets))
        {
            return targets;
        }
        return null;
    }

    public void CreateObjectiveMarkers(string objectiveId, List<Transform> targets, Sprite icon)
    {
        if (targets == null || targets.Count == 0) return;

        foreach (Transform target in targets)
        {
            string uniqueMarkerId = $"{objectiveId}_{target.GetInstanceID()}";
            
            // Create world space marker
            if (markerPrefab != null && markerContainer != null)
            {
                GameObject markerObj = Instantiate(markerPrefab, markerContainer);
                WorldSpaceObjectiveMarker marker = markerObj.GetComponent<WorldSpaceObjectiveMarker>();
                
                if (marker != null)
                {
                    marker.Initialize(target, icon);
                    activeMarkers[uniqueMarkerId] = marker;
                }
            }
        }
    }

    public void UnregisterTarget(string targetId)
    {
        if (targetLocations.ContainsKey(targetId))
        {
            targetLocations.Remove(targetId);
        }
    }
}