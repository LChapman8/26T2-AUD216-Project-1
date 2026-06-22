using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class PlayerAudioManager : MonoBehaviour
{
    public enum SoundType
    {
        Health,
        Stamina,
        Hunger
    }

    [System.Serializable]
    public class PlayerSoundProfile
    {
        [Tooltip("Unique identifier for this sound profile")]
        public string profileName;
        
        [Tooltip("Type of player state that determines which audio properties are available")]
        public SoundType soundType;

        [Header("Audio Source")]
        [Tooltip("Optional: Specific AudioSource to use for this profile")]
        public AudioSource customAudioSource;
        [Tooltip("Audio Mixer Group for routing and effects processing")]
        public AudioMixerGroup mixerGroupOutput;

        // Health-specific properties
        [Header("Health Sounds")]
        [Tooltip("Played when taking damage")]
        public AudioClip[] damageSounds;
        
        [Header("Heartbeat")]
        [Tooltip("Played when health is critically low")]
        public AudioClip lowHealthLoop;
        [Tooltip("Dedicated AudioSource for heartbeat")]
        public AudioSource heartbeatAudioSource;
        [Tooltip("Health threshold below which the heartbeat sound starts playing")]
        [Range(0f, 100f)] public float playBelowThisHealth = 25f;
        [Range(0f, 1f)] public float damageVolume = 1f;
        [Range(0f, 1f)] public float lowHealthVolume = 0.7f;

        // Stamina-specific properties
        [Header("Stamina Sounds")]
        [Tooltip("Played when stamina is depleted")]
        public AudioClip staminaDepletedSound;
        [Tooltip("Played when stamina starts regenerating")]
        public AudioClip staminaRegeneratingSound;
        [Range(0f, 1f)] public float staminaVolume = 0.8f;

        // Hunger-specific properties
        [Header("Hunger Sounds")]
        [Tooltip("Played when hunger is critically low")]
        public AudioClip[] hungerSounds;
        [Tooltip("Played when eating/recovering hunger")]
        public AudioClip[] eatingSound;
        [Range(0f, 1f)] public float hungerVolume = 0.8f;
        [Range(0f, 1f)] public float eatingVolume = 1f;
    }

    [SerializeField] private PlayerSoundProfile[] soundProfiles;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private AudioSource defaultAudioSource;

    private Coroutine lowHealthCoroutine;
    private bool isLowHealthPlaying;

    private void Awake()
    {
        InitializeAudioSources();
    }

    private void InitializeAudioSources()
    {
        if (defaultAudioSource == null)
        {
            defaultAudioSource = gameObject.AddComponent<AudioSource>();
            defaultAudioSource.playOnAwake = false;
        }

        foreach (var profile in soundProfiles)
        {
            if (profile.customAudioSource != null)
            {
                profile.customAudioSource.playOnAwake = false;
                if (profile.mixerGroupOutput != null)
                {
                    profile.customAudioSource.outputAudioMixerGroup = profile.mixerGroupOutput;
                }
            }
        }
    }

    public void PlayDamageSound()
    {
        var profile = GetProfileByType(SoundType.Health);
        if (profile == null || profile.damageSounds == null || profile.damageSounds.Length == 0) return;

        AudioSource sourceToUse = profile.customAudioSource != null ? profile.customAudioSource : defaultAudioSource;
        AudioClip randomClip = profile.damageSounds[Random.Range(0, profile.damageSounds.Length)];
        
        sourceToUse.PlayOneShot(randomClip, profile.damageVolume);
    }

    public void UpdateLowHealthState(bool isLowHealth)
    {
        var profile = GetProfileByType(SoundType.Health);
        if (profile == null || profile.lowHealthLoop == null) return;

        // Get current health from PlayerStats
        float currentHealth = playerStats != null ? playerStats.GetCurrentHealth() : 0f;
        bool shouldPlayHeartbeat = currentHealth <= profile.playBelowThisHealth;

        if (shouldPlayHeartbeat && !isLowHealthPlaying)
        {
            StartLowHealthLoop(profile);
        }
        else if (!shouldPlayHeartbeat && isLowHealthPlaying)
        {
            StopLowHealthLoop();
        }
    }

    private void StartLowHealthLoop(PlayerSoundProfile profile)
    {
        if (lowHealthCoroutine != null)
        {
            StopCoroutine(lowHealthCoroutine);
        }
        lowHealthCoroutine = StartCoroutine(PlayLowHealthLoop(profile));
    }

    private IEnumerator PlayLowHealthLoop(PlayerSoundProfile profile)
    {
        AudioSource sourceToUse = profile.heartbeatAudioSource != null ? profile.heartbeatAudioSource : defaultAudioSource;
        sourceToUse.clip = profile.lowHealthLoop;
        sourceToUse.loop = true;
        isLowHealthPlaying = true;
        
        sourceToUse.Play();

        while (isLowHealthPlaying && playerStats != null)
        {
            float currentHealth = playerStats.GetCurrentHealth();
            
            // Calculate health percentage relative to the threshold
            float healthPercentage = (currentHealth / profile.playBelowThisHealth) * 100f;
            
            // Map volume based on health percentage
            float minVolume = 0.02f;
            float maxVolume = 1f;
            float volumeScale;

            if (healthPercentage <= 10f) // Below 10% of threshold health
            {
                volumeScale = maxVolume;
            }
            else
            {
                // Map health percentage (10-100) to volume (1-0.02)
                float t = (healthPercentage - 10f) / 90f; // Normalize to 0-1 range
                volumeScale = Mathf.Lerp(maxVolume, minVolume, t);
            }
            
            // Apply the volume
            sourceToUse.volume = volumeScale;
            
            yield return null;
        }

        sourceToUse.Stop();
        isLowHealthPlaying = false;
    }

    private void StopLowHealthLoop()
    {
        if (lowHealthCoroutine != null)
        {
            StopCoroutine(lowHealthCoroutine);
            lowHealthCoroutine = null;
        }

        var profile = GetProfileByType(SoundType.Health);
        if (profile != null)
        {
            AudioSource sourceToUse = profile.heartbeatAudioSource != null ? profile.heartbeatAudioSource : defaultAudioSource;
            sourceToUse.Stop();
        }
        isLowHealthPlaying = false;
    }

    public void PlayStaminaDepletedSound()
    {
        var profile = GetProfileByType(SoundType.Stamina);
        if (profile == null || profile.staminaDepletedSound == null) return;

        AudioSource sourceToUse = profile.customAudioSource != null ? profile.customAudioSource : defaultAudioSource;
        sourceToUse.PlayOneShot(profile.staminaDepletedSound, profile.staminaVolume);
    }

    public void PlayStaminaRegeneratingSound()
    {
        var profile = GetProfileByType(SoundType.Stamina);
        if (profile == null || profile.staminaRegeneratingSound == null) return;

        AudioSource sourceToUse = profile.customAudioSource != null ? profile.customAudioSource : defaultAudioSource;
        sourceToUse.PlayOneShot(profile.staminaRegeneratingSound, profile.staminaVolume);
    }

    public void PlayHungerSound()
    {
        var profile = GetProfileByType(SoundType.Hunger);
        if (profile == null || profile.hungerSounds == null || profile.hungerSounds.Length == 0) return;

        AudioSource sourceToUse = profile.customAudioSource != null ? profile.customAudioSource : defaultAudioSource;
        AudioClip randomClip = profile.hungerSounds[Random.Range(0, profile.hungerSounds.Length)];
        
        sourceToUse.PlayOneShot(randomClip, profile.hungerVolume);
    }

    public void PlayEatingSound()
    {
        var profile = GetProfileByType(SoundType.Hunger);
        if (profile == null || profile.eatingSound == null || profile.eatingSound.Length == 0) return;

        AudioSource sourceToUse = profile.customAudioSource != null ? profile.customAudioSource : defaultAudioSource;
        AudioClip randomClip = profile.eatingSound[Random.Range(0, profile.eatingSound.Length)];
        
        sourceToUse.PlayOneShot(randomClip, profile.eatingVolume);
    }

    private PlayerSoundProfile GetProfileByType(SoundType type)
    {
        if (soundProfiles == null) return null;
        
        foreach (var profile in soundProfiles)
        {
            if (profile.soundType == type)
                return profile;
        }
        return null;
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (soundProfiles != null)
        {
            foreach (var profile in soundProfiles)
            {
                if (profile != null && string.IsNullOrEmpty(profile.profileName))
                {
                    profile.profileName = profile.soundType.ToString();
                }
            }
        }
    }
    #endif
} 