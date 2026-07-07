using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Hunger Settings")]
    [SerializeField] private Slider hungerBar;
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float hungerDecreaseRate = 1.667f;
    [SerializeField] private float lowHungerThreshold = 25f;

    [Header("Health Settings")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float damageAmount = 10f;
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
    [SerializeField] private Animator vignetteAnimator;
    [SerializeField] private float midHealthThreshold = 50f;
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
    private bool isLowHungerAudioActive = false;
    private bool wasStaminaEmpty = false;
    private bool wasStaminaRegenerating = false;
    private bool isStaminaLocked = false;

    [Header("Stamina Exhaustion")]
    [SerializeField] private float staminaUnlockPercent = 10f;

    private static readonly int ActiveParameter = Animator.StringToHash("Active");
    private static readonly int HitParameter = Animator.StringToHash("Hit");

    private float lastHungerUpdate = 0f;
    private float hungerUpdateInterval = 0.1f;
    private float lastStaminaUpdate = 0f;
    private float staminaUpdateInterval = 0.05f;

    private Rigidbody playerRigidbody;

    private void Awake()
    {
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

        if (currentTime >= lastHungerUpdate + hungerUpdateInterval)
        {
            HandleHunger();
            lastHungerUpdate = currentTime;
        }

        if (currentTime >= lastStaminaUpdate + staminaUpdateInterval)
        {
            HandleStamina();
            lastStaminaUpdate = currentTime;
        }

        if (currentHealth <= 0 || currentHunger <= 0)
        {
            Die();
        }

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

        bool isLowHunger = currentHunger <= lowHungerThreshold;

        if (isLowHunger && !isLowHungerAudioActive)
        {
            if (playerAudioManager != null)
            {
                playerAudioManager.PlayHungerSound();
            }

            isLowHungerAudioActive = true;
        }
        else if (!isLowHunger)
        {
            isLowHungerAudioActive = false;
        }
    }

    private void HandleStamina()
    {
        float previousStamina = currentStamina;
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
            wasStaminaRegenerating = false;
            return;
        }

        currentStamina = Mathf.Clamp(currentStamina + staminaChange, 0, maxStamina);

        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
        }

        bool isStaminaEmpty = currentStamina <= 0f;

        if (isStaminaEmpty && !isStaminaLocked)
        {
            isStaminaLocked = true;

            if (playerAudioManager != null)
            {
                playerAudioManager.PlayStaminaDepletedSound();
            }
        }

        float unlockStaminaAmount = maxStamina * (staminaUnlockPercent / 100f);

        if (isStaminaLocked && currentStamina >= unlockStaminaAmount)
        {
            isStaminaLocked = false;
        }

        wasStaminaEmpty = isStaminaEmpty;

        bool isRegenerating = !isSprinting && currentStamina > previousStamina && currentStamina < maxStamina;

        if (isRegenerating && !wasStaminaRegenerating)
        {
            if (playerAudioManager != null)
            {
                playerAudioManager.PlayStaminaRegeneratingSound();
            }
        }

        wasStaminaRegenerating = isRegenerating;
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
            damageEffectRoot.SetActive(true);

        if (damageParticles != null)
            damageParticles.Play();

        if (damageAnimator != null)
            damageAnimator.SetTrigger(damageAnimationTrigger);
    }

    private void DisablePlayerControls()
    {
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }

        if (playerRigidbody != null)
            playerRigidbody.isKinematic = true;
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
            healthBar.value = currentHealth;

        if (playerAudioManager != null)
            playerAudioManager.PlayDamageSound();

        UpdateLowHealthEffect();
        ShowDamageEffects();

        if (currentHealth <= 0)
            Die();
    }

    private void ShowDamageEffects()
    {
        if (damageEffectRoot != null)
        {
            damageEffectRoot.SetActive(true);
            StartCoroutine(HideDamageEffect());
        }

        if (damageParticles != null)
            damageParticles.Play();

        if (damageAnimator != null)
            damageAnimator.SetTrigger(damageAnimationTrigger);

        if (vignetteAnimator != null && currentHealth <= midHealthThreshold)
            vignetteAnimator.SetTrigger(HitParameter);
    }

    private IEnumerator HideDamageEffect()
    {
        yield return new WaitForSeconds(0.5f);

        if (damageEffectRoot != null)
            damageEffectRoot.SetActive(false);
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
            xpBar.value = currentXp;

        if (isFirstXP)
            UpdateLevelText();
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

        Transform xpGainTextTransform = xpGainInstance.transform.Find(xpGainTextPath);
        if (xpGainTextTransform != null)
        {
            if (xpGainTextTransform.TryGetComponent<TextMeshProUGUI>(out var xpGainText))
                xpGainText.text = $"+{amount} XP";
        }

        if (xpGainInstance.TryGetComponent<Animator>(out var animator))
        {
            int layerIndex = animator.GetLayerIndex("Base Layer");
            if (layerIndex != -1 && animator.HasState(layerIndex, Animator.StringToHash("ANIM_HUD_Event_XPGain_In")))
                animator.Play("ANIM_HUD_Event_XPGain_In", layerIndex);
        }

        Destroy(xpGainInstance, xpGainAnimationDuration);
    }

    private void LevelUp()
    {
        currentLevel++;
        xpToNextLevel *= xpScalingFactor;

        if (xpBar != null)
            xpBar.maxValue = xpToNextLevel;

        UpdateLevelText();
        ShowLevelUpAnimation();
    }

    private void ShowLevelUpAnimation()
    {
        if (levelUpAnimatorPrefab == null || targetCanvas == null) return;

        GameObject levelUpInstance = Instantiate(levelUpAnimatorPrefab, targetCanvas.transform);

        if (levelUpInstance.TryGetComponent<RectTransform>(out var rectTransform))
            rectTransform.localScale = Vector3.one;

        Transform levelTextTransform = levelUpInstance.transform.Find(levelNumberTextPath);
        if (levelTextTransform != null)
        {
            if (levelTextTransform.TryGetComponent<TextMeshProUGUI>(out var levelNumberText))
                levelNumberText.text = currentLevel.ToString();
        }

        if (levelUpInstance.TryGetComponent<Animator>(out var animator))
        {
            int layerIndex = animator.GetLayerIndex("Base Layer");
            if (layerIndex != -1 && animator.HasState(layerIndex, Animator.StringToHash("ANIM_HUD_Event_LevelUp_In")))
                animator.Play("ANIM_HUD_Event_LevelUp_In", layerIndex);
        }

        Destroy(levelUpInstance, 5f);
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
            levelText.text = currentXp <= 0 ? "XP" : currentLevel.ToString();
    }

    public void AddHealth(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (healthBar != null)
            healthBar.value = currentHealth;

        if (UIAudioManager.Instance != null)
            UIAudioManager.Instance.PlayMedicineUsedSound();

        UpdateLowHealthEffect();
    }

    public void AddHunger(float amount)
    {
        if (isDead) return;

        currentHunger = Mathf.Min(currentHunger + amount, maxHunger);

        if (hungerBar != null)
            hungerBar.value = currentHunger;

        if (playerAudioManager != null)
            playerAudioManager.PlayEatingSound();

        if (currentHunger > lowHungerThreshold)
            isLowHungerAudioActive = false;
    }

    public bool CanUseSpecialMove(float staminaCost)
    {
        if (isStaminaLocked) return false;

        float currentStaminaPercentage = (currentStamina / maxStamina) * 100f;
        return currentStaminaPercentage >= staminaRequiredForSpecialMoves && currentStamina >= staminaCost;
    }

    public bool UseStaminaForDoubleJump()
    {
        if (CanUseSpecialMove(staminaCostDoubleJump))
        {
            currentStamina -= staminaCostDoubleJump;
            currentStamina = Mathf.Max(0, currentStamina);

            if (staminaBar != null)
                staminaBar.value = currentStamina;

            return true;
        }

        return false;
    }

    public bool UseStaminaForSlide()
    {
        if (CanUseSpecialMove(staminaCostSlide))
        {
            currentStamina -= staminaCostSlide;
            currentStamina = Mathf.Max(0, currentStamina);

            if (staminaBar != null)
                staminaBar.value = currentStamina;

            return true;
        }

        return false;
    }

    public void SetSprinting(bool sprinting)
    {
        if (isDead) return;
        isSprinting = sprinting;
    }

    public float GetCurrentHunger() => currentHunger;
    public float GetCurrentHealth() => currentHealth;
    public bool HasStaminaForSprinting() => !isStaminaLocked && currentStamina > 0;
    public int GetCurrentLevel() => currentLevel;
    public float GetCurrentXP() => currentXp;
    public float GetXPToNextLevel() => xpToNextLevel;
    public bool IsDead() => isDead;

    private void UpdateLowHealthEffect()
    {
        if (vignetteAnimator == null) return;

        bool shouldBeLowHealth = currentHealth <= lowHealthThreshold;

        isLowHealthEffectActive = shouldBeLowHealth;
        vignetteAnimator.SetBool(ActiveParameter, isLowHealthEffectActive);

        if (playerAudioManager != null)
            playerAudioManager.UpdateLowHealthState(isLowHealthEffectActive);
    }

    private void OnValidate()
    {
        if (lowHealthThreshold > midHealthThreshold)
            lowHealthThreshold = midHealthThreshold;

        if (midHealthThreshold > maxHealth)
            midHealthThreshold = maxHealth;

        if (lowHungerThreshold > maxHunger)
            lowHungerThreshold = maxHunger;
    }
}