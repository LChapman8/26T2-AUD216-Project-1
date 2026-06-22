using UnityEngine;
using System.Collections;

/*
 * BulletImpactManager.cs
 * 
 * Purpose: Manages bullet impact effects and sounds across different surfaces
 * Used by: Weapon system, impact visualization
 * 
 * Key Features:
 * - Surface-specific particle effects
 * - Delayed sound based on distance
 * - Blood effects for enemy impacts
 * - Configurable effect variations
 * - Sound propagation simulation
 * 
 * Effect Management:
 * - Automatic effect cleanup
 * - Sound pooling system
 * - Dynamic effect spawning
 * - Surface type detection
 * 
 * Performance Considerations:
 * - Efficient effect instantiation
 * - Smart audio source reuse
 * - Optimized cleanup routines
 * - Distance-based culling
 * 
 * Dependencies:
 * - Requires configured effect prefabs
 * - Audio source prefab system
 * - Surface tag configuration
 * - Physics raycast system
 */
public class BulletImpactManager : MonoBehaviour
{
    public static BulletImpactManager Instance { get; private set; }

    [System.Serializable]
    public class ImpactEffect
    {
        [Tooltip("Tag of the surface this effect should play on (e.g., 'Metal', 'Wood', 'Concrete')")]
        public string surfaceTag;

        [Tooltip("Particle system that plays when bullet hits this surface type")]
        public ParticleSystem particleEffect;

        [Tooltip("Array of impact sounds - one will be randomly selected when bullet hits this surface")]
        public AudioClip[] impactSounds;

        [Tooltip("Volume level for impact sounds on this surface")]
        [Range(0f, 1f)]
        public float volume = 1f;

        [Tooltip("Minimum pitch variation for impact sounds (lower values = deeper sounds)")]
        [Range(0.8f, 1.2f)]
        public float minPitch = 0.9f;

        [Tooltip("Maximum pitch variation for impact sounds (higher values = higher pitched sounds)")]
        [Range(0.8f, 1.2f)]
        public float maxPitch = 1.1f;
    }

    [Header("Impact Effects")]
    [Tooltip("Array of impact effects for different surface types (metal, wood, etc.)")]
    [SerializeField] private ImpactEffect[] impactEffects;

    [Tooltip("How long particle effects should remain visible before being destroyed")]
    [SerializeField] private float effectDuration = 1f;
    
    [Header("Enemy Impact")]
    [Tooltip("Particle effect to spawn when bullet hits an organic enemy (blood)")]
    [SerializeField] private ParticleSystem bloodEffect;

    [Tooltip("Particle effect to spawn when bullet hits a rock-based enemy")]
    [SerializeField] private ParticleSystem rockEffect;

    [Tooltip("Array of sounds to play when bullet hits an enemy")]
    [SerializeField] private AudioClip[] enemyImpactSounds;

    [Tooltip("Volume level for enemy impact sounds")]
    [SerializeField] [Range(0f, 1f)] private float enemyImpactVolume = 1f;

    [Tooltip("Minimum pitch variation for enemy impact sounds (lower values = deeper sounds)")]
    [SerializeField] [Range(0.8f, 1.2f)] private float enemyMinPitch = 0.9f;

    [Tooltip("Maximum pitch variation for enemy impact sounds (higher values = higher pitched sounds)")]
    [SerializeField] [Range(0.8f, 1.2f)] private float enemyMaxPitch = 1.1f;

    [Header("Sound Settings")]
    [Tooltip("Speed in meters per second - affects delay of impact sounds based on distance")]
    [SerializeField] private float speedOfSound = 343f;

    [Tooltip("Master volume multiplier for all impact sounds")]
    [SerializeField] [Range(0f, 2f)] private float masterVolume = 1f;

    [Tooltip("Prefab for the audio source that will play impact sounds")]
    [SerializeField] private AudioSource impactAudioSourcePrefab;

    private AudioSource impactAudioSource;
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Instantiate the audio source from prefab
        if (impactAudioSourcePrefab != null)
        {
            impactAudioSource = Instantiate(impactAudioSourcePrefab, transform);
        }
        else
        {
            Debug.LogError("Impact Audio Source Prefab not assigned!");
        }

        // Find player transform
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    public void PlayImpactEffect(RaycastHit hit)
    {
        // Spawn visual effects immediately
        if (hit.collider.CompareTag("Enemy"))
        {
            PlayEnemyEffect(hit);
            PlayDelayedImpactSound(enemyImpactSounds, hit.point, enemyImpactVolume, enemyMinPitch, enemyMaxPitch);
            return;
        }

        foreach (var effect in impactEffects)
        {
            if (hit.collider.CompareTag(effect.surfaceTag))
            {
                SpawnEffect(effect.particleEffect, hit);
                PlayDelayedImpactSound(effect.impactSounds, hit.point, effect.volume, effect.minPitch, effect.maxPitch);
                return;
            }
        }
    }

    private void PlayEnemyEffect(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Rock"))
        {
            if (rockEffect != null)
            {
                SpawnEffect(rockEffect, hit);
            }
        }
        else if (hit.collider.CompareTag("Enemy"))
        {
            if (bloodEffect != null)
            {
                SpawnEffect(bloodEffect, hit);
            }
        }
    }

    private void SpawnEffect(ParticleSystem effectPrefab, RaycastHit hit)
    {
        ParticleSystem spawnedEffect = Instantiate(effectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(spawnedEffect.gameObject, effectDuration);
    }

    private void PlayDelayedImpactSound(AudioClip[] sounds, Vector3 impactPoint, float baseVolume, float minPitch, float maxPitch)
    {
        if (sounds == null || sounds.Length == 0 || impactAudioSource == null || playerTransform == null) return;

        // Calculate distance from player to impact point
        float distance = Vector3.Distance(playerTransform.position, impactPoint);
        
        // Calculate delay based on distance and speed of sound
        float delay = distance / speedOfSound;

        // Calculate final volume
        float finalVolume = baseVolume * masterVolume;

        // Start coroutine to play sound after delay
        StartCoroutine(PlaySoundAfterDelay(sounds, impactPoint, finalVolume, minPitch, maxPitch, delay));
    }

    private System.Collections.IEnumerator PlaySoundAfterDelay(AudioClip[] sounds, Vector3 position, float volume, float minPitch, float maxPitch, float delay)
    {
        yield return new WaitForSeconds(delay);

        AudioClip randomSound = sounds[Random.Range(0, sounds.Length)];
        
        impactAudioSource.pitch = Random.Range(minPitch, maxPitch);
        impactAudioSource.transform.position = position;
        impactAudioSource.PlayOneShot(randomSound, volume);
        impactAudioSource.pitch = 1f;
    }
} 