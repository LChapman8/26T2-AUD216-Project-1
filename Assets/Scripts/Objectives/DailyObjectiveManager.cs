using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/*
 * DailyObjectiveManager.cs
 * 
 * Purpose: Manages daily objective progression and timing
 * Used by: Game progression, mission system
 * 
 * Key Features:
 * - Time-based objective activation
 * - Day/night cycle integration
 * - Prerequisite checking
 * - Time-critical objectives
 * - Wave system integration
 * 
 * Time Management:
 * - Objective timing thresholds
 * - Day progression tracking
 * - Time pausing system
 * - Dusk transition handling
 * 
 * Dependencies:
 * - DayNightCycle system
 * - ObjectiveManager
 * - WaveManager
 * - AnimationEventHandler
 * - DailyObjectiveData
 */

public class DailyObjectiveManager : MonoBehaviour
{
    private const float TIME_PAUSE_THRESHOLD = 0.85f;
    private const float LATE_TIME_PAUSE_THRESHOLD = 0.95f;
    private const float ZOMBIE_DEFENSE_ANNOUNCE_TIME = 0.88f;

    public static DailyObjectiveManager Instance { get; private set; }

    [SerializeField] private DailyObjectiveData dailyObjectives;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private AnimationEventHandler animationEventHandler;
    [SerializeField] private WaveManager waveManager;

    private Dictionary<int, List<DailyObjective>> objectivesByDay;
    private HashSet<string> activeObjectives = new HashSet<string>();
    private HashSet<string> m_DayObjectives = new HashSet<string>();
    private HashSet<string> m_NightObjectives = new HashSet<string>();
    private int currentDay = 1;
    private int lastDay = 1;
    private bool m_HasCompletedDayObjectives = false;

    [Header("Time Critical Objectives")]
    [SerializeField] private TimeCriticalObjective[] timeCriticalObjectives;
    private TimeCriticalObjective m_CurrentTimeCriticalObjective;

    private bool m_WasTimePaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeObjectives();
    }

    private void Start()
    {
        if (dayNightCycle == null)
            dayNightCycle = FindFirstObjectByType<DayNightCycle>();
            
        if (animationEventHandler == null)
            animationEventHandler = FindFirstObjectByType<AnimationEventHandler>();

        if (waveManager == null)
            waveManager = FindFirstObjectByType<WaveManager>();

        // Give a small delay to allow all targets to register
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        // Wait for next frame to allow all objects to initialize
        yield return new WaitForEndOfFrame();
        
        // Hide all daily objectives at start
        if (ObjectiveManager.Instance != null && dailyObjectives != null && dailyObjectives.objectives != null)
        {
            foreach (var objective in dailyObjectives.objectives)
            {
                if (ObjectiveManager.Instance.HasObjective(objective.objectiveId))
                {
                    ObjectiveManager.Instance.HideObjective(objective.objectiveId);
                }
            }
        }

        InitializeDayNightObjectives();
        UpdateTimeCriticalObjective();
    }

    private void InitializeObjectives()
    {
        objectivesByDay = new Dictionary<int, List<DailyObjective>>();
        
        if (dailyObjectives == null || dailyObjectives.objectives == null)
        {
          //  Debug.LogWarning("No daily objectives data assigned!");
            return;
        }

        // Group objectives by day
        foreach (var objective in dailyObjectives.objectives)
        {
            if (!objectivesByDay.ContainsKey(objective.dayNumber))
            {
                objectivesByDay[objective.dayNumber] = new List<DailyObjective>();
            }
            objectivesByDay[objective.dayNumber].Add(objective);
        }

        // Sort objectives by time for each day
        foreach (var dayObjectives in objectivesByDay.Values)
        {
            dayObjectives.Sort((a, b) => a.timeToAppear.CompareTo(b.timeToAppear));
        }
    }

    private void InitializeDayNightObjectives()
    {
        m_DayObjectives.Clear();
        m_NightObjectives.Clear();

        if (dailyObjectives == null || dailyObjectives.objectives == null) return;

        foreach (var objective in dailyObjectives.objectives)
        {
            if (objective.dayNumber == currentDay)
            {
                ObjectiveData objectiveData = ObjectiveManager.Instance.GetObjectiveData(objective.objectiveId);
                if (objectiveData != null)
                {
                    if (objectiveData.isNightObjective)
                    {
                        m_NightObjectives.Add(objective.objectiveId);
                    }
                    else
                    {
                        m_DayObjectives.Add(objective.objectiveId);
                    }
                }
            }
        }
    }

    private void UpdateTimeCriticalObjective()
    {
        m_CurrentTimeCriticalObjective = null;
        if (timeCriticalObjectives != null)
        {
            foreach (var timeCritical in timeCriticalObjectives)
            {
                if (timeCritical.dayNumber == currentDay)
                {
                    m_CurrentTimeCriticalObjective = timeCritical;
                    break;
                }
            }
        }
    }

    private bool AreTimeCriticalObjectivesComplete()
    {
        if (m_CurrentTimeCriticalObjective == null || 
            m_CurrentTimeCriticalObjective.requiredObjectiveIds == null || 
            m_CurrentTimeCriticalObjective.requiredObjectiveIds.Length == 0)
        {
            return true;
        }

        foreach (string objectiveId in m_CurrentTimeCriticalObjective.requiredObjectiveIds)
        {
            if (!ObjectiveManager.Instance.IsObjectiveCompleted(objectiveId))
            {
                return false;
            }
        }

        return true;
    }

    private void Update()
    {
        if (dayNightCycle == null || animationEventHandler == null) return;

        float currentTime = dayNightCycle.GetTimeOfDay();
        currentDay = animationEventHandler.GetCurrentDay();

        // Check if day has changed
        if (currentDay != lastDay)
        {
            ResetDailyObjectives();
            InitializeDayNightObjectives();
            m_HasCompletedDayObjectives = false;
            lastDay = currentDay;
            dayNightCycle.SetTimeSpeedMultiplier(1f);
            UpdateTimeCriticalObjective();
        }

        // Check time critical objectives
        if (m_CurrentTimeCriticalObjective != null)
        {
            if (currentTime >= m_CurrentTimeCriticalObjective.pauseTimeOfDay)
            {
                if (!AreTimeCriticalObjectivesComplete())
                {
                    dayNightCycle.PauseTime();
                }
                else
                {
                    dayNightCycle.ResumeTime();
                }
            }
        }

        // Check if we need to pause time at TIME_PAUSE_THRESHOLD
        if (currentTime >= TIME_PAUSE_THRESHOLD)
        {
            if (!AreAllDayObjectivesCompleted())
            {
                dayNightCycle.PauseTime();
            }
            else
            {
                dayNightCycle.ResumeTime();
                dayNightCycle.SetTimeSpeedMultiplier(1f);
            }
        }

        // Separate pause check for zombie defense at 0.95
        if (currentTime >= LATE_TIME_PAUSE_THRESHOLD)
        {
            dayNightCycle.PauseTime();
        }

        // Check for objective completion and trigger dusk transition
        if (!m_HasCompletedDayObjectives)
        {
            bool completed = AreDayObjectivesCompleted();
            if (completed)
            {
                m_HasCompletedDayObjectives = true;
                TriggerDuskTransition();
            }
        }

        // Handle regular objectives
        if (objectivesByDay.TryGetValue(currentDay, out var regularObjectives))
        {
            foreach (var objective in regularObjectives)
            {
                if (!activeObjectives.Contains(objective.objectiveId) && 
                    currentTime >= objective.timeToAppear &&
                    objective.objectiveId != ObjectiveConstants.ZOMBIE_DEFENCE)
                {
                    AddObjective(objective);
                }
            }
        }

        // Handle zombie defense objective separately
        if (objectivesByDay.TryGetValue(currentDay, out var nightObjectives))
        {
            foreach (var objective in regularObjectives)
            {
                if (objective.objectiveId != ObjectiveConstants.ZOMBIE_DEFENCE && 
                    !activeObjectives.Contains(objective.objectiveId) && 
                    currentTime >= objective.timeToAppear)
                {
                    AddObjective(objective);
                }
            }
        }
    }

    private void AddObjective(DailyObjective objective)
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveData objectiveData = ObjectiveManager.Instance.GetObjectiveData(objective.objectiveId);
            if (objectiveData != null && !ArePrerequisitesMet(objectiveData))
            {
                return;
            }

            Fish.ResetObjectiveFishCount(objective.objectiveId);
            ObjectiveManager.Instance.ShowObjective(objective.objectiveId);
            activeObjectives.Add(objective.objectiveId);
        }
    }

    private bool ArePrerequisitesMet(ObjectiveData objectiveData)
    {
        if (objectiveData.prerequisites == null || objectiveData.prerequisites.Length == 0)
        {
            return true;
        }

        foreach (ObjectiveData prerequisite in objectiveData.prerequisites)
        {
            if (prerequisite == null) continue;
            
            if (!ObjectiveManager.Instance.IsObjectiveCompleted(prerequisite.objectiveId))
            {
                return false;
            }
        }

        return true;
    }

    public bool AreAllDayObjectivesCompleted()
    {
        if (m_DayObjectives.Count == 0) return true;

        foreach (string objectiveId in m_DayObjectives)
        {
            if (!ObjectiveManager.Instance.IsObjectiveCompleted(objectiveId))
            {
                return false;
            }
        }
        return true;
    }

    private bool AreDayObjectivesCompleted()
    {
        if (m_DayObjectives.Count == 0) return false;

        foreach (string objectiveId in m_DayObjectives)
        {
            if (!ObjectiveManager.Instance.IsObjectiveCompleted(objectiveId))
            {
                return false;
            }
        }
        
        return true;
    }

    private void TriggerDuskTransition()
    {
        if (dayNightCycle != null)
        {
            float currentTime = dayNightCycle.GetTimeOfDay();
            float duskStartTime = dayNightCycle.GetDuskStartTime();
            
            if (duskStartTime > currentTime)
            {
                dayNightCycle.SetTimeOfDay(duskStartTime);
            }
            else
            {
                dayNightCycle.ResumeTime();
                dayNightCycle.SetTimeSpeedMultiplier(1f);
            }
        }
        else
        {
            Debug.LogWarning("DayNightCycle reference is missing!");
        }
    }

    public void ResetDailyObjectives()
    {
        activeObjectives.Clear();
        m_HasCompletedDayObjectives = false;
        Fish.ResetAllFishCounts();
        m_WasTimePaused = false;  // Reset pause state
        
        // Ensure zombie defense objective is properly reset
        if (ObjectiveManager.Instance != null)
        {
            // Only hide and reset if it was active
            if (ObjectiveManager.Instance.IsObjectiveActive(ObjectiveConstants.ZOMBIE_DEFENCE))
            {
                ObjectiveManager.Instance.HideObjective(ObjectiveConstants.ZOMBIE_DEFENCE);
                activeObjectives.Remove(ObjectiveConstants.ZOMBIE_DEFENCE);
             //   Debug.Log("[ZombieDefense] Objective reset for new day");
            }
            
            // Always reset WaveManager state
            if (waveManager != null)
            {
                waveManager.ResetZombieDefenseState();
            }
        }
    }

    public void CheckObjectives()
    {
        if (objectivesByDay.TryGetValue(currentDay, out var todaysObjectives))
        {
            float currentTime = dayNightCycle.GetTimeOfDay();
            foreach (var objective in todaysObjectives)
            {
                if (!activeObjectives.Contains(objective.objectiveId) && 
                    currentTime >= objective.timeToAppear)
                {
                    AddObjective(objective);
                }
            }
        }
    }

    private void PauseTime()
    {
        m_WasTimePaused = Time.timeScale == 0;
        Time.timeScale = 0;
    }

    private void ResumeTime()
    {
        // Only resume time if it wasn't already paused before we paused it
        if (!m_WasTimePaused)
        {
            Time.timeScale = 1;
        }
    }
}