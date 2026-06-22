using UnityEngine;
using System.Collections;

public class MedicineItem : Item
{
    [SerializeField] private float healAmount = 25f;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private ParticleSystem healingEffect;

    private void Start()
    {
        if (playerAnimator == null)
        {
            playerAnimator = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
        }
    }

    public override void UseItem()
    {
        //Debug.Log($"Using medicine item: {itemName}");

        if (playerAnimator != null)
        {
            playerAnimator.Play("Heal");
        }

        if (healingEffect != null)
        {
            healingEffect.Play();
        }

        // Play medicine used sound
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayMedicineUsedSound();
        }

        PlayerStats playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.AddHealth(healAmount);
            
            if (Inventory.Instance != null)
            {
                int slot = Inventory.Instance.GetSlotForItem(this);
                //Debug.Log($"Found medicine in slot: {slot}");
                if (slot != -1)
                {
                    //Debug.Log("Removing medicine item from inventory");
                    Inventory.Instance.RemoveItem(slot);
                    Destroy(gameObject);
                }
            }
            //Debug.LogError("Could not find Inventory instance!");
        }
        //Debug.LogError("Could not find PlayerStats component!");
    }

    private IEnumerator RemoveAfterEffects()
    {
        // Wait for animation if it exists
        if (playerAnimator != null)
        {
            yield return new WaitForSeconds(playerAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        // Wait for particles if they exist
        if (healingEffect != null)
        {
            yield return new WaitForSeconds(healingEffect.main.duration);
        }

        RemoveFromInventory();
    }

    private void RemoveFromInventory()
    {
        Inventory inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        if (inventory != null)
        {
            int slot = inventory.GetSlotForItem(this);
            if (slot != -1)
            {
                inventory.RemoveItem(slot);
                Destroy(gameObject);
            }
        }
    }
} 