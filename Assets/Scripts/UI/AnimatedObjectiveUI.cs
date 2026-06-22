using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Core.Pooling;

/*
 * AnimatedObjectiveUI.cs
 * 
 * Purpose: Handles the display and animation of individual objective UI elements.
 * Used by: UIPoolManager, ObjectiveManager
 * 
 * Controls the lifecycle of objective notifications, including initialization,
 * animation states, and return to pool. Works with the UI pooling system to
 * efficiently manage objective display resources.
 * 
 * Dependencies:
 * - UIPoolManager (for pooling)
 * - Requires animator with specific animation states
 * - ObjectiveData (for objective information)
 * - TextMeshPro for text display
 */

public class AnimatedObjectiveUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Animator animator;

    private static readonly string ANIM_IN = "ANIM_HUD_ObjectiveItem_In";
    private static readonly string ANIM_OUT = "ANIM_HUD_ObjectiveItem_Out";
    private static readonly string ANIM_ACTIVE = "ANIM_HUD_ObjectiveItem_Active";
    private static readonly string ANIM_INACTIVE = "ANIM_HUD_ObjectiveItem_Inactive";

    private const string POOL_NAME = "ObjectiveUI";
    private bool isReturningToPool = false;

    public void Initialize(ObjectiveData objectiveData)
    {
        isReturningToPool = false;
        
        if (animator == null)
            animator = GetComponent<Animator>();

        if (descriptionText != null)
            descriptionText.text = objectiveData.description;
        
        if (iconImage != null && objectiveData.icon != null)
            iconImage.sprite = objectiveData.icon;

        PlayAnimation(ANIM_IN);
    }

    public void CompleteObjective()
    {
        if (!isReturningToPool)
        {
            isReturningToPool = true;
            StartCoroutine(CompleteAndReturn());
        }
    }

    private IEnumerator CompleteAndReturn()
    {
        PlayAnimation(ANIM_INACTIVE);
        yield return new WaitForSeconds(1f);
        
        PlayAnimation(ANIM_OUT);
        yield return new WaitForSeconds(0.5f);
        
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (UIPoolManager.Instance != null)
        {
            UIPoolManager.Instance.Return(POOL_NAME, this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }
    }

    public void SetActive(bool active)
    {
        PlayAnimation(active ? ANIM_ACTIVE : ANIM_INACTIVE);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isReturningToPool = false;
    }
} 