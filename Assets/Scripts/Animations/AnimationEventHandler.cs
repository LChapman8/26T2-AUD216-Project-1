using UnityEngine;
using System.Collections;
using TMPro;

public class AnimationEventHandler : MonoBehaviour
{
    public static AnimationEventHandler Instance { get; private set; }

    public enum AnnouncementType
    {
        DayNumber,
        EnemiesCleared,
        PlayerDeath
    }

    [Header("References")]
    [SerializeField] private GameObject dayNumberUI; // Reference to the Day Number UI GameObject
    [SerializeField] private GameObject enemiesClearedUI; // Reference to the Enemies Cleared UI GameObject
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private TextMeshProUGUI dayNumberText;
    [SerializeField] private TextMeshProUGUI enemiesClearedText;

    [Header("Timing Settings")]
    [SerializeField] private float dayAnnouncementTime = 0.26f;

    private bool isDayAnnouncementPlaying = false;
    private int currentDay = 1;
    private bool hasDayBeenAnnounced = false;
    private bool wasNightTime = false;
    private bool hasIncrementedDay = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Ensure this GameObject is at the root of the hierarchy
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<WaveManager>();
        }

        if (dayNightCycle == null)
        {
            dayNightCycle = FindFirstObjectByType<DayNightCycle>();
        }

        // Initialize night state based on current time
        float timeOfDay = dayNightCycle.GetTimeOfDay();
        float dawnStart = dayNightCycle.GetDawnStartTime();
        wasNightTime = timeOfDay >= dayNightCycle.GetNightTime() || timeOfDay <= dawnStart;
        
//        Debug.Log($"Initial state - Time: {timeOfDay:F3}, DawnStart: {dawnStart:F3}, " +
//                  $"IsNight: {wasNightTime}, Day: {currentDay}");

        // If we start between dawn start and announcement time, set up for the announcement
        if (timeOfDay >= dawnStart && timeOfDay < dayAnnouncementTime)
        {
            hasIncrementedDay = true;
            hasDayBeenAnnounced = false;
        }
        // If we start after announcement time but before night, mark day 1 as announced
        else if (timeOfDay >= dayAnnouncementTime && timeOfDay < dayNightCycle.GetNightTime())
        {
            hasDayBeenAnnounced = true;
        }
    }

    private void Update()
    {
        if (dayNightCycle == null || isDayAnnouncementPlaying) return;

        float timeOfDay = dayNightCycle.GetTimeOfDay();
        float dawnStart = dayNightCycle.GetDawnStartTime();
        bool isCurrentlyNightTime = timeOfDay >= dayNightCycle.GetNightTime() || timeOfDay <= dawnStart;

        // Detect transition from night to dawn
        if (wasNightTime && timeOfDay >= dawnStart && timeOfDay < dayAnnouncementTime)
        {
            currentDay++;
            hasIncrementedDay = true;
            hasDayBeenAnnounced = false;
        }

        // Update night state for next frame
        wasNightTime = isCurrentlyNightTime;

        // Check if we should announce the new day
        if (timeOfDay >= dayAnnouncementTime && !hasDayBeenAnnounced && hasIncrementedDay)
        {
            hasDayBeenAnnounced = true;
            hasIncrementedDay = false;
            ShowDayAnnouncement();
        }
    }

    private void ShowDayAnnouncement()
    {
        if (isDayAnnouncementPlaying) return;

        if (dayNumberText != null)
        {
            dayNumberText.text = $"DAY {currentDay}";
        }
        
        if (dayNumberUI != null)
        {
            dayNumberUI.SetActive(true);
            if (enemiesClearedUI != null)
            {
                enemiesClearedUI.SetActive(false);
            }
        }
        isDayAnnouncementPlaying = true;
        StartCoroutine(PlayWithDelays(AnnouncementType.DayNumber));
    }

    /// <summary>
    /// Public method to force show the day announcement (for testing/debugging)
    /// </summary>
    public void ShowDayAnnouncementNow()
    {
        ShowDayAnnouncement();
    }

    public void ShowEnemiesClearedAnnouncement()
    {
        if (enemiesClearedText != null)
        {
            enemiesClearedText.text = "ENEMIES CLEARED!";
        }
        
        if (enemiesClearedUI != null)
        {
            enemiesClearedUI.SetActive(true);
            dayNumberUI?.SetActive(false); // Ensure other UI is hidden
        }
        isDayAnnouncementPlaying = true;
        StartCoroutine(PlayWithDelays(AnnouncementType.EnemiesCleared));
    }

    /// <summary>
    /// Shows a custom announcement message using the enemies cleared UI
    /// </summary>
    /// <param name="message">The custom message to display</param>
    public void ShowCustomAnnouncement(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return; // Don't show empty messages
        }

        if (enemiesClearedText != null)
        {
            enemiesClearedText.text = message;
        }
        
        if (enemiesClearedUI != null)
        {
            enemiesClearedUI.SetActive(true);
            dayNumberUI?.SetActive(false); // Ensure other UI is hidden
        }
        isDayAnnouncementPlaying = true;
        StartCoroutine(PlayWithDelays(AnnouncementType.EnemiesCleared));
    }

    private IEnumerator PlayWithDelays(AnnouncementType type)
    {
        GameObject targetUI = type == AnnouncementType.DayNumber ? dayNumberUI : enemiesClearedUI;
        if (targetUI != null)
        {
            Animator animator = targetUI.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                // Play the "In" animation state which we can see exists in the Animator Controller
                animator.Play("In", 0);
                
                // Optional debug log to confirm the animation is playing
   //             Debug.Log($"Playing 'In' animation on {targetUI.name}");
            }
            else
            {
                Debug.LogError($"Missing Animator or AnimatorController on {targetUI.name}");
            }
        }
        else
        {
            Debug.LogError($"Target UI is null for announcement type: {type}");
        }
        yield break;
    }

    public void OnAnimationComplete(AnnouncementType type)
    {
        StartCoroutine(CompleteWithDelay(type));
    }

    private IEnumerator CompleteWithDelay(AnnouncementType type)
    {
        if (type == AnnouncementType.DayNumber)
        {
            isDayAnnouncementPlaying = false;
            dayNumberUI?.SetActive(false);
        }
        else
        {
            isDayAnnouncementPlaying = false;
            enemiesClearedUI?.SetActive(false);
        }
        yield break;
    }

    public void PlayClearedSound()
    {
        waveManager?.PlayClearedSound();
    }

    public void PlayDayNumberSound()
    {
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlayDayNumberSound();
        }
    }

    public void TriggerAnimation()
    {
        if (!isDayAnnouncementPlaying)
        {
            if (dayNumberUI != null)
            {
                dayNumberUI.SetActive(true);
            }
            isDayAnnouncementPlaying = true;
            StartCoroutine(PlayWithDelays(AnnouncementType.DayNumber));
        }
    }

    public void SkipAnimation()
    {
        if (isDayAnnouncementPlaying)
        {
            StopAllCoroutines();
            isDayAnnouncementPlaying = false;
            if (dayNumberUI != null)
            {
                dayNumberUI.SetActive(false);
            }
        }
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }

    /// <summary>
    /// Sets the current day (for testing/debugging purposes)
    /// </summary>
    /// <param name="day">The day number to set</param>
    public void SetCurrentDay(int day)
    {
        currentDay = day;
        hasDayBeenAnnounced = false;
        hasIncrementedDay = false;
    }

    public void ShowDeathAnnouncement()
    {
        if (dayNumberText != null)
        {
            dayNumberText.text = "YOU DIED!";
        }
        
        if (dayNumberUI != null)
        {
            dayNumberUI.SetActive(true);
            enemiesClearedUI?.SetActive(false);
        }
        isDayAnnouncementPlaying = true;
        StartCoroutine(PlayWithDelays(AnnouncementType.PlayerDeath));
    }
}