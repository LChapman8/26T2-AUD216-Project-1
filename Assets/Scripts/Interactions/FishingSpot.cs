using UnityEngine;

public class FishingSpot : MonoBehaviour
{
    [SerializeField] private string objectDisplayName = "Fishing Spot";
    [SerializeField] private string inputKey = "E";
    [SerializeField] private string actionMessage = "fish";
    [SerializeField] private InteractionPromptUI promptUI;

    private bool playerInRange = false;
    private bool isFishing = false;
    private FishingRod activeFishingRod;

    private void Start()
    {
        if (promptUI == null)
        {
            promptUI = FindFirstObjectByType<InteractionPromptUI>();
        }
    }

    private void Update()
    {
        if (playerInRange && activeFishingRod != null)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!isFishing)
                {
                    StartFishing();
                }
            }
            else if (isFishing)
            {
                StopFishing();
            }
        }
    }

    private void StartFishing()
    {
        isFishing = true;
        activeFishingRod.StartFishing();
        if (promptUI != null)
        {
            promptUI.ShowPrompt(objectDisplayName, inputKey, "keep fishing...");
        }
    }

    private void StopFishing()
    {
        isFishing = false;
        activeFishingRod.StopFishing();
        if (promptUI != null)
        {
            promptUI.ShowPrompt(objectDisplayName, inputKey, actionMessage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if player has fishing rod equipped
            Inventory inventory = other.GetComponent<Inventory>();
            if (inventory != null)
            {
                activeFishingRod = inventory.GetEquippedItem()?.GetComponent<FishingRod>();
                if (activeFishingRod != null)
                {
                    playerInRange = true;
                    if (promptUI != null)
                    {
                        promptUI.ShowPrompt(objectDisplayName, inputKey, actionMessage);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (isFishing)
            {
                StopFishing();
            }
            activeFishingRod = null;
            if (promptUI != null)
            {
                promptUI.HidePrompt();
            }
        }
    }
} 