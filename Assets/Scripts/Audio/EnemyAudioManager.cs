using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class EnemyAudioManager : MonoBehaviour
{
    public enum EnemyType
    {
        StandardEnemy,
        Boss
    }

    [System.Serializable]
    public class VocalizationSettings
    {
        [Tooltip("Individual vocalisation sounds")]
        public AudioClip[] vocalisationSounds;
    }

    [System.Serializable]
    public class EnemySoundProfile
    {
        [Tooltip("Unique identifier for this enemy - must match the name referenced in enemy scripts")]
        public string enemyName;
        
        [Tooltip("Type of enemy that determines base volume levels")]
        public EnemyType enemyType;

        [Header("Sounds")]
        [Tooltip("Random vocalisation sounds played while enemy is active")]
        public VocalizationSettings generalVocalisations;
        [Tooltip("Sounds played when enemy attacks - one will be randomly selected")]
        public AudioClip[] attackSounds;
        [Tooltip("Sounds played when enemy dies - one will be randomly selected")]
        public AudioClip[] deathSounds;

        [Header("Enemy Audio Setup")]
        [Tooltip("The enemy prefab this profile is for")]
        public GameObject enemyPrefab;
        [Tooltip("Minimum vocal delay in seconds")]
        public float minVocalDelay = 2f;
        [Tooltip("Maximum vocal delay in seconds")]
        public float maxVocalDelay = 6f;
        [Tooltip("Volume multiplier for vocalisations (0-1)")]
        [Range(0f, 1f)] public float vocalisationVolume = 1f;
        [Tooltip("Volume multiplier for attack sounds (0-1)")]
        [Range(0f, 1f)] public float attackVolume = 1f;
        [Tooltip("Volume multiplier for the death sound (0-1)")]
        [Range(0f, 1f)] public float deathVolume = 1f;
        [HideInInspector]
        [Tooltip("The AudioSource for vocalisation and death sounds")]
        public AudioSource vocalisationAudioSource;
        [HideInInspector]
        [Tooltip("The AudioSource for attack sounds")]
        public AudioSource attackAudioSource;
        [HideInInspector]
        public AudioMixerGroup vocalisationMixerGroup;
        [HideInInspector]
        public AudioMixerGroup attackMixerGroup;

        [Header("Timing")]
        [Tooltip("Delay in seconds before playing attack sound to align to the appropriate moment in the animation")]
        [Range(0f, 1f)] public float attackSoundDelay = 0.2f;
    }

    [SerializeField] private EnemySoundProfile[] enemyProfiles;
    private Dictionary<string, EnemySoundProfile> profileLookup;
    private Dictionary<AudioSource, Coroutine> activeVocalizations = new Dictionary<AudioSource, Coroutine>();

    [HideInInspector]
    [SerializeField] private int audioPoolSize = 10;
    [HideInInspector]
    [SerializeField] private float maxHearingDistance = 30f;
    private List<AudioSource> audioPool;
    private List<AudioRequest> pendingRequests = new List<AudioRequest>();

    private struct AudioRequest
    {
        public Vector3 position;
        public AudioClip clip;
        public float volume;
    }

    private Transform playerTransform;

    private void Awake()
    {
        InitializeAudioSystem();
        AutoAssignAudioSources();
        InitializeAudioPool();
    }

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void InitializeAudioSystem()
    {
        profileLookup = new Dictionary<string, EnemySoundProfile>();

        if (enemyProfiles == null) return;

        foreach (var profile in enemyProfiles)
        {
            if (profile == null || string.IsNullOrEmpty(profile.enemyName)) continue;
            profileLookup[profile.enemyName] = profile;
        }
    }

    private void InitializeAudioPool()
    {
        audioPool = new List<AudioSource>();
        for (int i = 0; i < audioPoolSize; i++)
        {
            GameObject obj = new GameObject($"AudioSource_{i}");
            obj.transform.parent = transform;
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioPool.Add(source);
        }
    }

    private void Update()
    {
        ProcessAudioRequests();
    }

    private void ProcessAudioRequests()
    {
        // Sort requests by distance to player
        pendingRequests.Sort((a, b) => 
            Vector3.Distance(a.position, playerTransform.position).CompareTo(
            Vector3.Distance(b.position, playerTransform.position)));

        foreach (var source in audioPool)
        {
            if (!source.isPlaying && pendingRequests.Count > 0)
            {
                var request = pendingRequests[0];
                pendingRequests.RemoveAt(0);
                
                source.transform.position = request.position;
                source.clip = request.clip;
                source.volume = request.volume;
                source.Play();
            }
        }
        pendingRequests.Clear();
    }

    public void RequestVocalization(Vector3 position, AudioClip clip, float volume)
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                return;
        }

        if (Vector3.Distance(position, playerTransform.position) > maxHearingDistance) return;
        
        pendingRequests.Add(new AudioRequest
        {
            position = position,
            clip = clip,
            volume = volume
        });
    }

    public void PlayVocalization(string enemyName, AudioSource source)
    {
        if (!profileLookup.TryGetValue(enemyName, out EnemySoundProfile profile)) return;

        if (profile.generalVocalisations.vocalisationSounds.Length > 0)
        {
            AudioClip randomClip = profile.generalVocalisations.vocalisationSounds[
                Random.Range(0, profile.generalVocalisations.vocalisationSounds.Length)];
            
            RequestVocalization(source.transform.position, randomClip, 
                profile.vocalisationVolume);
        }
    }

    public void PlayAttackSound(string enemyName, AudioSource source)
    {
        if (!profileLookup.TryGetValue(enemyName, out EnemySoundProfile profile))
            return;

        if (profile.attackSounds != null && profile.attackSounds.Length > 0)
        {
            AudioClip randomAttackSound = profile.attackSounds[Random.Range(0, profile.attackSounds.Length)];
            source.PlayOneShot(randomAttackSound, profile.attackVolume);
        }
    }

    public void PlayDeathSound(string enemyName, AudioSource source)
    {
        if (!profileLookup.TryGetValue(enemyName, out EnemySoundProfile profile))
            return;

        // Play random death sound if available
        if (profile.deathSounds != null && profile.deathSounds.Length > 0)
        {
            AudioClip randomDeathSound = profile.deathSounds[Random.Range(0, profile.deathSounds.Length)];
            source.PlayOneShot(randomDeathSound, profile.deathVolume);
        }
    }

    public float GetAttackSoundDelay(string enemyName)
    {
        if (!profileLookup.TryGetValue(enemyName, out EnemySoundProfile profile))
            return 0f;
        
        return profile.attackSoundDelay;
    }

    private void OnDestroy()
    {
        // Clean up all active coroutines
        foreach (var coroutine in activeVocalizations.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeVocalizations.Clear();
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure enemy profiles have unique names
        if (enemyProfiles != null)
        {
            HashSet<string> names = new HashSet<string>();
            foreach (var profile in enemyProfiles)
            {
                if (profile != null && !string.IsNullOrEmpty(profile.enemyName))
                {
                    if (!names.Add(profile.enemyName))
                    {
                        Debug.LogWarning($"Duplicate enemy profile name found: {profile.enemyName}");
                    }
                }
            }
        }
    }
    #endif

    // New: Coroutine to continuously play vocalizations for an enemy
    public void StartVocalizationLoop(string enemyName, AudioSource source)
    {
        if (source == null) return;
        // Stop any existing loop for this source first
        if (activeVocalizations.TryGetValue(source, out var existing) && existing != null)
        {
            StopCoroutine(existing);
        }
        var coroutine = StartCoroutine(VocalizationLoop(enemyName, source));
        activeVocalizations[source] = coroutine;
    }

    private IEnumerator VocalizationLoop(string enemyName, AudioSource source)
    {
        if (source == null) yield break;
        if (!profileLookup.TryGetValue(enemyName, out EnemySoundProfile profile))
            yield break;
        int lastIndex = -1;
        while (true)
        {
            // Exit if the AudioSource or its GameObject was destroyed
            if (source == null || source.Equals(null) || source.gameObject == null)
            {
                break;
            }

            // Ensure playerTransform is valid
            if (playerTransform == null || playerTransform.Equals(null))
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTransform = player.transform;
            }

            // If we still don't have a player, wait briefly and retry
            if (playerTransform == null || playerTransform.Equals(null))
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // Only play if the enemy is within audible range
            float distanceToPlayer = Vector3.Distance(source.transform.position, playerTransform.position);
            if (distanceToPlayer > maxHearingDistance)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            if (profile.generalVocalisations.vocalisationSounds != null && profile.generalVocalisations.vocalisationSounds.Length > 0)
            {
                int newIndex = Random.Range(0, profile.generalVocalisations.vocalisationSounds.Length);
                if (profile.generalVocalisations.vocalisationSounds.Length > 1)
                {
                    // Ensure the same clip is not played twice in a row
                    while (newIndex == lastIndex)
                        newIndex = Random.Range(0, profile.generalVocalisations.vocalisationSounds.Length);
                }
                lastIndex = newIndex;
                AudioClip clip = profile.generalVocalisations.vocalisationSounds[newIndex];
                if (clip != null)
                {
                    source.PlayOneShot(clip, profile.vocalisationVolume);
                }
            }
            // Wait for a random duration between the min and max vocal delays
            float waitTime = Random.Range(profile.minVocalDelay, profile.maxVocalDelay);
            yield return new WaitForSeconds(waitTime);
        }

        // Clean up tracking if we exit the loop
        if (activeVocalizations.ContainsKey(source))
        {
            activeVocalizations.Remove(source);
        }
    }

    public void StopVocalizationLoop(AudioSource source)
    {
        if (source == null) return;
        if (activeVocalizations.TryGetValue(source, out var coroutine) && coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        activeVocalizations.Remove(source);
    }

    // Auto-assign the audio source references based on the enemyPrefab.
    private void AutoAssignAudioSources()
    {
        if (enemyProfiles != null)
        {
            foreach (var profile in enemyProfiles)
            {
                if (profile.enemyPrefab != null)
                {
                    // Assign the parent's AudioSource for vocalisation and death sounds
                    if (profile.vocalisationAudioSource == null)
                    {
                        profile.vocalisationAudioSource = profile.enemyPrefab.GetComponent<AudioSource>();
                    }
                    // Assign the child's AudioSource (named "Audio Source - Attack") for attack sounds
                    if (profile.attackAudioSource == null)
                    {
                        Transform attackChild = profile.enemyPrefab.transform.Find("Audio Source - Attack");
                        if (attackChild != null)
                        {
                            profile.attackAudioSource = attackChild.GetComponent<AudioSource>();
                        }
                    }

                    // Ensure the output mixer groups are properly assigned
                    if (profile.vocalisationAudioSource != null && profile.vocalisationMixerGroup != null)
                        profile.vocalisationAudioSource.outputAudioMixerGroup = profile.vocalisationMixerGroup;

                    if (profile.attackAudioSource != null && profile.attackMixerGroup != null)
                        profile.attackAudioSource.outputAudioMixerGroup = profile.attackMixerGroup;
                }
            }
        }
    }

    // Public methods to get the correct mixer group for an enemy
    public AudioMixerGroup GetVocalisationMixerGroup(string enemyName)
    {
        if (profileLookup.TryGetValue(enemyName, out EnemySoundProfile profile))
            return profile.vocalisationMixerGroup;
        return null;
    }

    public AudioMixerGroup GetAttackMixerGroup(string enemyName)
    {
        if (profileLookup.TryGetValue(enemyName, out EnemySoundProfile profile))
            return profile.attackMixerGroup;
        return null;
    }
} 