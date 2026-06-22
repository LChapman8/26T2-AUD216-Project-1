using UnityEngine;
using System.Collections;

public class AmbienceMusicManager : MonoBehaviour
{
    [System.Serializable]
    public class AreaAudio
    {
        public string areaName;
        [HideInInspector] public AudioSource ambienceSource;
        public AudioClip ambienceClip;
        [Range(0f, 1f)]
        public float ambienceVolume = 1.0f;
        [HideInInspector] public AudioSource musicSource;
        public AudioClip musicClip;
        [Range(0f, 1f)]
        public float musicVolume = 1.0f;
    }

    public AreaAudio[] areas = new AreaAudio[]
    {
        new AreaAudio { areaName = "Main" },
        new AreaAudio { areaName = "Swamp" },
        new AreaAudio { areaName = "Campfire" },
        new AreaAudio { areaName = "Cave" }
    };

    public float fadeDuration = 2.0f;
    [Tooltip("Set which area will play on game start")]
    public string startingArea;

    private string currentArea = "";
    private Coroutine fadeCoroutine;

    void Awake()
    {
        // Find and assign AudioSources
        foreach (var area in areas)
        {
            area.ambienceSource = GameObject.Find($"{area.areaName}Ambience")?.GetComponent<AudioSource>();
            area.musicSource = GameObject.Find($"{area.areaName}Music")?.GetComponent<AudioSource>();

            if (area.ambienceSource == null)
                Debug.LogWarning($"AudioManager: {area.areaName}Ambience AudioSource not found!");
            if (area.musicSource == null)
                Debug.LogWarning($"AudioManager: {area.areaName}Music AudioSource not found!");
        }
    }

    void Start()
    {
        // Initialize AudioSources with correct settings and assigned clips
        foreach (var area in areas)
        {
            InitializeAudioSource(area.ambienceSource, area.ambienceClip);
            InitializeAudioSource(area.musicSource, area.musicClip);
        }

        // Set the starting area if specified
        if (!string.IsNullOrEmpty(startingArea))
        {
            ChangeArea(startingArea, initialSetup: true);
        }
    }

    void Update()
    {
        // Update volumes directly for the current area
        var currentAreaAudio = System.Array.Find(areas, area => area.areaName == currentArea);
        if (currentAreaAudio != null)
        {
            UpdateSourceVolume(currentAreaAudio.ambienceSource, currentAreaAudio.ambienceVolume);
            UpdateSourceVolume(currentAreaAudio.musicSource, currentAreaAudio.musicVolume);
        }
    }

    private void InitializeAudioSource(AudioSource source, AudioClip clip)
    {
        if (source != null)
        {
            source.clip = clip;
            source.loop = true;
            source.volume = 0;
            source.dopplerLevel = 0;
            if (clip != null)
                source.Play();
        }
    }

    private void UpdateSourceVolume(AudioSource source, float targetVolume)
    {
        if (source != null && source.clip != null)
        {
            source.volume = targetVolume;
        }
    }

    public void ChangeArea(string newAreaName, bool initialSetup = false)
    {
        if (currentArea == newAreaName && !initialSetup)
        {
            return;
        }

        // Stop any ongoing fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // If it's initial setup, set volumes immediately; otherwise, start fade
        if (initialSetup)
        {
            SetAreaVolumes(newAreaName);
            currentArea = newAreaName;
        }
        else
        {
            fadeCoroutine = StartCoroutine(FadeBetweenAreas(currentArea, newAreaName));
        }
    }

    private void SetAreaVolumes(string areaName)
    {
        foreach (var area in areas)
        {
            float targetVolume = (area.areaName == areaName) ? 1 : 0;
            UpdateSourceVolume(area.ambienceSource, targetVolume * area.ambienceVolume);
            UpdateSourceVolume(area.musicSource, targetVolume * area.musicVolume);
        }
    }

    private IEnumerator FadeBetweenAreas(string oldAreaName, string newAreaName)
    {
        float elapsedTime = 0;
        var oldArea = System.Array.Find(areas, a => a.areaName == oldAreaName);
        var newArea = System.Array.Find(areas, a => a.areaName == newAreaName);

        if (oldArea != null && newArea != null)
        {
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;

                // Fade out old area
                if (oldArea.ambienceSource != null)
                    oldArea.ambienceSource.volume = Mathf.Lerp(oldArea.ambienceVolume, 0, t);
                if (oldArea.musicSource != null)
                    oldArea.musicSource.volume = Mathf.Lerp(oldArea.musicVolume, 0, t);

                // Fade in new area
                if (newArea.ambienceSource != null)
                    newArea.ambienceSource.volume = Mathf.Lerp(0, newArea.ambienceVolume, t);
                if (newArea.musicSource != null)
                    newArea.musicSource.volume = Mathf.Lerp(0, newArea.musicVolume, t);

                yield return null;
            }

            // Ensure final volumes are set exactly
            UpdateSourceVolume(oldArea.ambienceSource, 0);
            UpdateSourceVolume(oldArea.musicSource, 0);
            UpdateSourceVolume(newArea.ambienceSource, newArea.ambienceVolume);
            UpdateSourceVolume(newArea.musicSource, newArea.musicVolume);
        }

        currentArea = newAreaName;
    }
}