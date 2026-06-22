using UnityEngine;
using UnityEngine.Audio;

public class UIAudioManager : MonoBehaviour
{
    [System.Serializable]
    public class AudioSourceSetup
    {
        [Tooltip("AudioSource component for this category")]
        public AudioSource audioSource;
        [Tooltip("AudioMixerGroup for routing audio")]
        public AudioMixerGroup mixerGroup;
    }

    [System.Serializable]
    public class UISounds
    {
        [Header("Menu Sounds")]
        [SerializeField] private AudioSourceSetup menuAudioSetup;
        [Tooltip("Sound played when opening or showing a menu panel")]
        public AudioClip menuToggleOn;
        [Tooltip("Sound played when closing or hiding a menu panel")]
        public AudioClip menuToggleOff;
        [Tooltip("Master volume for all menu-related sounds")]
        [Range(0f, 1f)] public float menuVolume = 1f;

        [Header("Button Sounds")]
        [SerializeField] private AudioSourceSetup buttonAudioSetup;
        [Tooltip("Sound played when pressing or enabling a button")]
        public AudioClip buttonToggleOn;
        [Tooltip("Sound played when releasing or disabling a button")]
        public AudioClip buttonToggleOff;
        [Tooltip("Master volume for all button interaction sounds")]
        [Range(0f, 1f)] public float buttonVolume = 0.8f;

        [Header("Notification Sounds")]
        [SerializeField] private AudioSourceSetup notificationAudioSetup;
        [Tooltip("Sound played when player completes any objective")]
        public AudioClip objectiveCompleted;
        [Tooltip("Sound played when all zombies are eliminated for the night")]
        public AudioClip allEnemiesCleared;
        [Tooltip("Master volume for all notification sounds")]
        [Range(0f, 1f)] public float notificationVolume = 1f;

        [Header("Pickup Sounds")]
        [SerializeField] private AudioSourceSetup pickupAudioSetup;
        [Tooltip("Sound played when collecting or picking up any item")]
        public AudioClip itemPickup;
        [Tooltip("Master volume for all pickup interaction sounds")]
        [Range(0f, 1f)] public float pickupVolume = 0.8f;

        [Header("Consumption Sounds")]
        [SerializeField] private AudioSourceSetup consumptionAudioSetup;
        [Tooltip("Sound played when eating fish")]
        public AudioClip fishEaten;
        [Tooltip("Sound played when eating from a food can")]
        public AudioClip foodCanEaten;
        [Tooltip("Sound played when using medicine")]
        public AudioClip medicineUsed;
        [Tooltip("Master volume for all consumption sounds")]
        [Range(0f, 1f)] public float consumptionVolume = 0.8f;

        [Header("Day/Night Sounds")]
        [SerializeField] private AudioSourceSetup dayNightAudioSetup;
        [Tooltip("Sound played when announcing a new day")]
        public AudioClip dayNumberSound;
        [Range(0f, 1f)] public float dayAnnouncementVolume = 1f;

        [Header("Level Up Sounds")]
        [SerializeField] private AudioSourceSetup levelUpAudioSetup;
        [Tooltip("Sound played when player levels up")]
        public AudioClip levelUpSound;
        [Range(0f, 1f)] public float levelUpVolume = 1f;

        // Getters for AudioSourceSetup
        public AudioSourceSetup MenuAudioSetup => menuAudioSetup;
        public AudioSourceSetup ButtonAudioSetup => buttonAudioSetup;
        public AudioSourceSetup NotificationAudioSetup => notificationAudioSetup;
        public AudioSourceSetup PickupAudioSetup => pickupAudioSetup;
        public AudioSourceSetup ConsumptionAudioSetup => consumptionAudioSetup;
        public AudioSourceSetup DayNightAudioSetup => dayNightAudioSetup;
        public AudioSourceSetup LevelUpAudioSetup => levelUpAudioSetup;
    }
    
    [Header("Menu Sounds")]
    [SerializeField] private AudioSourceSetup menuAudioSetup;
    [Tooltip("Sound played when opening or showing a menu panel")]
    public AudioClip menuToggleOn;
    [Tooltip("Sound played when closing or hiding a menu panel")]
    public AudioClip menuToggleOff;
    [Tooltip("Master volume for all menu-related sounds")]
    [Range(0f, 1f)] public float menuVolume = 1f;

    [Header("Button Sounds")]
    [SerializeField] private AudioSourceSetup buttonAudioSetup;
    [Tooltip("Sound played when pressing or enabling a button")]
    public AudioClip buttonToggleOn;
    [Tooltip("Sound played when releasing or disabling a button")]
    public AudioClip buttonToggleOff;
    [Tooltip("Master volume for all button interaction sounds")]
    [Range(0f, 1f)] public float buttonVolume = 0.8f;

    [Header("Notification Sounds")]
    [SerializeField] private AudioSourceSetup notificationAudioSetup;
    [Tooltip("Sound played when player completes any objective")]
    public AudioClip objectiveCompleted;
    [Tooltip("Sound played when all zombies are eliminated for the night")]
    public AudioClip allEnemiesCleared;
    [Tooltip("Master volume for all notification sounds")]
    [Range(0f, 1f)] public float notificationVolume = 1f;

    [Header("Pickup Sounds")]
    [SerializeField] private AudioSourceSetup pickupAudioSetup;
    [Tooltip("Sound played when collecting or picking up any item")]
    public AudioClip itemPickup;
    [Tooltip("Master volume for all pickup interaction sounds")]
    [Range(0f, 1f)] public float pickupVolume = 0.8f;

    [Header("Consumption Sounds")]
    [SerializeField] private AudioSourceSetup consumptionAudioSetup;
    [Tooltip("Sound played when eating fish")]
    public AudioClip fishEaten;
    [Tooltip("Sound played when eating from a food can")]
    public AudioClip foodCanEaten;
    [Tooltip("Sound played when using medicine")]
    public AudioClip medicineUsed;
    [Tooltip("Master volume for all consumption sounds")]
    [Range(0f, 1f)] public float consumptionVolume = 0.8f;

    [Header("Day/Night Sounds")]
    [SerializeField] private AudioSourceSetup dayNightAudioSetup;
    [Tooltip("Sound played when announcing a new day")]
    public AudioClip dayNumberSound;
    [Range(0f, 1f)] public float dayAnnouncementVolume = 1f;

    [Header("Level Up Sounds")]
    [SerializeField] private AudioSourceSetup levelUpAudioSetup;
    [Tooltip("Sound played when player levels up")]
    public AudioClip levelUpSound;
    [Range(0f, 1f)] public float levelUpVolume = 1f;

    private static UIAudioManager instance;
    public static UIAudioManager Instance { get { return instance; } }

    // Add this new public method for playing generic sounds
    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        // Use the button audio setup for generic sounds
        PlaySound(buttonAudioSetup, clip, volume);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        InitializeAudioSourceSetup(menuAudioSetup);
        InitializeAudioSourceSetup(buttonAudioSetup);
        InitializeAudioSourceSetup(notificationAudioSetup);
        InitializeAudioSourceSetup(pickupAudioSetup);
        InitializeAudioSourceSetup(consumptionAudioSetup);
        InitializeAudioSourceSetup(dayNightAudioSetup);
        InitializeAudioSourceSetup(levelUpAudioSetup);
    }

    private void InitializeAudioSourceSetup(AudioSourceSetup setup)
    {
        if (setup == null) return;
        
        if (setup.audioSource == null)
        {
            setup.audioSource = gameObject.AddComponent<AudioSource>();
        }

        setup.audioSource.playOnAwake = false;
        setup.audioSource.spatialBlend = 0f; // 2D sound

        if (setup.mixerGroup != null)
        {
            setup.audioSource.outputAudioMixerGroup = setup.mixerGroup;
        }
    }

    private void PlaySound(AudioSourceSetup setup, AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip != null && setup?.audioSource != null)
        {
            setup.audioSource.PlayOneShot(clip, volumeMultiplier);
        }
    }

    // Menu toggle sounds
    public void PlayMenuToggle(bool isOpening)
    {
        AudioClip clipToPlay = isOpening ? menuToggleOn : menuToggleOff;
        PlaySound(menuAudioSetup, clipToPlay, menuVolume);
    }

    // Button toggle sounds
    public void PlayButtonToggle(bool isTogglingOn)
    {
        AudioClip clipToPlay = isTogglingOn ? buttonToggleOn : buttonToggleOff;
        PlaySound(buttonAudioSetup, clipToPlay, buttonVolume);
    }

    // Notification sounds
    public void PlayObjectiveCompleted()
    {
        PlaySound(notificationAudioSetup, objectiveCompleted, notificationVolume);
    }

    public void PlayAllEnemiesCleared()
    {
        PlaySound(notificationAudioSetup, allEnemiesCleared, notificationVolume);
    }

    // Pickup sound
    public void PlayPickupSound()
    {
        PlaySound(pickupAudioSetup, itemPickup, pickupVolume);
    }

    // Consumption sounds
    public void PlayFishEatenSound()
    {
        PlaySound(consumptionAudioSetup, fishEaten, consumptionVolume);
    }

    public void PlayFoodCanEatenSound()
    {
        PlaySound(consumptionAudioSetup, foodCanEaten, consumptionVolume);
    }

    public void PlayMedicineUsedSound()
    {
        PlaySound(consumptionAudioSetup, medicineUsed, consumptionVolume);
    }

    public void PlayDayNumberSound()
    {
        PlaySound(dayNightAudioSetup, dayNumberSound, dayAnnouncementVolume);
    }

    public void PlayLevelUpSound()
    {
        PlaySound(levelUpAudioSetup, levelUpSound, levelUpVolume);
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (menuAudioSetup == null || buttonAudioSetup == null || notificationAudioSetup == null || pickupAudioSetup == null || consumptionAudioSetup == null || dayNightAudioSetup == null || levelUpAudioSetup == null) return;
        
        ValidateAudioSetup(menuAudioSetup, "Menu");
        ValidateAudioSetup(buttonAudioSetup, "Button");
        ValidateAudioSetup(notificationAudioSetup, "Notification");
        ValidateAudioSetup(pickupAudioSetup, "Pickup");
        ValidateAudioSetup(consumptionAudioSetup, "Consumption");
        ValidateAudioSetup(dayNightAudioSetup, "Day/Night");
        ValidateAudioSetup(levelUpAudioSetup, "Level Up");
    }

    private void ValidateAudioSetup(AudioSourceSetup setup, string categoryName)
    {
        if (setup == null) return;
        
        if (setup.audioSource == null)
            Debug.LogWarning($"UIAudioManager: {categoryName} AudioSource is missing!");
        if (setup.mixerGroup == null)
            Debug.LogWarning($"UIAudioManager: {categoryName} AudioMixerGroup is missing!");
    }
    #endif
} 