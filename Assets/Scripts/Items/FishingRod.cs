using UnityEngine;
using System.Collections;

public class FishingRod : Item
{
    [Header("Position Settings")]
    [SerializeField] private Vector3 raisedPosition;
    [SerializeField] private Vector3 raisedRotation;
    [SerializeField] private Vector3 loweredPositionOffset;
    [SerializeField] private Vector3 loweredRotationOffset;
    
    [Header("Movement Speeds")]
    [Tooltip("How fast the fishing rod moves when being equipped")]
    [SerializeField] private float raiseSpeed = 8f;
    [Tooltip("How fast the fishing rod moves when being unequipped")]
    [SerializeField] private float lowerSpeed = 6f;

    [Header("Fishing Settings")]
    [SerializeField] private float minFishingTime = 5f;
    [SerializeField] private float maxFishingTime = 15f;
    [SerializeField] private AudioClip fishingStartSound;
    [SerializeField] private AudioClip fishCaughtSound;

    [Header("Fish Settings")]
    [SerializeField] private string fishName = "Fresh Fish";
    [SerializeField] private string fishDescription = "A freshly caught fish. Can be eaten to restore hunger.";
    [SerializeField] private Sprite fishIcon;
    [SerializeField] private float fishNutritionValue = 25f;

    [Header("UI Messages")]
    [SerializeField] private string startFishingMessage = "Fishing...";
    [SerializeField] private string stopFishingMessage = "Stopped fishing";
    [SerializeField] private string fishCaughtMessage = "Fish caught!";
    [SerializeField] private string inventoryFullMessage = "Inventory is full!";

    private bool isRaised = false;
    private bool isFishing = false;
    private bool isTransitioning = false;
    private Coroutine fishingCoroutine;

    private float m_FishingSpotMultiplier = 1f;

    public void SetFishingSpotMultiplier(float _multiplier)
    {
        m_FishingSpotMultiplier = Mathf.Max(0.1f, _multiplier);
    }

    private void Start()
    {
        // Ensure we start in lowered position
        transform.localPosition = raisedPosition + loweredPositionOffset;
        transform.localRotation = Quaternion.Euler(raisedRotation + loweredRotationOffset);

        if (itemIcon == null)
        {
            //Debug.LogError($"No icon assigned for {itemName} in FishingRodItem!");
        }
    }

    public void MoveToRaisedPosition()
    {
        if (isTransitioning) return;
        
       // Debug.Log("Moving fishing rod to raised position");
        isRaised = true;
        isTransitioning = true;
        StartCoroutine(TransitionPosition(true));
    }

    public void MoveToLoweredPosition()
    {
        if (isTransitioning) return;
        
        //Debug.Log("Moving fishing rod to lowered position");
        isRaised = false;
        isTransitioning = true;
        StopFishing(); // Make sure to stop fishing when lowering
        StartCoroutine(TransitionPosition(false));
    }

    public bool IsRaised()
    {
        return isRaised;
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    private IEnumerator TransitionPosition(bool raising)
    {
        Vector3 targetPos = raising ? raisedPosition : raisedPosition + loweredPositionOffset;
        Vector3 targetRot = raising ? raisedRotation : raisedRotation + loweredRotationOffset;
        
        float elapsedTime = 0f;
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        
        // Use different speeds for raising and lowering
        float currentSpeed = raising ? raiseSpeed : lowerSpeed;
        
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * currentSpeed;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsedTime);
            transform.localRotation = Quaternion.Lerp(startRot, Quaternion.Euler(targetRot), elapsedTime);
            yield return null;
        }

        // Ensure we reach the exact target position/rotation
        transform.localPosition = targetPos;
        transform.localRotation = Quaternion.Euler(targetRot);
        
        isTransitioning = false;
       // Debug.Log($"Transition complete. IsRaised: {isRaised}");
    }

    public override void UseItem()
    {
        if (!isRaised)
        {
            MoveToRaisedPosition();
        }
    }

    public void StartFishing()
    {
        if (!isFishing && isRaised)
        {
            isFishing = true;
            if (UIAudioManager.Instance != null)
            {
                UIAudioManager.Instance.PlayOneShot(fishingStartSound);
            }
            fishingCoroutine = StartCoroutine(FishingProcess());
            EventManager.Instance.ShowFishingPrompt(startFishingMessage);
        }
    }

    public void StopFishing()
    {
        if (isFishing)
        {
            isFishing = false;
            if (fishingCoroutine != null)
            {
                StopCoroutine(fishingCoroutine);
                fishingCoroutine = null;
            }
            EventManager.Instance.ShowFishingPrompt(stopFishingMessage);
            
            // Hide the message after a short delay
            StartCoroutine(HidePromptAfterDelay(1.5f));
        }
    }

    private IEnumerator HidePromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EventManager.Instance.HideInteractionPrompt();
    }

    private IEnumerator FishingProcess()
    {
        // Apply the spot multiplier to the fishing time
        float baseTime = Random.Range(minFishingTime, maxFishingTime);
        float adjustedFishingTime = baseTime * m_FishingSpotMultiplier;
        float elapsedTime = 0f;

        while (elapsedTime < adjustedFishingTime)
        {
            if (!isFishing)
            {
                EventManager.Instance.ShowFishingPrompt(stopFishingMessage);
                StartCoroutine(HidePromptAfterDelay(1.5f));
                yield break;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Fish caught!
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayOneShot(fishCaughtSound);
        }

        // Add fish to inventory
        if (Inventory.Instance != null)
        {
            GameObject fishObj = new GameObject("Fish");
            Fish fishItem = fishObj.AddComponent<Fish>();
            fishItem.InitializeFish(fishName, fishDescription, fishIcon, fishNutritionValue);

            if (!Inventory.Instance.AddItem(fishItem))
            {
                Destroy(fishObj);
                EventManager.Instance.ShowFishingPrompt(inventoryFullMessage);
            }
            else
            {
                EventManager.Instance.ShowFishingPrompt(fishCaughtMessage);
            }
        }

        isFishing = false;
        yield return new WaitForSeconds(2f);
        if (!isFishing) // Only hide if we haven't started fishing again
        {
            EventManager.Instance.HideInteractionPrompt();
        }
    }

    public bool IsFishing()
    {
        return isFishing;
    }
} 