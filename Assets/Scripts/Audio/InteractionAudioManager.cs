using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

/*
 * InteractionAudioManager.cs
 * 
 * Purpose: Manages audio for interactive objects in the game world
 * Used by: Interactive objects, switches, doors, mechanisms
 * 
 * Key Features:
 * - Per-interaction sound profiles
 * - Volume control
 * - Mixer routing
 * - Sound variation
 * 
 * Performance Considerations:
 * - Efficient audio source pooling
 * - Smart profile lookup
 * - Optimized sound triggering
 * 
 * Dependencies:
 * - AudioMixer system
 * - Interaction system
 * - Requires configured interaction profiles
 */

[DisallowMultipleComponent]
public class InteractionAudioManager : MonoBehaviour
{
    public enum InteractionType
    {
        SimpleInteraction,
        ElectricalBox,
        GeneralInteraction,
        Firesticks,
        Campfire,
        MixingDesk
    }

    [System.Serializable]
    public class InteractionSoundProfile
    {
        [Tooltip("Unique identifier for this interaction - must match the name referenced in interaction scripts")]
        public string interactionName;
        
        [Tooltip("Type of interaction that determines which audio properties are available")]
        public InteractionType interactionType;

        // Simple Interaction
        [Tooltip("Single audio clip to play for this interaction")]
        public AudioClip interactionSound;
        [Tooltip("Optional: Specific AudioSource to use for this interaction")]
        public AudioSource customAudioSource;

        // General Interaction
        [Tooltip("Array of one-shot sounds that can be played for general interactions")]
        public AudioClip[] interactionSounds;

        // Fire-based (Firesticks & Campfire)
        [Tooltip("Array of ignition sounds - one will be randomly selected when the fire is lit")]
        public AudioClip[] igniteSounds;
        [Tooltip("Continuous loop sound that plays while the fire is burning")]
        public AudioClip fireLoop;
        [Tooltip("Time in seconds to wait after ignition before starting the continuous fire loop")]
        [Range(0f, 5f)] public float fireLoopStartDelay = 1f;

        // Electrical Box
        [Tooltip("Sound played when toggling the electrical box lever")]
        public AudioClip leverSound;

        [Tooltip("Looping sound for electrical sparks")]
        public AudioClip sparksSound;

        [Tooltip("Audio Mixer Group for routing and effects processing - determines output path")]
        public AudioMixerGroup mixerGroupOutput;

        [HideInInspector] public bool isLit = false;

        // Mixing Desk
        [Tooltip("Audio clip for the left speaker")]
        public AudioClip leftSpeakerAudio;
        [Tooltip("Audio clip for the right speaker")]
        public AudioClip rightSpeakerAudio;
        [Tooltip("AudioSource component for the left speaker")]
        public AudioSource leftSpeakerSource;
        [Tooltip("AudioSource component for the right speaker")]
        public AudioSource rightSpeakerSource;
    }

    [SerializeField] private InteractionSoundProfile[] interactionProfiles;
    
    private Dictionary<string, InteractionSoundProfile> profileLookup;
    private Dictionary<string, List<AudioSource>> profileAudioSources;

    private void Awake()
    {
        InitializeAudioSystem();
    }

    private void InitializeAudioSystem()
    {
        profileLookup = new Dictionary<string, InteractionSoundProfile>();
        profileAudioSources = new Dictionary<string, List<AudioSource>>();

        if (interactionProfiles == null) return;

        foreach (var profile in interactionProfiles)
        {
            if (profile == null || string.IsNullOrEmpty(profile.interactionName)) continue;

            profileLookup[profile.interactionName] = profile;
            profileAudioSources[profile.interactionName] = new List<AudioSource>();
        }

        // Configure audio sources for firesticks
        foreach (var profile in interactionProfiles)
        {
            if (profile == null || string.IsNullOrEmpty(profile.interactionName)) continue;

            foreach (var source in profileAudioSources[profile.interactionName])
            {
                source.spatialBlend = 1f; // 3D sound
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 1f;
                source.maxDistance = 8f;
                source.priority = 128;
            }
        }
    }

    public void PlayInteractionSound(string interactionName)
    {
        if (!profileLookup.TryGetValue(interactionName, out InteractionSoundProfile profile) || 
            !profileAudioSources.TryGetValue(interactionName, out List<AudioSource> sources))
            return;

        if (profile.fireLoop != null)
        {
            foreach (var source in sources)
            {
                source.PlayOneShot(profile.fireLoop, 1f);
            }
        }
    }

    public void RegisterAudioSource(string profileName, AudioSource source)
    {
        if (!profileLookup.TryGetValue(profileName, out InteractionSoundProfile profile))
            return;

        if (!profileAudioSources[profileName].Contains(source))
        {
            profileAudioSources[profileName].Add(source);
            ConfigureAudioSource(source); // Apply standard firestick audio settings
        }
    }

    private void ConfigureAudioSource(AudioSource source)
    {
        //source.spatialBlend = 1f;
        //source.rolloffMode = AudioRolloffMode.Linear;
        //source.minDistance = 1f;
        //source.maxDistance = 8f;
        source.priority = 128;
    }

    public void IgniteFirestick(string profileName, AudioSource source)
    {
        if (!profileLookup.TryGetValue(profileName, out InteractionSoundProfile profile))
            return;

        if (profile.interactionType != InteractionType.Firesticks && 
            profile.interactionType != InteractionType.Campfire)
            return;

        if (!profileAudioSources[profileName].Contains(source))
            return;

        // Set the output mixer group
        if (profile.mixerGroupOutput != null)
        {
            source.outputAudioMixerGroup = profile.mixerGroupOutput;
        }

        // Play random ignite sound
        if (profile.igniteSounds != null && profile.igniteSounds.Length > 0)
        {
            AudioClip randomIgniteSound = profile.igniteSounds[Random.Range(0, profile.igniteSounds.Length)];
            source.PlayOneShot(randomIgniteSound, 1f);
        }

        // Start fire loop after delay
        if (profile.fireLoop != null)
        {
            StartCoroutine(StartFireLoopDelayed(source, profile));
        }
    }

    private IEnumerator StartFireLoopDelayed(AudioSource source, InteractionSoundProfile profile)
    {
        yield return new WaitForSeconds(profile.fireLoopStartDelay);
        
        // Set the output mixer group
        if (profile.mixerGroupOutput != null)
        {
            source.outputAudioMixerGroup = profile.mixerGroupOutput;
        }

        source.clip = profile.fireLoop;
        source.time = Random.Range(0f, profile.fireLoop.length);
        source.loop = true;
        source.Play();
    }

    public void ResetFirestick(string interactionName)
    {
        if (!profileLookup.TryGetValue(interactionName, out InteractionSoundProfile profile) || 
            !profileAudioSources.TryGetValue(interactionName, out List<AudioSource> sources))
            return;

        profile.isLit = false;
        foreach (var source in sources)
        {
            source.Stop();
            source.volume = 0f;
        }
    }

    public void PlaySimpleInteraction(string profileName, AudioSource defaultSource)
    {
        if (!profileLookup.TryGetValue(profileName, out InteractionSoundProfile profile))
            return;

        if (profile.interactionType != InteractionType.SimpleInteraction)
            return;

        if (profile.interactionSound != null)
        {
            AudioSource sourceToUse = profile.customAudioSource != null ? profile.customAudioSource : defaultSource;
            
            // Set the output mixer group
            if (profile.mixerGroupOutput != null)
            {
                sourceToUse.outputAudioMixerGroup = profile.mixerGroupOutput;
            }

            sourceToUse.PlayOneShot(profile.interactionSound, 1f);
        }
    }

    public void PlayElectricalBoxSound(string profileName, AudioSource source, bool isLeverSound)
    {
        if (!profileLookup.TryGetValue(profileName, out InteractionSoundProfile profile))
            return;

        if (profile.interactionType != InteractionType.ElectricalBox)
            return;

        // Set the output mixer group
        if (profile.mixerGroupOutput != null)
        {
            source.outputAudioMixerGroup = profile.mixerGroupOutput;
        }

        if (isLeverSound)
        {
            if (profile.leverSound != null)
            {
                source.PlayOneShot(profile.leverSound, 1f);
            }
        }
        else
        {
            if (profile.sparksSound != null)
            {
                source.clip = profile.sparksSound;
                source.loop = true;
                source.Play();
            }
        }
    }

    public void PlayMixingDeskAudio(string profileName, bool play)
    {
        if (!profileLookup.TryGetValue(profileName, out InteractionSoundProfile profile))
        {
            Debug.LogWarning($"Profile {profileName} not found");
            return;
        }

        if (profile.interactionType != InteractionType.MixingDesk)
        {
            Debug.LogWarning($"Profile {profileName} is not a MixingDesk type");
            return;
        }

        // Validate required components with non-fatal logging
        if (profile.leftSpeakerAudio == null)
            Debug.LogWarning($"[{profileName}] Left speaker audio clip is missing");
        if (profile.rightSpeakerAudio == null)
            Debug.LogWarning($"[{profileName}] Right speaker audio clip is missing");
        if (profile.leftSpeakerSource == null)
            Debug.LogWarning($"[{profileName}] Left speaker AudioSource is missing");
        if (profile.rightSpeakerSource == null)
            Debug.LogWarning($"[{profileName}] Right speaker AudioSource is missing");

        if (profile.leftSpeakerAudio == null || profile.rightSpeakerAudio == null ||
            profile.leftSpeakerSource == null || profile.rightSpeakerSource == null)
        {
            return;
        }

        if (play)
        {
            // Set the output mixer group for both sources
            if (profile.mixerGroupOutput != null)
            {
                profile.leftSpeakerSource.outputAudioMixerGroup = profile.mixerGroupOutput;
                profile.rightSpeakerSource.outputAudioMixerGroup = profile.mixerGroupOutput;
            }

            // Configure both sources
            profile.leftSpeakerSource.clip = profile.leftSpeakerAudio;
            profile.rightSpeakerSource.clip = profile.rightSpeakerAudio;
            
            // Ensure both sources are at the start
            profile.leftSpeakerSource.time = 0;
            profile.rightSpeakerSource.time = 0;

            // Schedule both sources to start on the next audio frame for sample accuracy
            double startTime = AudioSettings.dspTime + 0.1; // Small delay to ensure scheduling
            profile.leftSpeakerSource.PlayScheduled(startTime);
            profile.rightSpeakerSource.PlayScheduled(startTime);
        }
        else
        {
            profile.leftSpeakerSource.Stop();
            profile.rightSpeakerSource.Stop();
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure interaction profiles have unique names
        if (interactionProfiles != null)
        {
            HashSet<string> names = new HashSet<string>();
            foreach (var profile in interactionProfiles)
            {
                if (profile != null && !string.IsNullOrEmpty(profile.interactionName))
                {
                    if (!names.Add(profile.interactionName))
                    {
                        Debug.LogWarning($"Duplicate interaction profile name found: {profile.interactionName}");
                    }
                }
            }
        }
    }

    [ContextMenu("Debug Interaction Profiles")]
    private void DebugInteractionProfiles()
    {
        if (interactionProfiles == null)
        {
            Debug.Log("No interaction profiles assigned.");
            return;
        }

        foreach (var profile in interactionProfiles)
        {
            if (profile == null) continue;
            
            Debug.Log($"Interaction: {profile.interactionName}\n" +
                     $"Fire Loop: {(profile.fireLoop != null ? profile.fireLoop.name : "none")}");
        }
    }
    #endif
} 