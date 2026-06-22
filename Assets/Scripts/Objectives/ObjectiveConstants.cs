using UnityEngine;

/*
 * ObjectiveConstants.cs
 * 
 * Purpose: Centralizes objective identifiers for consistent reference
 * Used by: Objective system, mission management
 * 
 * Key Features:
 * - Constant string definitions
 * - Shared objective identifiers
 * - Type-safe objective references
 * 
 * Usage:
 * - Day/night objective identification
 * - Mission system integration
 * - Event system references
 * 
 * Dependencies:
 * - Used by ObjectiveManager
 * - Used by DailyObjectiveManager
 * - Mission system integration
 */

public static class ObjectiveConstants
{
    // Day Objectives
    public const string PICKUP_FISHING_ROD = "PickupFishingRod";
    public const string CATCH_FISH = "CatchAFish";
    
    // Night Objectives
    public const string ZOMBIE_DEFENCE = "ZombieDefence";
} 