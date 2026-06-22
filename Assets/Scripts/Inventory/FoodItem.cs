using UnityEngine;

public class FoodItem : Item
{
    [SerializeField] protected float nutritionValue = 15f;

    public override void UseItem()
    {
        //Debug.Log($"Using food item: {itemName}");
        
        PlayerStats playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.AddHunger(nutritionValue);
            
            // Play appropriate food consumption sound
            if (UIAudioManager.Instance != null)
            {
                if (itemName == "Food Can")
                {
                    UIAudioManager.Instance.PlayFoodCanEatenSound();
                }
            }
            
            if (Inventory.Instance != null)
            {
                int slot = Inventory.Instance.GetSlotForItem(this);
                //Debug.Log($"Found item in slot: {slot}");
                if (slot != -1)
                {
                    //Debug.Log("Removing food item from inventory");
                    Inventory.Instance.RemoveItem(slot);
                    Destroy(gameObject);
                }
            }
            //Debug.LogError("Could not find Inventory instance!");
        }
        //Debug.LogError("Could not find PlayerStats component!");
    }
} 