using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Hunger Settings")]
    [SerializeField] private Slider hungerBar;
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float hungerDecreaseRate = 1.667f; // Will take 60 seconds to go from 100 to 0
    
    [Header("Health Settings")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float damageAmount = 10f;  // Amount of damage when debug key is pressed
    [Tooltip("Key used to test damage in play mode")]
    [SerializeField] private KeyCode debugDamageKey = KeyCode.P;

    [Header("Stamina Settings")]
    [SerializeField] private Slider staminaBar;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaIncreaseRate = 10f;
    [SerializeField] private float staminaRequiredForSpecialMoves = 50f;
    [SerializeField] private float staminaCostDoubleJump = 30f;
    [SerializeField] private float staminaCostSlide = 40f;
    [SerializeField] private float sprintStaminaCostPerSecond = 15f;

    [Header("Damage Feedback")]
    [SerializeField] private ParticleSystem damageParticles;
    [SerializeField] private Animator damageAnimator;
    [SerializeField] private string damageAnimationTrigger = "damageAnimationTrigger";
    [SerializeField] private GameObject damageEffectRoot;
    [Tooltip("Vignette effect for damage and low health indication")]
    [SerializeField] private Animator vignetteAnimator;
    [Tooltip("Health threshold for triggering hit effects (default 50)")]
    [SerializeField] private float midHealthThreshold = 50f;
    [Tooltip("Health threshold to activate constant vignette effect (default 25)")]
    [SerializeField] private float lowHealthThreshold = 25f;

    [Header("XP Settings")]
    [SerializeField] private Slider xpBar;
    [SerializeField] private float baseXpToLevelUp = 100f;
    [SerializeField] private float xpScalingFactor = 1.5f;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Level Up Animation")]
    [SerializeField] private GameObject levelUpAnimatorPrefab;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private string levelNumberTextPath = "LevelBox/Label_PlayerLevel";

    [Header("XP Gain Animation")]
    [SerializeField] private GameObject xpGainAnimatorPrefab;
    [SerializeField] private Transform xpGainParent;
    [SerializeField] private string xpGainTextPath = "Content/HUD_XPLog_Item/Content/Text";
    [SerializeField] private float xpGainAnimationDuration = 2f;

    [Header("Death Settings")]
    [SerializeField] private float deathDelay = 2f;

    [Header("Audio")]
    [SerializeField] private PlayerAudioManager playerAudioManager;

    private float currentHunger;
    private float currentHealth;
    private float currentStamina;
    private bool isSprinting = false;
    private float currentXp;
    private int currentLevel = 1;
    private float xpToNextLevel;
    private bool isDead = false;
    private bool isLowHealthEffectActive = false;
    private static readonly int ActiveParameter = Animator.StringToHash("Active");
    private static readonly int HitParameter = Animator.StringToHash("Hit");

    private float lastHungerUpdate = 0f;
    private float hungerUpdateInterval = 0.1f; // Update hunger every 100ms instead of every frame
    private float lastStaminaUpdate = 0f;
    private float staminaUpdateInterval = 0.05f; // Update stamina every 50ms
    
    private Rigidbody playerRigidbody;

    private void Awake()
    {
        // Ensure vignette starts inactive
        if (vignetteAnimator != null)
        {
            vignetteAnimator.SetBool(ActiveParameter, false);
            isLowHealthEffectActive = false;
        }
    }

    private void Start()
    {
        InitializeStats();
        InitializeUI();
        InitializeXPSystem();
        UpdateLowHealthEffect();
        
        // Cache component reference
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void InitializeStats()
    {
        currentHunger = maxHunger;
        currentHealth = maxHealth;
        currentStamina = 0f;
    }

    private void InitializeUI()
    {
        if (hungerBar != null)
        {
            hungerBar.maxValue = maxHunger;
            hungerBar.value = currentHunger;
        }
        
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }

        if (damageEffectRoot != null)
        {
            damageEffectRoot.SetActive(false);
        }
    }

    private void InitializeXPSystem()
    {
        currentXp = 0f;
        xpToNextLevel = baseXpToLevelUp;
        
        if (xpBar != null)
        {
            xpBar.maxValue = xpToNextLevel;
            xpBar.value = currentXp;
        }

        UpdateLevelText();
    }

    private void Update()
    {
        if (isDead) return;

        float currentTime = Time.time;

        // Update hunger at intervals instead of every frame
        if (currentTime >= lastHungerUpdate + hungerUpdateInterval)
        {
            HandleHunger();
            lastHungerUpdate = currentTime;
        }

        // Update stamina at intervals instead of every frame
        if (currentTime >= lastStaminaUpdate + staminaUpdateInterval)
        {
            HandleStamina();
            lastStaminaUpdate = currentTime;
        }

        // Check health/hunger death condition less frequently
        if (currentHealth <= 0 || currentHunger <= 0)
        {
            Die();
        }

        // Keep debug input check
        if (Input.GetKeyDown(debugDamageKey))
        {
            TakeDamage(damageAmount);
        }
    }

    private void HandleHunger()
    {
        if (currentHunger <= 0) return;

        currentHunger -= hungerDecreaseRate * hungerUpdateInterval;
        currentHunger = Mathf.Max(0, currentHunger);
        
        if (hungerBar != null)
        {
            hungerBar.value = currentHunger;
        }
    }

    private void HandleStamina()
    {
        float staminaChange;
        
        if (isSprinting)
        {
            staminaChange = -sprintStaminaCostPerSecond * staminaUpdateInterval;
        }
        else if (currentStamina < maxStamina)
        {
            staminaChange = staminaIncreaseRate * staminaUpdateInterval;
        }
        else
        {
            return; // No change needed
        }

        currentStamina = Mathf.Clamp(currentStamina + staminaChange, 0, maxStamina);
        
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        DisablePlayerControls();
        ShowDeathEffects();
        AnimationEventHandler.Instance?.ShowDeathAnnouncement();
        StartCoroutine(QuitGameSequence());
    }

    private void ShowDeathEffects()
    {
        if (damageEffectRoot != null)
        {
            damageEffectRoot.SetActive(true);
        }

        if (damageParticles != null)
        {
            damageParticles.Play();
        }

        if (damageAnimator != null)
        {
            damageAnimator.SetTrigger(damageAnimationTrigger);
        }
    }

    private void DisablePlayerControls()
    {
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
        }
    }

    private IEnumerator QuitGameSequence()
    {
        yield return new WaitForSeconds(deathDelay);

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (playerAudioManager != null)
        {
            playerAudioManager.PlayDamageSound();
        }

        UpdateLowHealthEffect();  // Update low health state first
        ShowDamageEffects();     // Then show hit effects

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ShowDamageEffects()
    {
        if (damageEffectRoot != null)
        {
            damageEffectRoot.SetActive(true);
            StartCoroutine(HideDamageEffect());
        }

        if (damageParticles != null)
        {
            damageParticles.Play();
        }

        if (damageAnimator != null)
        {
            damageAnimator.SetTrigger(damageAnimationTrigger);
        }

        // Only trigger hit effect when health is below midHealthThreshold
        if (vignetteAnimator != null && currentHealth <= midHealthThreshold)
        {
            vignetteAnimator.SetTrigger(HitParameter);
//            Debug.Log($"Triggering hit effect at {currentHealth:F1} health (below mid threshold of {midHealthThreshold})");
        }
    }

    private IEnumerator HideDamageEffect()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (damageEffectRoot != null)
        {
            damageEffectRoot.SetActive(false);
        }
    }

    public void AddXP(float amount)
    {
        if (isDead) return;
        
        bool isFirstXP = currentXp <= 0;
        currentXp += amount;
        
        ShowXPGainAnimation(amount);
        
        while (currentXp >= xpToNextLevel)
        {
            float excess = currentXp - xpToNextLevel;
            LevelUp();
            currentXp = excess;
        }

        if (xpBar != null)
        {
            xpBar.value = currentXp;
        }

        if (isFirstXP)
        {
            UpdateLevelText();
        }
    }

    private void ShowXPGainAnimation(float amount)
    {
        if (xpGainAnimatorPrefab == null) return;

        Transform parent = xpGainParent != null ? xpGainParent : targetCanvas?.transform;
        if (parent == null)
        {
            targetCanvas = FindFirstObjectByType<Canvas>();
            if (targetCanvas == null) return;
            parent = targetCanvas.transform;
        }

        GameObject xpGainInstance = Instantiate(xpGainAnimatorPrefab, parent);
        
        // Set XP text
        Transform xpGainTextTransform = xpGainInstance.transform.Find(xpGainTextPath);
        if (xpGainTextTransform != null)
        {
            if (xpGainTextTransform.TryGetComponent<TextMeshProUGUI>(out var xpGainText))
            {
                xpGainText.text = $"+{amount} XP";
            }
        }

        // Play animation
        if (xpGainInstance.TryGetComponent<Animator>(out var animator))
        {
            int layerIndex = animator.GetLayerIndex("Base Layer");
            if (layerIndex != -1 && animator.HasState(layerIndex, Animator.StringToHash("ANIM_HUD_Event_XPGain_In")))
            {
                animator.Play("ANIM_HUD_Event_XPGain_In", layerIndex);
            }
        }

        Destroy(xpGainInstance, xpGainAnimationDuration);
    }

    private void LevelUp()
    {
        currentLevel++;
        xpToNextLevel *= xpScalingFactor;
        
        if (xpBar != null)
        {
            xpBar.maxValue = xpToNextLevel;
        }

        UpdateLevelText();
        ShowLevelUpAnimation();
    }

    private void ShowLevelUpAnimation()
    {
        if (levelUpAnimatorPrefab == null || targetCanvas == null) return;

        GameObject levelUpInstance = Instantiate(levelUpAnimatorPrefab, targetCanvas.transform);
        
        if (levelUpInstance.TryGetComponent<RectTransform>(out var rectTransform))
        {
            rectTransform.localScale = Vector3.one;
        }

        // Set level text
        Transform levelTextTransform = levelUpInstance.transform.Find(levelNumberTextPath);
        if (levelTextTransform != null)
        {
            if (levelTextTransform.TryGetComponent<TextMeshProUGUI>(out var levelNumberText))
            {
                levelNumberText.text = currentLevel.ToString();
            }
        }

        // Play animation
        if (levelUpInstance.TryGetComponent<Animator>(out var animator))
        {
            int layerIndex = animator.GetLayerIndex("Base Layer");
            if (layerIndex != -1 && animator.HasState(layerIndex, Animator.StringToHash("ANIM_HUD_Event_LevelUp_In")))
            {
                animator.Play("ANIM_HUD_Event_LevelUp_In", layerIndex);
            }
        }

        Destroy(levelUpInstance, 5f);
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = currentXp <= 0 ? "XP" : currentLevel.ToString();
        }
    }

    // Public methods for external access
    public void AddHealth(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
        
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayMedicineUsedSound();
        }
        
        UpdateLowHealthEffect();
    }

    public void AddHunger(float amount)
    {
        if (isDead) return;
        currentHunger = Mathf.Min(currentHunger + amount, maxHunger);
        if (hungerBar != null)
        {
            hungerBar.value = currentHunger;
        }
    }

    public bool CanUseSpecialMove(float staminaCost)
    {
        float currentStaminaPercentage = (currentStamina / maxStamina) * 100f;
        return currentStaminaPercentage >= staminaRequiredForSpecialMoves && currentStamina >= staminaCost;
    }

    public bool UseStaminaForDoubleJump()
    {
        if (CanUseSpecialMove(staminaCostDoubleJump))
        {
            currentStamina -= staminaCostDoubleJump;
            if (staminaBar != null)
            {
                staminaBar.value = currentStamina;
            }
            return true;
        }
        return false;
    }

    public bool UseStaminaForSlide()
    {
        if (CanUseSpecialMove(staminaCostSlide))
        {
            currentStamina -= staminaCostSlide;
            if (staminaBar != null)
            {
                staminaBar.value = currentStamina;
            }
            return true;
        }
        return false;
    }

    public void SetSprinting(bool sprinting)
    {
        if (isDead) return;
        isSprinting = sprinting;
    }

    // Getter methods
    public float GetCurrentHunger() => currentHunger;
    public float GetCurrentHealth() => currentHealth;
    public bool HasStaminaForSprinting() => currentStamina > 0;
    public int GetCurrentLevel() => currentLevel;
    public float GetCurrentXP() => currentXp;
    public float GetXPToNextLevel() => xpToNextLevel;
    public bool IsDead() => isDead;

    private void UpdateLowHealthEffect()
    {
        if (vignetteAnimator == null) return;

        bool shouldBeLowHealth = currentHealth <= lowHealthThreshold;
        
        // Force update the state even if it hasn't changed
        isLowHealthEffectActive = shouldBeLowHealth;
        vignetteAnimator.SetBool(ActiveParameter, isLowHealthEffectActive);

        if (playerAudioManager != null)
        {
            playerAudioManager.UpdateLowHealthState(isLowHealthEffectActive);
        }
    }

    private void OnValidate()
    {
        if (lowHealthThreshold > midHealthThreshold)
        {
      //      Debug.LogWarning("Low health threshold should not be higher than mid health threshold. Adjusting...");
            lowHealthThreshold = midHealthThreshold;
        }
        
        if (midHealthThreshold > maxHealth)
        {
        //    Debug.LogWarning("Mid health threshold should not be higher than max health. Adjusting...");
            midHealthThreshold = maxHealth;
        }
    }
}