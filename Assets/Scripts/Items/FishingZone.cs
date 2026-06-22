using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class FishingZone : MonoBehaviour
{
    [Header("Fishing Quality Settings")]
    [Tooltip("Multiplier for fishing time (0.5 = half time, 2 = double time)")]
    [SerializeField, Range(0.1f, 2f)] private float fishingTimeMultiplier = 1f;
    
    #pragma warning disable 0414
    [Tooltip("Description of this fishing spot's quality")]
    [SerializeField] private string spotQualityDescription = "Average fishing spot";
    
    [Header("Time Settings")]
    [SerializeField] private float fishingStartTime = 0.3f;
    [SerializeField] private float fishingEndTime = 0.7f;
    #pragma warning restore 0414
    
    [Header("UI Settings")]
    [SerializeField] private string objectDisplayName = "Fishing Spot";
    [SerializeField] private string inputKey = "LMB";
    [SerializeField] private string actionMessage = "hold to fish";
    [SerializeField] private InteractionPromptUI promptUI;

    private bool playerInRange = false;
    private FishingRod activeFishingRod = null;
    private FirstPersonController playerController = null;

    private void Start()
    {
        if (promptUI == null)
        {
            promptUI = FindFirstObjectByType<InteractionPromptUI>();
            if (promptUI == null)
            {
                Debug.LogError("InteractionPromptUI not found. Please assign it in the inspector.");
            }
        }
    }

    private void Update()
    {
        if (playerInRange)
        {
            // Check for fishing rod state changes
            if (playerController != null)
            {
                foreach (var item in playerController.GetInventory().GetAllItems())
                {
                    if (item is FishingRodItem fishingRodItem)
                    {
                        FishingRod newRod = fishingRodItem.GetFishingRodController();
                        if (newRod != activeFishingRod)
                        {
                            activeFishingRod = newRod;
                            UpdatePrompt();
                        }
                        break;
                    }
                }
            }

            if (activeFishingRod != null && activeFishingRod.IsRaised())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (!activeFishingRod.IsFishing())
                    {
                        activeFishingRod.StartFishing();
                    }
                }
                else if (Input.GetMouseButtonUp(0) || !playerInRange)
                {
                    if (activeFishingRod.IsFishing())
                    {
                        activeFishingRod.StopFishing();
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerController = other.GetComponent<FirstPersonController>();
            if (playerController != null)
            {
                foreach (var item in playerController.GetInventory().GetAllItems())
                {
                    if (item is FishingRodItem fishingRodItem)
                    {
                        FishingRod rod = fishingRodItem.GetFishingRodController();
                        if (rod != null)
                        {
                            // Apply the fishing spot's quality settings to the rod
                            rod.SetFishingSpotMultiplier(fishingTimeMultiplier);
                            activeFishingRod = rod;
                        }
                        break;
                    }
                }
            }
            UpdatePrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (activeFishingRod != null)
            {
                if (activeFishingRod.IsFishing())
                {
                    activeFishingRod.StopFishing();
                }
                // Reset the rod's multiplier when leaving the zone
                activeFishingRod.SetFishingSpotMultiplier(1f);
            }

            playerInRange = false;
            activeFishingRod = null;
            playerController = null;
            
            if (promptUI != null)
            {
                promptUI.HidePrompt();
            }
        }
    }

    private void UpdatePrompt()
    {
        if (!playerInRange || promptUI == null)
        {
            return;
        }

        if (activeFishingRod != null && activeFishingRod.IsRaised())
        {
            promptUI.ShowPrompt(objectDisplayName, inputKey, actionMessage);
        }
        else
        {
            promptUI.HidePrompt();
        }
    }
} 