using UnityEngine;
using System.Collections.Generic;

public class Fish : FoodItem
{
    private static Dictionary<string, int> fishCaughtPerObjective = new Dictionary<string, int>();
    private bool isFirstFish = true;
    [SerializeField] private float xpReward = 15f;
    [SerializeField] private float healthRestoreAmount = 10f;

    private static readonly Dictionary<string, int> fishRequirements = new Dictionary<string, int>
    {
        { "CatchTwoFishes", 2 },
        { "CatchThreeFishes", 3 },
        { "CatchFourFishes", 4 },
        { "CatchFiveFishes", 5 }
    };

    public void InitializeFish(string name, string description, Sprite icon, float nutrition)
    {
        if (string.IsNullOrEmpty(name) || icon == null)
        {
            return;
        }

        itemName = name;
        itemDescription = description;
        itemIcon = icon;
        nutritionValue = nutrition;

        // Check active objectives and increment their counters
        CheckFishObjectives();

        // Award XP when fish is caught
        PlayerStats playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.AddXP(xpReward);
        }

        if (isFirstFish && FirstTimeInteractionTracker.Instance != null)
        {
            FirstTimeInteractionTracker.Instance.OnFirstFishCaught();
            isFirstFish = false;
        }
    }

    private void CheckFishObjectives()
    {
        if (ObjectiveManager.Instance != null)
        {
            foreach (var requirement in fishRequirements)
            {
                CheckAndIncrementObjective(requirement.Key, requirement.Value);
            }
        }
    }

    private void CheckAndIncrementObjective(string objectiveId, int requiredFish)
    {
        // Only process if the objective is currently active
        if (ObjectiveManager.Instance.IsObjectiveActive(objectiveId))
        {
            // Initialize counter if needed
            if (!fishCaughtPerObjective.ContainsKey(objectiveId))
            {
                fishCaughtPerObjective[objectiveId] = 0;
            }

            // Increment fish count for this objective
            fishCaughtPerObjective[objectiveId]++;

            // Complete objective if target reached
            if (fishCaughtPerObjective[objectiveId] >= requiredFish)
            {
                ObjectiveManager.Instance.CompleteObjective(objectiveId);
            }
        }
    }

    // Reset method for when objectives become active or day changes
    public static void ResetObjectiveFishCount(string objectiveId)
    {
        fishCaughtPerObjective.Remove(objectiveId);
    }

    // Reset all counters (useful for day changes)
    public static void ResetAllFishCounts()
    {
        fishCaughtPerObjective.Clear();
    }

    public override void UseItem()
    {
        PlayerStats playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.AddHunger(nutritionValue);
            playerStats.AddHealth(healthRestoreAmount);
            
            if (UIAudioManager.Instance != null)
            {
                UIAudioManager.Instance.PlayFishEatenSound();
            }
            
            if (Inventory.Instance != null)
            {
                int slot = Inventory.Instance.GetSlotForItem(this);
                if (slot != -1)
                {
                    Inventory.Instance.RemoveItem(slot);
                    Destroy(gameObject);
                }
            }
        }
    }
} 