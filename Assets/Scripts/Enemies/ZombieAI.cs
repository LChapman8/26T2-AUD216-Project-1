using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/*
 * ZombieAI.cs
 * 
 * Purpose: Controls zombie behavior, combat, and lifecycle
 * Used by: Enemy system, wave management
 * 
 * Key Features:
 * - Health and damage system
 * - Player tracking and pursuit
 * - Attack patterns
 * - Death handling
 * - UI health display
 * - XP rewards
 * 
 * AI Behaviors:
 * - Detection range checking
 * - Attack range management
 * - Movement control
 * - Animation state management
 * 
 * Performance Considerations:
 * - Efficient distance calculations
 * - Smart animation transitions
 * - Optimized UI updates
 * - Clean event handling
 * 
 * Dependencies:
 * - Requires Animator component
 * - ZombieManager integration
 * - PlayerStats for XP
 * - UI canvas system
 */

public class ZombieAI : MonoBehaviour, IDamageable
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName = "Zombie";
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("UI Elements")]
    [SerializeField] private Canvas worldSpaceCanvas;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("Set to 0 for infinite detection range")]
    [SerializeField] private float detectionRange = 0f;
    private Transform playerTransform;
    private NavMeshAgent agent;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackDamage = 20f;
    private float lastAttackTime;
    private bool isAttacking;

    // Wave system modifiers
    private float speedMultiplier = 1f;
    private float healthMultiplier = 1f;
    public event System.Action OnZombieDeath;

    // Animator reference
    private Animator animator;
    private bool isDying = false;
    private ZombieManager zombieManager;

    [Header("XP Reward")]
    [SerializeField] private float xpReward = 25f;  // Base XP reward for killing this zombie type
    public float XPReward => xpReward;

    [Header("Audio")]
    [SerializeField] private string enemyAudioProfileName; // Match this with profile name in EnemyAudioManager
    private AudioSource audioSource;
    private AudioSource attackAudioSource;
    private EnemyAudioManager enemyAudioManager;

#pragma warning disable 0414

    private bool isPositionLocked = false;
#pragma warning restore 0414
    private void Start()
    {
        currentHealth = maxHealth;

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Get the NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange;
        }

        // Setup collider if needed
        if (GetComponent<Collider>() == null)
        {
            CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.center = new Vector3(0, 1f, 0);
        }

        // Get the animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component missing from " + gameObject.name);
        }

        // Initialize UI
        InitializeHealthUI();
        InitializeNameUI();

        // Register with ZombieManager
        zombieManager = FindFirstObjectByType<ZombieManager>();
        if (zombieManager != null)
        {
            zombieManager.RegisterZombie(gameObject);
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Get the attack audio source from child
        Transform attackAudioChild = transform.Find("Audio Source - Attack");
        if (attackAudioChild != null)
        {
            attackAudioSource = attackAudioChild.GetComponent<AudioSource>();
        }

        enemyAudioManager = FindFirstObjectByType<EnemyAudioManager>();
        if (enemyAudioManager != null)
        {
            // Start the looping ambient sound
            enemyAudioManager.StartVocalizationLoop(enemyAudioProfileName, audioSource);
        }
    }

    private void Update()
    {
        // Check for death first, before any other state checks
        if (currentHealth <= 0 && !isDying)
        {
            Die();
            return;
        }

        if (isDying)
        {
            if (agent != null) agent.isStopped = true;
            return;
        }

        if (playerTransform != null)
        {
            // Get current animation state
            var currentState = animator.GetCurrentAnimatorStateInfo(0);
            bool isInAttackAnimation = currentState.IsName("Attack");

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // If we're in attack animation, force position lock
            if (isInAttackAnimation)
            {
                isPositionLocked = true;
                if (agent != null) agent.isStopped = true;
                animator.SetBool("IsMoving", false);
                return;
            }

            // If we're not in attack animation, ensure we can move
            isPositionLocked = false;
            isAttacking = false;  // Make sure we clear the attack flag

            if (detectionRange == 0 || distanceToPlayer <= detectionRange)
            {
                if (distanceToPlayer > attackRange)
                {
                    agent.isStopped = false;
                    agent.speed = moveSpeed * speedMultiplier;
                    agent.SetDestination(playerTransform.position);
                    animator.SetBool("IsMoving", true);
                }
                else
                {
                    animator.SetBool("IsMoving", false);
                    if (agent != null) agent.isStopped = true;

                    // Look at player for attack
                    Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                    directionToPlayer.y = 0;
                    transform.rotation = Quaternion.LookRotation(directionToPlayer);
                    StartAttack();
                }
            }
            else
            {
                animator.SetBool("IsMoving", false);
                if (agent != null) agent.isStopped = true;
            }
        }
    }

    private void StartAttack()
    {
        if (animator != null && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            // Ensure movement is stopped
            animator.SetBool("IsMoving", false);
            isAttacking = true;
            isPositionLocked = true;
            lastAttackTime = Time.time;
            
            // Set attack parameters
            animator.SetTrigger("Attack");
            animator.SetBool("IsAttacking", true);
            
            if (attackAudioSource != null)
            {
                StartCoroutine(PlayDelayedAttackSound());
            }
            
            StartCoroutine(AttackSequence());
        }
    }

    private IEnumerator PlayDelayedAttackSound()
    {
        yield return new WaitForSeconds(enemyAudioManager.GetAttackSoundDelay(enemyAudioProfileName));
        if (attackAudioSource != null)
        {
            enemyAudioManager.PlayAttackSound(enemyAudioProfileName, attackAudioSource);
        }
    }

    private IEnumerator AttackSequence()
    {
        while (true)
        {
            // Check if we should stop attacking
            if (playerTransform == null)
            {
                break;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer > attackRange)
            {
                break;
            }

            // Wait until damage point in animation
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && 
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f &&
                Time.time >= lastAttackTime + attackCooldown)
            {
                DealDamage();
                lastAttackTime = Time.time;
                
                // Wait until near end of current attack
                while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.9f)
                {
                    // Check distance even during animation
                    distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                    if (distanceToPlayer > attackRange)
                    {
                        EndAttack();
                        yield break;
                    }
                    yield return null;
                }
            }

            yield return null;
        }

        EndAttack();
    }

    private void EndAttack()
    {
        isAttacking = false;
        isPositionLocked = false;
        animator.SetBool("IsAttacking", false);
    }

    private void DealDamage()
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                IDamageable playerDamageable = playerTransform.GetComponent<IDamageable>();
                if (playerDamageable != null)
                {
                    playerDamageable.TakeDamage(attackDamage);
                }
            }
        }
    }

    private void InitializeNameUI()
    {
        if (nameText != null)
        {
            nameText.text = npcName;
        }
    }

    private void InitializeHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)}";
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDying) return;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthUI();
        
        if (currentHealth <= 0 && !isDying)
        {
            Die();
        }
    }

    private void Die()
    {
        isDying = true;
        isAttacking = false;
        
        // Notify any listeners
        OnZombieDeath?.Invoke();
        
        // Stop movement and disable collider
        if (agent != null) agent.isStopped = true;
        if (GetComponent<Collider>() != null)
            GetComponent<Collider>().enabled = false;

        // Hide UI elements
        if (worldSpaceCanvas != null)
            worldSpaceCanvas.enabled = false;

        // Play death animation and sound
        animator.SetBool("IsDying", true);
        
        // Start coroutine to handle death sequence
        StartCoroutine(DeathSequence());
        StartCoroutine(FadeOutAudio());

        if (enemyAudioManager != null)
        {
            enemyAudioManager.PlayDeathSound(enemyAudioProfileName, audioSource);
        }
    }

    private IEnumerator DeathSequence()
    {
        // Wait until we're in the death animation
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Dying"))
        {
            yield return null;
        }
        
        // Wait for death animation to complete
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f)
        {
            yield return null;
        }
        
        // Pause animation
        animator.speed = 0;
        
        // Wait before starting sink animation
        yield return new WaitForSeconds(4f);
        
        // Sink into ground
        float sinkDuration = 1.5f;
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - new Vector3(0, 2f, 0); // Sink 2 units down
        
        while (elapsedTime < sinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / sinkDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        
        // Destroy the game object
        Destroy(gameObject);
    }

    private IEnumerator FadeOutAudio()
    {
        if (audioSource != null)
        {
            float startVolume = audioSource.volume;
            float elapsedTime = 0f;
            float fadeDuration = 2f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float newVolume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
                audioSource.volume = newVolume;
                yield return null;
            }

            audioSource.Stop();
        }
    }

    private void LateUpdate()
    {
        if (worldSpaceCanvas != null && Camera.main != null)
        {
            worldSpaceCanvas.transform.LookAt(Camera.main.transform);
            worldSpaceCanvas.transform.Rotate(0, 180, 0);
        }
    }

    // Wave system methods
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public void SetHealthMultiplier(float multiplier)
    {
        healthMultiplier = multiplier;
        maxHealth *= multiplier;
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public bool IsDead()
    {
        return isDying || currentHealth <= 0;
    }

    private void OnDestroy()
    {
        // Ensure zombie is unregistered if destroyed without dying animation
        if (zombieManager != null && !isDying)
        {
            zombieManager.UnregisterZombie(gameObject);
        }
    }
}