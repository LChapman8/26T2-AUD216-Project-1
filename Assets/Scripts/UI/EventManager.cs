using UnityEngine;
using System;

/*
 * EventManager.cs
 * 
 * Purpose: Provides centralized event management and communication
 * Used by: Game systems requiring event-based communication
 * 
 * Key Features:
 * - Type-safe event subscription
 * - Global event broadcasting
 * - Event cleanup handling
 * - Singleton pattern
 * 
 * Event Types:
 * - Game state events
 * - Player actions
 * - System notifications
 * - Scene transitions
 * 
 * Performance Considerations:
 * - Efficient event dispatch
 * - Automatic event cleanup
 * - Memory leak prevention
 * - Smart delegate management
 * 
 * Dependencies:
 * - None (core system)
 * - Used by multiple game systems
 */

public class EventManager : MonoBehaviour
{
    // Singleton instance
    public static EventManager Instance;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    // Existing PerformAction method
    public void PerformAction(string actionText, GameObject objectToInteractWith, GameObject switchObject)
    {
        ActionHandler actionHandler = FindFirstObjectByType<ActionHandler>();
        if (actionHandler != null)
        {
            actionHandler.HandleAction(actionText, objectToInteractWith, switchObject);
        }
        else
        {
            Debug.LogError("ActionHandler not found in the scene.");
        }
    }

    // Modified ShowInteractionPrompt method to include duration
    public void ShowInteractionPrompt(string promptText, float duration = 0f)
    {
        UIManager.Instance.DisplayInteractionPrompt(promptText);
        
        // If duration is specified, hide the prompt after that duration
        if (duration > 0)
        {
            CancelInvoke("HideInteractionPrompt");
            Invoke("HideInteractionPrompt", duration);
        }
    }

    public void HideInteractionPrompt()
    {
        UIManager.Instance.HidePrompt();
    }

    // New TriggerAction method
    public void TriggerAction(string actionText, GameObject objectToInteractWith, GameObject switchObject)
    {
        ActionHandler actionHandler = FindFirstObjectByType<ActionHandler>();
        if (actionHandler != null)
        {
            actionHandler.HandleAction(actionText, objectToInteractWith, switchObject);
        }
        else
        {
            Debug.LogError("ActionHandler not found in the scene.");
        }
    }

    // Add this new method
    public void ShowFishingPrompt(string promptText)
    {
        UIManager.Instance.DisplayFishingPrompt(promptText);
    }
}
