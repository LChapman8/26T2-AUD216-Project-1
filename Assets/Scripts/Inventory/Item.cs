using UnityEngine;

// Change from abstract to regular class
public class Item : MonoBehaviour
{
    [SerializeField] protected string itemName;
    [SerializeField] protected Sprite itemIcon;
    [SerializeField] protected string itemDescription;
    
    [Header("Objective")]
    [SerializeField] protected string objectiveId;
    
    protected virtual void Awake()
    {
        // Only show warning if item has a name but no icon
        if (itemIcon == null && !string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning($"No icon assigned for item: {itemName}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory inventory = other.GetComponent<Inventory>();
            if (inventory != null && inventory.AddItem(this))
            {
                // Play pickup sound
                if (UIAudioManager.Instance != null)
                {
                    UIAudioManager.Instance.PlayPickupSound();
                }
                
                OnPickup();
                
                // Check if this item completes an objective
                if (!string.IsNullOrEmpty(objectiveId) && ObjectiveManager.Instance != null)
                {
                    try
                    {
                        ObjectiveManager.Instance.CompleteObjective(objectiveId);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to complete objective {objectiveId}: {e.Message}");
                    }
                }
            }
        }
    }

    // Change from abstract to virtual
    public virtual void UseItem()
    {
        // Default implementation does nothing
    }

    protected virtual void OnPickup()
    {
        // Don't disable the whole GameObject, just the renderer and collider
        if (GetComponent<Collider>() != null)
            GetComponent<Collider>().enabled = false;
        if (GetComponent<Renderer>() != null)
            GetComponent<Renderer>().enabled = false;
        
        // If there are child renderers, disable them too
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
    }

    public virtual string GetItemName() => itemName;
    public Sprite GetItemIcon() 
    {
        // Remove the warning here since we already check in Awake
        return itemIcon;
    }
    public string GetItemDescription() => itemDescription;
} 