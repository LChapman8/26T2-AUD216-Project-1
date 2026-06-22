using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

/*
 * WeaponAudioManager.cs
 * 
 * Purpose: Handles all weapon-related sound effects.
 * Used by: WeaponController, CrossbowController
 * 
 * Manages weapon sound profiles including shooting, reloading,
 * and state change sounds. Provides randomization and variation
 * in weapon sounds for more dynamic audio.
 * 
 * Performance Considerations:
 * - Uses pooled audio sources
 * - Implements sound variation system
 * - Optimizes audio clip loading
 * 
 * Dependencies:
 * - AudioMixer system
 * - Weapon system
 * - Requires configured weapon sound profiles
 */

public enum WeaponType
{
    Gun,
    Axe
}

public class WeaponAudioManager : MonoBehaviour
{
    #region Classes
    [System.Serializable]
    public class WeaponSoundProfile
    {
        [Header("Weapon Info")]
        [Tooltip("Unique identifier for this weapon's sound profile")]
        public string weaponName;
        
        [Tooltip("Array of audio clips that will be randomly selected when the weapon fires")]
        public AudioClip[] shootSounds;
        [Tooltip("Volume level for shooting sounds")]
        [Range(0f, 1f)] public float shootVolume = 1f;
        [Tooltip("Minimum pitch variation for shooting sounds")]
        [Range(0.8f, 1.2f)] public float minPitchVariation = 0.95f;
        [Tooltip("Maximum pitch variation for shooting sounds")]
        [Range(0.8f, 1.2f)] public float maxPitchVariation = 1.05f;
        
        [Header("Weapon State Sounds")]
        [Tooltip("Sound played when weapon is raised")]
        public AudioClip raiseWeaponSound;
        [Tooltip("Sound played when weapon is lowered")]
        public AudioClip lowerWeaponSound;
        [Tooltip("Volume level for raise/lower weapon sounds")]
        [Range(0f, 1f)] public float toggleSoundVolume = 1f;
        [Tooltip("Amount of random pitch variation applied to raise/lower sounds")]
        [Range(0f, 0.2f)] public float togglePitchVariation = 0.1f;
        
        [Header("Reload Sounds")]
        [Tooltip("Sound played when reload begins")]
        public AudioClip reloadStartSound;
        [Tooltip("Sound played when reload completes")]
        public AudioClip reloadEndSound;
        [Tooltip("Array of sounds played during the reload sequence")]
        public AudioClip[] reloadActionSounds;
        [Tooltip("Volume level for all reload-related sounds")]
        [Range(0f, 1f)] public float reloadVolume = 1f;

        [Header("Weapon Type")]
        [Tooltip("Type of weapon that determines which audio properties are available")]
        public WeaponType weaponType;
    }
    #endregion

    #region Private Fields
    [SerializeField] private WeaponSoundProfile[] m_WeaponProfiles;
    [SerializeField] private AudioSource m_WeaponAudioSource;
    [SerializeField] private AudioMixerGroup m_AudioMixerGroup;
    
    private Dictionary<string, WeaponSoundProfile> m_ProfileLookup;
    private Dictionary<string, AudioSource> m_WeaponAudioSources;
    #endregion

    #region Public Properties
    public AudioSource WeaponAudioSource
    {
        get => m_WeaponAudioSource;
        set => m_WeaponAudioSource = value;
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeAudioSources();
    }

    private void OnValidate()
    {
        // Ensure weapon profiles have unique names
        if (m_WeaponProfiles != null)
        {
            HashSet<string> names = new HashSet<string>();
            foreach (var profile in m_WeaponProfiles)
            {
                if (profile != null && !string.IsNullOrEmpty(profile.weaponName))
                {
                    if (!names.Add(profile.weaponName))
                    {
                        Debug.LogWarning($"Duplicate weapon profile name found: {profile.weaponName}");
                    }
                }
            }
        }
    }
    #endregion

    #region Private Methods
    private void InitializeAudioSources()
    {
        // Initialize the main weapon audio source if not set
        if (m_WeaponAudioSource == null)
        {
            m_WeaponAudioSource = gameObject.AddComponent<AudioSource>();
            m_WeaponAudioSource.playOnAwake = false;
            m_WeaponAudioSource.spatialBlend = 0f; // Full 2D as these are player's weapons
        }

        if (m_AudioMixerGroup != null)
        {
            m_WeaponAudioSource.outputAudioMixerGroup = m_AudioMixerGroup;
        }

        m_ProfileLookup = new Dictionary<string, WeaponSoundProfile>();
        m_WeaponAudioSources = new Dictionary<string, AudioSource>();

        if (m_WeaponProfiles == null) return;

        foreach (var profile in m_WeaponProfiles)
        {
            if (profile == null || string.IsNullOrEmpty(profile.weaponName)) continue;

            m_ProfileLookup[profile.weaponName] = profile;
            
            // Create dedicated audio source for each weapon
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            
            if (m_AudioMixerGroup != null)
            {
                source.outputAudioMixerGroup = m_AudioMixerGroup;
            }
            
            m_WeaponAudioSources[profile.weaponName] = source;
        }
    }
    #endregion

    #region Public Methods
    public void PlayShootSound(string weaponName)
    {
        if (!m_ProfileLookup.TryGetValue(weaponName, out WeaponSoundProfile profile)) return;

        if (profile.shootSounds != null && profile.shootSounds.Length > 0)
        {
            AudioClip randomShootSound = profile.shootSounds[Random.Range(0, profile.shootSounds.Length)];
            m_WeaponAudioSource.pitch = Random.Range(profile.minPitchVariation, profile.maxPitchVariation);
            m_WeaponAudioSource.PlayOneShot(randomShootSound, profile.shootVolume);
            m_WeaponAudioSource.pitch = 1f;
        }
    }

    public void PlayWeaponRaiseSound(string weaponName)
    {
        if (!m_ProfileLookup.TryGetValue(weaponName, out WeaponSoundProfile profile)) return;

        if (profile.raiseWeaponSound != null)
        {
            float randomPitch = 1f + Random.Range(-profile.togglePitchVariation, profile.togglePitchVariation);
            m_WeaponAudioSource.pitch = randomPitch;
            m_WeaponAudioSource.PlayOneShot(profile.raiseWeaponSound, profile.toggleSoundVolume);
            m_WeaponAudioSource.pitch = 1f;
        }
    }

    public void PlayWeaponLowerSound(string weaponName)
    {
        if (!m_ProfileLookup.TryGetValue(weaponName, out WeaponSoundProfile profile)) return;

        if (profile.lowerWeaponSound != null)
        {
            float randomPitch = 1f + Random.Range(-profile.togglePitchVariation, profile.togglePitchVariation);
            m_WeaponAudioSource.pitch = randomPitch;
            m_WeaponAudioSource.PlayOneShot(profile.lowerWeaponSound, profile.toggleSoundVolume);
            m_WeaponAudioSource.pitch = 1f;
        }
    }

    public void PlayReloadStartSound(string weaponName)
    {
        if (!m_ProfileLookup.TryGetValue(weaponName, out WeaponSoundProfile profile)) return;

        if (profile.reloadStartSound != null)
        {
            m_WeaponAudioSource.PlayOneShot(profile.reloadStartSound, profile.reloadVolume);
        }
    }

    public void PlayReloadEndSound(string weaponName)
    {
        if (!m_ProfileLookup.TryGetValue(weaponName, out WeaponSoundProfile profile)) return;

        if (profile.reloadEndSound != null)
        {
            m_WeaponAudioSource.PlayOneShot(profile.reloadEndSound, profile.reloadVolume);
        }
    }

    public void PlayReloadActionSound(string weaponName)
    {
        if (!m_ProfileLookup.TryGetValue(weaponName, out WeaponSoundProfile profile)) return;

        if (profile.reloadActionSounds != null && profile.reloadActionSounds.Length > 0)
        {
            AudioClip randomReloadSound = profile.reloadActionSounds[Random.Range(0, profile.reloadActionSounds.Length)];
            m_WeaponAudioSource.PlayOneShot(randomReloadSound, profile.reloadVolume);
        }
    }
    #endregion

    #if UNITY_EDITOR
    [ContextMenu("Debug Weapon Profiles")]
    private void DebugWeaponProfiles()
    {
        if (m_WeaponProfiles == null)
        {
            Debug.Log("No weapon profiles assigned.");
            return;
        }

        foreach (var profile in m_WeaponProfiles)
        {
            if (profile == null) continue;
            
            Debug.Log($"Weapon: {profile.weaponName}\n" +
                     $"Shoot Sounds: {(profile.shootSounds?.Length ?? 0)} clips\n" +
                     $"Reload Sounds: {(profile.reloadActionSounds?.Length ?? 0)} clips");
        }
    }
    #endif
}