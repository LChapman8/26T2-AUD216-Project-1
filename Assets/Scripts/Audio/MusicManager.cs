using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Linq;

[DisallowMultipleComponent]
public class MusicManager : MonoBehaviour
{
    public enum MusicState
    {
        Exploring,
        Fighting,
        Inside
    }

    public enum PlaybackMode
    {
        Random,         // Randomly select next clip
        Sequential     // Play clips in order
    }

    [System.Serializable]
    public class MusicTrack
    {
        [Tooltip("A descriptive name for this music track in the inspector")]
        public string trackName;

        [Tooltip("The game state this music track is associated with")]
        public MusicState state;

        [Tooltip("Array of music clips that will be played for this state. All clips should be the same length")]
        public AudioClip[] musicClips;

        [Tooltip("Volume level for this track (0.0 to 1.0)")]
        [Range(0f, 1f)]
        public float volume = 1.0f;

        [Tooltip("The Audio Mixer Group this track should output to")]
        public AudioMixerGroup outputMixerGroup;

        [Tooltip("If true, the track will continue playing clips. If false, it will stop after playing through the clips once")]
        public bool loop = true;

        [Tooltip("Sequential: Play clips in order as listed\nRandom: Randomly select the next clip to play")]
        public PlaybackMode playbackMode = PlaybackMode.Sequential;

        [Tooltip("When using Random mode, avoid playing the same clip twice in a row (only applies when there are 3 or more clips)")]
        public bool avoidRepeatInRandom = true;

        [Tooltip("List of states that this state cannot override. For example, if Inside is listed here, this track won't play if the Inside state is currently active")]
        public MusicState[] cannotOverride;

        [HideInInspector] public AudioSource source;
        [HideInInspector] public int currentClipIndex = 0;
        [HideInInspector] public int lastPlayedClipIndex = -1;
    }

    [Tooltip("Array of music tracks, each defining the music for a different game state")]
    public MusicTrack[] musicTracks;

    [Tooltip("Duration in seconds for crossfading between different music states")]
    public float crossFadeDuration = 2.0f;

    [Tooltip("The music state to play when the game starts")]
    public MusicState startingState = MusicState.Exploring;

    private MusicState currentState;
    private Coroutine fadeCoroutine;
    private bool isIndoors = false;
    private bool isFighting = false;
    private MusicState previousNonFightingState = MusicState.Exploring;

    private void Awake()
    {
        InitializeAudioSources();
    }

    private void Start()
    {
        previousNonFightingState = startingState;
        ChangeState(startingState, true);
    }

    public void SetIndoorState(bool indoor)
    {
        isIndoors = indoor;
        
        // Update previousNonFightingState when not fighting, so it reflects the correct state
        if (!isFighting)
        {
            previousNonFightingState = indoor ? MusicState.Inside : MusicState.Exploring;
        }
        
        UpdateMusicState();
    }

    public void SetFightingState(bool fighting)
    {
       // Debug.Log($"[MusicManager] Setting fighting state to: {fighting}");
        
        if (fighting)
        {
            // When entering combat, save the current state if it's not already Fighting
            if (currentState != MusicState.Fighting)
            {
                previousNonFightingState = currentState;
            }
            isFighting = true;
            UpdateMusicState();
        }
        else
        {
            // When exiting combat, restore to the previous non-fighting state
            isFighting = false;
            ChangeState(previousNonFightingState);
        }
    }

    private void UpdateMusicState()
    {
        MusicState newState;
        
        // Determine the desired state based on conditions
        if (isFighting)
        {
            newState = MusicState.Fighting;
        }
        else if (isIndoors)
        {
            newState = MusicState.Inside;
        }
        else
        {
            newState = MusicState.Exploring;
        }

        // Check if the new state is allowed to override the current state
        var currentTrack = musicTracks.FirstOrDefault(t => t.state == currentState);
        var newTrack = musicTracks.FirstOrDefault(t => t.state == newState);

        if (currentTrack != null && newTrack != null)
        {
            // If the current state is in the new state's cannotOverride list, don't change
            if (newTrack.cannotOverride != null && 
                newTrack.cannotOverride.Contains(currentState))
            {
                return;
            }
        }

        ChangeState(newState);
    }

    private void InitializeAudioSources()
    {
        foreach (var track in musicTracks)
        {
            track.source = gameObject.AddComponent<AudioSource>();
            if (track.musicClips != null && track.musicClips.Length > 0)
            {
                track.source.clip = track.musicClips[0];
            }
            track.source.loop = false;  // We'll handle looping ourselves
            track.source.volume = 0;
            track.source.playOnAwake = false;
            track.source.spatialBlend = 0f;
            
            if (track.outputMixerGroup != null)
            {
                track.source.outputAudioMixerGroup = track.outputMixerGroup;
            }
        }
    }

    private void Update()
    {
        // Check for tracks that need to schedule their next clip
        foreach (var track in musicTracks)
        {
            if (track.source.isPlaying && track.source.time >= track.source.clip.length - 0.1f)
            {
                ScheduleNextClip(track);
            }
        }
    }

    private void ScheduleNextClip(MusicTrack track)
    {
        if (track.musicClips == null || track.musicClips.Length == 0) return;
        if (!track.loop && track.currentClipIndex >= track.musicClips.Length - 1) return;

        int nextClipIndex;
        if (track.playbackMode == PlaybackMode.Sequential)
        {
            nextClipIndex = (track.currentClipIndex + 1) % track.musicClips.Length;
        }
        else // Random mode
        {
            if (track.musicClips.Length == 1)
            {
                nextClipIndex = 0;
            }
            else if (track.musicClips.Length == 2)
            {
                nextClipIndex = track.currentClipIndex == 0 ? 1 : 0;
            }
            else
            {
                do
                {
                    nextClipIndex = Random.Range(0, track.musicClips.Length);
                } while (track.avoidRepeatInRandom && nextClipIndex == track.currentClipIndex);
            }
        }

        // Calculate exact time to schedule the next clip
        double nextStartTime = AudioSettings.dspTime + (track.source.clip.length - track.source.time);
        
        track.lastPlayedClipIndex = track.currentClipIndex;
        track.currentClipIndex = nextClipIndex;
        track.source.clip = track.musicClips[nextClipIndex];
        track.source.PlayScheduled(nextStartTime);
    }

    private void StartTrack(MusicTrack track)
    {
        if (track.musicClips == null || track.musicClips.Length == 0) return;
        
        track.currentClipIndex = 0;
        track.lastPlayedClipIndex = -1;
        track.source.clip = track.musicClips[0];
        track.source.Play();
    }

    public void ChangeState(MusicState newState, bool immediate = false)
    {
        if (currentState == newState && !immediate) return;

       // Debug.Log($"[MusicManager] State changing from {currentState} to {newState}. Indoor: {isIndoors}, Fighting: {isFighting}");

        // Update previousNonFightingState when changing to a non-fighting state
        if (newState != MusicState.Fighting && !isFighting)
        {
            previousNonFightingState = newState;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (immediate)
        {
            SetStateVolumes(newState);
            currentState = newState;
            
            // Start playing all tracks (they'll be muted if not active)
            foreach (var track in musicTracks)
            {
                if (!track.source.isPlaying)
                    track.source.Play();
            }
        }
        else
        {
            fadeCoroutine = StartCoroutine(CrossFadeToState(newState));
        }
    }

    private void SetStateVolumes(MusicState state)
    {
        foreach (var track in musicTracks)
        {
            if (track.state == state)
            {
                track.source.volume = track.volume;
                if (!track.source.isPlaying)
                    StartTrack(track);
            }
            else
            {
                track.source.volume = 0f;
                track.source.Stop();  // Stop tracks that aren't being used
            }
        }
    }

    private IEnumerator CrossFadeToState(MusicState newState)
    {
        float elapsedTime = 0f;
        float[] startVolumes = new float[musicTracks.Length];
        float[] targetVolumes = new float[musicTracks.Length];
        
        // Start the new track if it's not playing
        var newTrack = musicTracks.FirstOrDefault(t => t.state == newState);
        if (newTrack != null && !newTrack.source.isPlaying)
        {
            StartTrack(newTrack);
        }
        
        for (int i = 0; i < musicTracks.Length; i++)
        {
            startVolumes[i] = musicTracks[i].source.volume;
            targetVolumes[i] = musicTracks[i].state == newState ? musicTracks[i].volume : 0f;
        }

        while (elapsedTime < crossFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / crossFadeDuration;

            for (int i = 0; i < musicTracks.Length; i++)
            {
                musicTracks[i].source.volume = Mathf.Lerp(startVolumes[i], targetVolumes[i], t);
            }

            yield return null;
        }

        // Stop all tracks except the current one
        foreach (var track in musicTracks)
        {
            if (track.state != newState)
            {
                track.source.Stop();
            }
            else
            {
                track.source.volume = track.volume;
            }
        }

        currentState = newState;
    }
} 