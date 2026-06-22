using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    /*
     * DayNightCycle.cs
     * 
     * Purpose: Controls the day/night cycle system including lighting, colors, and environmental effects
     * Used by: Main game world, lighting system, environmental systems
     * 
     * Key Features:
     * - Manages time progression through day/night cycles
     * - Controls sun position, intensity, and color throughout the cycle
     * - Handles sky and cloud color transitions
     * - Controls ambient lighting changes
     * - Manages particle systems based on time of day
     * - Provides time-based events and state information
     * 
     * Time Phases:
     * - Night (nightTime to dawnStart)
     * - Dawn (dawnStart to dayStart)
     * - Day (dayTime to duskStart)
     * - Dusk (duskStart to nightStart)
     * 
     * Performance Considerations:
     * - Uses interval-based updates for non-critical visual changes
     * - Caches particle system states
     * - Implements smooth transitions using interpolation
     * 
     * Dependencies:
     * - Requires configured Light component for sun
     * - Needs assigned skybox and cloud materials
     * - Optional particle system groups for environmental effects
     * - ObjectiveManager for time-based objectives
     * - ZombieManager for night-time events
     */

    [Header("Time Settings")]
    private float dayLength = 100f;    // Duration of daytime in seconds
    private float nightLength = 200f;  // Duration of nighttime in seconds
    [SerializeField, Range(0f, 1f)] private float timeOfDay = 0.5f;  // Normalized time (0-1) representing current time of day
    [SerializeField] private int dayNumber = 1;  // Current day number, increments at dayTransitionTime
    [SerializeField] private float dayTransitionTime = 0.14f;  // Time of day when the day number increments
    [SerializeField] private float currentTimeMultiplier = 1f;  // Controls speed of time passage (1 = normal, 2 = double speed, etc.)

    [Header("Cycle Thresholds")]
    [SerializeField, Range(0f, 1f)] private float dawnStart = 0.14f;  // When dawn begins
    [SerializeField, Range(0f, 1f)] private float dawn = 0.21f;       // Dawn complete
    [SerializeField, Range(0f, 1f)] private float dayStart = 0.301f;   // Start transition to day angle
    [SerializeField, Range(0f, 1f)] private float dayTime = 0.386f;    // When full day begins
    [SerializeField, Range(0f, 1f)] private float duskStart = 0.635f;  // Start transition to dusk angle
    [SerializeField, Range(0f, 1f)] private float dusk = 0.726f;       // Dusk complete
    [SerializeField, Range(0f, 1f)] private float nightStart = 0.794f; // Start transition to night angle
    [SerializeField, Range(0f, 1f)] private float nightTime = 0.88f;   // When night begins

    [Header("Sun Settings")]
    [SerializeField] private Light sunLight;
    [SerializeField] private float sunIntensityDay = 1.5f;
    [SerializeField] private float sunIntensityDawn = 1.0f;
    [SerializeField] private float sunIntensityDusk = 1.2f;
    [SerializeField] private float sunIntensityNight = 0f;
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color dawnColor = new Color(1f, 0.8f, 0.5f, 1f);    // Warm orange
    [SerializeField] private Color duskColor = new Color(1f, 0.3f, 0.2f, 1f);    // Deep orange-red
    [SerializeField] private Color nightColor = new Color(0.1f, 0.1f, 0.3f, 1f); // Dark blue

    [Header("Ambient Light Settings")]
    [SerializeField] private Color ambientDayColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color ambientDawnColor = new Color(0.5f, 0.4f, 0.3f, 1f);
    [SerializeField] private Color ambientDuskColor = new Color(0.4f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color ambientNightColor = new Color(0.1f, 0.1f, 0.2f, 1f);
    [SerializeField] private float ambientDayIntensity = 1f;
    [SerializeField] private float ambientDawnIntensity = 0.7f;
    [SerializeField] private float ambientDuskIntensity = 0.5f;
    [SerializeField] private float ambientNightIntensity = 0.2f;

    [Header("Rotation Settings")]
    [SerializeField] private float dayAngle = 30f;
    [SerializeField] private float dawnAngle = 0f;     // Angle during dawn
    [SerializeField] private float duskAngle = 180f;   // Angle during dusk
    [SerializeField] private float nightAngle = 260f;
    //[SerializeField, Range(0f, 1f)] private float transitionSmoothness = 0.5f;

    [Header("Sky Settings")]
    [SerializeField] private Material skydomeMaterial;
    
    [Header("Day Sky Colors")]
    [SerializeField] private Color dayTopColor = new Color(0.4f, 0.6f, 1f, 1f);     // Light blue
    [SerializeField] private Color dayBottomColor = new Color(0.7f, 0.8f, 1f, 1f);  // Pale blue
    
    [Header("Dawn Sky Colors")]
    [SerializeField] private Color dawnTopColor = new Color(0.5f, 0.3f, 0.2f, 1f);     // Orange-ish
    [SerializeField] private Color dawnBottomColor = new Color(0.8f, 0.6f, 0.4f, 1f);  // Light orange
    
    [Header("Dusk Sky Colors")]
    [SerializeField] private Color duskTopColor = new Color(0.3f, 0.2f, 0.4f, 1f);     // Purple-ish
    [SerializeField] private Color duskBottomColor = new Color(0.6f, 0.4f, 0.3f, 1f);  // Orange-purple
    
    [Header("Night Sky Colors")]
    [SerializeField] private Color nightTopColor = new Color(0.1f, 0.1f, 0.2f, 1f);    // Dark blue
    [SerializeField] private Color nightBottomColor = new Color(0.2f, 0.2f, 0.3f, 1f); // Slightly lighter blue

    [Header("Cloud Settings")]
    [SerializeField] private Material cloudMaterial;
    
    [Header("Day Cloud Colors")]
    [SerializeField] private Color cloudDayColor = new Color(1f, 1f, 1f, 1f);         // White
    [SerializeField] private Color cloudDayEmission = new Color(0.5f, 0.5f, 0.5f, 1f); // Subtle white glow
    
    [Header("Dawn Cloud Colors")]
    [SerializeField] private Color cloudDawnColor = new Color(1f, 0.8f, 0.6f, 1f);    // Warm white/orange
    [SerializeField] private Color cloudDawnEmission = new Color(0.6f, 0.4f, 0.2f, 1f); // Warm glow
    
    [Header("Dusk Cloud Colors")]
    [SerializeField] private Color cloudDuskColor = new Color(0.9f, 0.6f, 0.4f, 1f);  // Orange-pink
    [SerializeField] private Color cloudDuskEmission = new Color(0.5f, 0.3f, 0.2f, 1f); // Orange glow
    
    [Header("Night Cloud Colors")]
    [SerializeField] private Color cloudNightColor = new Color(0.3f, 0.3f, 0.4f, 1f); // Dark blue-grey
    [SerializeField] private Color cloudNightEmission = new Color(0.1f, 0.1f, 0.15f, 1f); // Subtle blue glow

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem[] subBeamParticles;      // SubBeam particles
    [SerializeField] private ParticleSystem[] duskParticles;         // Dusk particles
    [SerializeField] private ParticleSystem[] environmentParticles;  // Leaves, butterflies, grass etc.
    [SerializeField] private ParticleSystem[] snowParticles;        // Snow particles
    [SerializeField] private ParticleSystem[] rainParticles;        // Rain particles

    [System.Serializable]
    private class ParticleSystemControls
    {
        public bool playDuringDawn = true;
        public bool playDuringDay = true;
        public bool playDuringDusk = false;
        public bool playDuringNight = false;
    }

    [Header("Particle System Controls")]
    [SerializeField] private ParticleSystemControls subBeamControls = new ParticleSystemControls();
    [SerializeField] private ParticleSystemControls duskParticleControls = new ParticleSystemControls();
    [SerializeField] private ParticleSystemControls environmentControls = new ParticleSystemControls();
    [SerializeField] private ParticleSystemControls snowControls = new ParticleSystemControls();
    [SerializeField] private ParticleSystemControls rainControls = new ParticleSystemControls();

    [Header("Dependencies")]
    [SerializeField] private ObjectiveManager objectiveManager;
    [SerializeField] private ZombieManager zombieManager;

    [Header("Time Critical Objectives")]
    [SerializeField] private TimeCriticalObjective[] timeCriticalObjectives;
    private TimeCriticalObjective m_CurrentTimeCriticalObjective;

    private float cycleDuration => dayLength + nightLength;
    private bool isNighttime => timeOfDay <= dawnStart || timeOfDay >= nightStart;
    private bool lastNightState = false;
    private bool shouldPauseTime = false;
    private bool m_IsTimePaused = false;
    //[SerializeField] private bool hasSetInitialMultiplier = false;
    private bool hasIncrementedDayToday = false;  // Flag to ensure we only increment once per cycle

    // Add these cached values
    private float lastUpdateTime;
    private const float UPDATE_INTERVAL = 0.1f; // Update visuals every 100ms
    private Color currentSkyTopColor;
    private Color currentSkyBottomColor;
    private Color currentCloudColor;
    private Color currentCloudEmission;
    private float currentAmbientIntensity;
    private Color currentAmbientColor;
    private ParticleSystemState[] particleStates;

    private struct ParticleSystemState
    {
        public bool isPlaying;
        public ParticleSystem system;
    }

    public delegate void NightStateChanged(bool isNight);
    public event NightStateChanged OnNightStateChanged;

    public delegate void DayChangedHandler(int newDay);
    public event DayChangedHandler OnDayChanged;

    private void Awake()
    {
        // Cache initial colors
        currentSkyTopColor = skydomeMaterial != null ? skydomeMaterial.GetColor("_ColorTop") : Color.black;
        currentSkyBottomColor = skydomeMaterial != null ? skydomeMaterial.GetColor("_ColorBottom") : Color.black;
        currentCloudColor = cloudMaterial != null ? cloudMaterial.GetColor("_Color") : Color.white;
        currentCloudEmission = cloudMaterial != null ? cloudMaterial.GetColor("_EmissionColor") : Color.black;
        currentAmbientColor = RenderSettings.ambientLight;
        currentAmbientIntensity = RenderSettings.ambientIntensity;

        InitializeParticleStates();
    }

    private void InitializeParticleStates()
    {
        // Initialize particle system states
        int totalParticleSystems = 
            (subBeamParticles?.Length ?? 0) +
            (duskParticles?.Length ?? 0) +
            (environmentParticles?.Length ?? 0) +
            (snowParticles?.Length ?? 0) +
            (rainParticles?.Length ?? 0);

        particleStates = new ParticleSystemState[totalParticleSystems];
        int index = 0;

        void InitializeGroup(ParticleSystem[] systems)
        {
            if (systems == null) return;
            foreach (var system in systems)
            {
                if (system != null)
                {
                    particleStates[index++] = new ParticleSystemState 
                    { 
                        isPlaying = system.isPlaying,
                        system = system 
                    };
                }
            }
        }

        InitializeGroup(subBeamParticles);
        InitializeGroup(duskParticles);
        InitializeGroup(environmentParticles);
        InitializeGroup(snowParticles);
        InitializeGroup(rainParticles);
    }

    private void Start()
    {
        if (sunLight == null)
            sunLight = GetComponent<Light>();

        if (objectiveManager == null)
        {
            objectiveManager = FindFirstObjectByType<ObjectiveManager>();
        }

        if (zombieManager == null)
        {
            zombieManager = FindFirstObjectByType<ZombieManager>();
        }

        UpdateCurrentTimeCriticalObjective();
        ValidateThresholds();
        UpdateAmbientLight();
    }

    private void UpdateCurrentTimeCriticalObjective()
    {
        m_CurrentTimeCriticalObjective = null;
        if (timeCriticalObjectives != null)
        {
            foreach (var objective in timeCriticalObjectives)
            {
                if (objective.dayNumber == dayNumber)
                {
                    m_CurrentTimeCriticalObjective = objective;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (m_IsTimePaused || shouldPauseTime) return;

        // Always update time every frame for smooth transitions
        UpdateTime();

        // Update visuals with interpolation
        UpdateVisuals();
    }

    private void UpdateTime()
    {
        float deltaTime = Time.deltaTime * currentTimeMultiplier;
        float timeStep = isNighttime ? deltaTime / nightLength : deltaTime / dayLength;

        bool wasBeforeTransition = timeOfDay < dayTransitionTime;
        bool wasNight = isNighttime;

        // Reset hasIncrementedDayToday when we're in the proper window
        if (timeOfDay >= nightTime || timeOfDay < dayTransitionTime)
        {
            if (hasIncrementedDayToday)
            {
                hasIncrementedDayToday = false;
            }
        }

        timeOfDay += timeStep;
        if (timeOfDay >= 1f)
        {
            timeOfDay = 0f;
        }

        // Check day transition
        if (!hasIncrementedDayToday && wasBeforeTransition && timeOfDay >= dayTransitionTime)
        {
            dayNumber++;
            hasIncrementedDayToday = true;
            OnDayChanged?.Invoke(dayNumber);
        }

        // Check night state change
        if (wasNight != isNighttime)
        {
            OnNightStateChanged?.Invoke(isNighttime);
        }
    }

    private void UpdateVisuals()
    {
        // Always update rotation for smooth shadows
        UpdateSunRotation();
        
        // Update other visuals at intervals but with smooth interpolation
        if (Time.time >= lastUpdateTime + UPDATE_INTERVAL)
        {
            // Calculate interpolation factor for smooth transitions
            float t = (Time.time - lastUpdateTime) / UPDATE_INTERVAL;
            t = Mathf.Clamp01(t);

            // Update materials with interpolation
            if (skydomeMaterial != null)
            {
                Color newTopColor, newBottomColor;
                GetCurrentSkyColors(out newTopColor, out newBottomColor);
                
                currentSkyTopColor = Color.Lerp(currentSkyTopColor, newTopColor, t);
                currentSkyBottomColor = Color.Lerp(currentSkyBottomColor, newBottomColor, t);
                
                skydomeMaterial.SetColor("_ColorTop", currentSkyTopColor);
                skydomeMaterial.SetColor("_ColorBottom", currentSkyBottomColor);
            }

            if (cloudMaterial != null)
            {
                Color newCloudColor, newCloudEmission;
                GetCurrentCloudColors(out newCloudColor, out newCloudEmission);
                
                currentCloudColor = Color.Lerp(currentCloudColor, newCloudColor, t);
                currentCloudEmission = Color.Lerp(currentCloudEmission, newCloudEmission, t);
                
                cloudMaterial.SetColor("_Color", currentCloudColor);
                cloudMaterial.SetColor("_EmissionColor", currentCloudEmission);
            }

            // Update sun intensity and color every frame for smoothness
            UpdateSunIntensity();
            UpdateSunColor();

            // Update ambient lighting with interpolation
            Color newAmbientColor;
            float newAmbientIntensity;
            GetCurrentAmbientLight(out newAmbientColor, out newAmbientIntensity);
            
            currentAmbientColor = Color.Lerp(currentAmbientColor, newAmbientColor, t);
            currentAmbientIntensity = Mathf.Lerp(currentAmbientIntensity, newAmbientIntensity, t);
            
            RenderSettings.ambientLight = currentAmbientColor;
            RenderSettings.ambientIntensity = currentAmbientIntensity;

            // Only update particle states at intervals since they don't need smooth transitions
            UpdateParticleSystems();

            lastUpdateTime = Time.time;
        }
    }

    private void UpdateParticleSystems()
    {
        string currentPhase = GetCurrentPhase();
        
        for (int i = 0; i < particleStates.Length; i++)
        {
            var state = particleStates[i];
            if (state.system == null) continue;

            bool shouldPlay = ShouldParticleSystemPlay(state.system, currentPhase);
            
            if (shouldPlay != state.isPlaying)
            {
                if (shouldPlay)
                    state.system.Play();
                else
                    state.system.Stop();
                
                particleStates[i].isPlaying = shouldPlay;
            }
        }
    }

    private bool ShouldParticleSystemPlay(ParticleSystem system, string currentPhase)
    {
        // Find which particle system array this belongs to
        ParticleSystemControls controls = null;
        
        // Check each array without using Contains
        if (IsParticleInArray(system, subBeamParticles))
            controls = subBeamControls;
        else if (IsParticleInArray(system, duskParticles))
            controls = duskParticleControls;
        else if (IsParticleInArray(system, environmentParticles))
            controls = environmentControls;
        else if (IsParticleInArray(system, snowParticles))
            controls = snowControls;
        else if (IsParticleInArray(system, rainParticles))
            controls = rainControls;

        if (controls == null) return false;

        switch (currentPhase)
        {
            case "Night":
            case "Night to Dawn":
                return controls.playDuringNight;
            case "Dawn Hold":
            case "Dawn to Day":
                return controls.playDuringDawn;
            case "Day":
                return controls.playDuringDay;
            case "Day to Dusk":
            case "Dusk Hold":
                return controls.playDuringDusk;
            case "Dusk to Night":
                return controls.playDuringNight;
            default:
                return false;
        }
    }

    private bool IsParticleInArray(ParticleSystem system, ParticleSystem[] array)
    {
        if (array == null || system == null) return false;
        
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == system) return true;
        }
        return false;
    }

    private void UpdateSunRotation()
    {
        if (sunLight == null) return;

        float targetAngle;
        float t;

        if (timeOfDay < dawnStart)
            targetAngle = nightAngle;
        else if (timeOfDay < dawn)
        {
            t = (timeOfDay - dawnStart) / (dawn - dawnStart);
            targetAngle = Mathf.LerpAngle(nightAngle, dawnAngle, SmoothTransition(t));
        }
        else if (timeOfDay < dayStart)
            targetAngle = dawnAngle;
        else if (timeOfDay < dayTime)
        {
            t = (timeOfDay - dayStart) / (dayTime - dayStart);
            targetAngle = Mathf.LerpAngle(dawnAngle, dayAngle, SmoothTransition(t));
        }
        else if (timeOfDay < duskStart)
            targetAngle = dayAngle;
        else if (timeOfDay < dusk)
        {
            t = (timeOfDay - duskStart) / (dusk - duskStart);
            targetAngle = Mathf.LerpAngle(dayAngle, duskAngle, SmoothTransition(t));
        }
        else if (timeOfDay < nightStart)
            targetAngle = duskAngle;
        else if (timeOfDay < nightTime)
        {
            t = (timeOfDay - nightStart) / (nightTime - nightStart);
            targetAngle = Mathf.LerpAngle(duskAngle, nightAngle, SmoothTransition(t));
        }
        else
            targetAngle = nightAngle;

        // Apply rotation
        sunLight.transform.rotation = Quaternion.Euler(targetAngle, 0f, 0f);
    }

    private void UpdateSunIntensity()
    {
        float intensity;
        float t;
        
        if (timeOfDay < dawnStart)
        {
            // Night
            intensity = sunIntensityNight;
        }
        else if (timeOfDay < dawn)
        {
            // Night to Dawn
            t = (timeOfDay - dawnStart) / (dawn - dawnStart);
            t = SmoothTransition(t);
            intensity = Mathf.Lerp(sunIntensityNight, sunIntensityDawn, t);
        }
        else if (timeOfDay < dayStart)
        {
            // Hold at Dawn
            intensity = sunIntensityDawn;
        }
        else if (timeOfDay < dayTime)
        {
            // Dawn to Day
            t = (timeOfDay - dayStart) / (dayTime - dayStart);
            t = SmoothTransition(t);
            intensity = Mathf.Lerp(sunIntensityDawn, sunIntensityDay, t);
        }
        else if (timeOfDay < duskStart)
        {
            // Day
            intensity = sunIntensityDay;
        }
        else if (timeOfDay < dusk)
        {
            // Day to Dusk
            t = (timeOfDay - duskStart) / (dusk - duskStart);
            t = SmoothTransition(t);
            intensity = Mathf.Lerp(sunIntensityDay, sunIntensityDusk, t);
        }
        else if (timeOfDay < nightStart)
        {
            // Hold at Dusk
            intensity = sunIntensityDusk;
        }
        else if (timeOfDay < nightTime)
        {
            // Dusk to Night
            t = (timeOfDay - nightStart) / (nightTime - nightStart);
            t = SmoothTransition(t);
            intensity = Mathf.Lerp(sunIntensityDusk, sunIntensityNight, t);
        }
        else
        {
            // Night
            intensity = sunIntensityNight;
        }
        
        sunLight.intensity = intensity;
    }

    private void UpdateSunColor()
    {
        Color targetColor;
        float t;
        
        if (timeOfDay < dawnStart)
        {
            // Night
            targetColor = nightColor;
        }
        else if (timeOfDay < dawn)
        {
            // Night to Dawn
            t = (timeOfDay - dawnStart) / (dawn - dawnStart);
            t = SmoothTransition(t);
            targetColor = Color.Lerp(nightColor, dawnColor, t);
        }
        else if (timeOfDay < dayStart)
        {
            // Hold at Dawn
            targetColor = dawnColor;
        }
        else if (timeOfDay < dayTime)
        {
            // Dawn to Day
            t = (timeOfDay - dayStart) / (dayTime - dayStart);
            t = SmoothTransition(t);
            targetColor = Color.Lerp(dawnColor, dayColor, t);
        }
        else if (timeOfDay < duskStart)
        {
            // Day
            targetColor = dayColor;
        }
        else if (timeOfDay < dusk)
        {
            // Day to Dusk
            t = (timeOfDay - duskStart) / (dusk - duskStart);
            t = SmoothTransition(t);
            targetColor = Color.Lerp(dayColor, duskColor, t);
        }
        else if (timeOfDay < nightStart)
        {
            // Hold at Dusk
            targetColor = duskColor;
        }
        else if (timeOfDay < nightTime)
        {
            // Dusk to Night
            t = (timeOfDay - nightStart) / (nightTime - nightStart);
            t = SmoothTransition(t);
            targetColor = Color.Lerp(duskColor, nightColor, t);
        }
        else
        {
            // Night
            targetColor = nightColor;
        }

        sunLight.color = targetColor;
    }

    private void UpdateAmbientLight()
    {
        Color targetColor;
        float targetIntensity;
        float t;

        if (timeOfDay < dawnStart)
        {
            // Night
            targetColor = ambientNightColor;
            targetIntensity = ambientNightIntensity;
        }
        else if (timeOfDay < dawn)
        {
            // Night to Dawn
            t = (timeOfDay - dawnStart) / (dawn - dawnStart);
            t = SmoothTransition(t);
            targetColor = Color.Lerp(ambientNightColor, ambientDawnColor, t);
            targetIntensity = Mathf.Lerp(ambientNightIntensity, ambientDawnIntensity, t);
        }
        else if (timeOfDay < dayStart)
        {
            // Hold at Dawn
            targetColor = ambientDawnColor;
            targetIntensity = ambientDawnIntensity;
        }
        else if (timeOfDay < dayTime)
        {
            // Dawn to Day
            t = (timeOfDay - dayStart) / (dayTime - dayStart);
            t = SmoothTransition(t);
            targetColor = Color.Lerp(ambientDawnColor, ambientDayColor, t);
            targetIntensity = Mathf.Lerp(ambientDawnIntensity, ambientDayIntensity, t);
        }
        else if (timeOfDay < duskStart)
        {
            // Day
            targetColor = ambientDayColor;
            targetIntensity = ambientDayIntensity;
        }
        else if (timeOfDay < dusk)
        {
            // Day to Dusk
            t = (timeOfDay - duskStart) / (dusk - duskStart);
            t = SmoothTransition(t);
            targetColor = Color.Lerp(ambientDayColor, ambientDuskColor, t);
            targetIntensity = Mathf.Lerp(ambientDayIntensity, ambientDuskIntensity, t);
        }
        else if (timeOfDay < nightStart)
        {
            // Hold at Dusk
            targetColor = ambientDuskColor;
            targetIntensity = ambientDuskIntensity;
        }
        else if (timeOfDay < nightTime)
        {
            // Dusk to Night
            t = (timeOfDay - nightStart) / (nightTime - nightStart);
            t = SmoothTransition(t);
            targetColor = Color.Lerp(ambientDuskColor, ambientNightColor, t);
            targetIntensity = Mathf.Lerp(ambientDuskIntensity, ambientNightIntensity, t);
        }
        else
        {
            // Night
            targetColor = ambientNightColor;
            targetIntensity = ambientNightIntensity;
        }

        RenderSettings.ambientLight = targetColor;
        RenderSettings.ambientIntensity = targetIntensity;
    }

    private void UpdateSkyColors()
    {
        if (skydomeMaterial == null) return;

        Color topColor;
        Color bottomColor;
        float t;

        if (timeOfDay < dawnStart)
        {
            // Night
            topColor = nightTopColor;
            bottomColor = nightBottomColor;
        }
        else if (timeOfDay < dawn)
        {
            // Night to Dawn
            t = (timeOfDay - dawnStart) / (dawn - dawnStart);
            t = SmoothTransition(t);
            topColor = Color.Lerp(nightTopColor, dawnTopColor, t);
            bottomColor = Color.Lerp(nightBottomColor, dawnBottomColor, t);
        }
        else if (timeOfDay < dayStart)
        {
            // Hold at Dawn
            topColor = dawnTopColor;
            bottomColor = dawnBottomColor;
        }
        else if (timeOfDay < dayTime)
        {
            // Dawn to Day
            t = (timeOfDay - dayStart) / (dayTime - dayStart);
            t = SmoothTransition(t);
            topColor = Color.Lerp(dawnTopColor, dayTopColor, t);
            bottomColor = Color.Lerp(dawnBottomColor, dayBottomColor, t);
        }
        else if (timeOfDay < duskStart)
        {
            // Day
            topColor = dayTopColor;
            bottomColor = dayBottomColor;
        }
        else if (timeOfDay < dusk)
        {
            // Day to Dusk
            t = (timeOfDay - duskStart) / (dusk - duskStart);
            t = SmoothTransition(t);
            topColor = Color.Lerp(dayTopColor, duskTopColor, t);
            bottomColor = Color.Lerp(dayBottomColor, duskBottomColor, t);
        }
        else if (timeOfDay < nightStart)
        {
            // Hold at Dusk
            topColor = duskTopColor;
            bottomColor = duskBottomColor;
        }
        else if (timeOfDay < nightTime)
        {
            // Dusk to Night
            t = (timeOfDay - nightStart) / (nightTime - nightStart);
            t = SmoothTransition(t);
            topColor = Color.Lerp(duskTopColor, nightTopColor, t);
            bottomColor = Color.Lerp(duskBottomColor, nightBottomColor, t);
        }
        else
        {
            // Night
            topColor = nightTopColor;
            bottomColor = nightBottomColor;
        }

        // Update the shader properties
        skydomeMaterial.SetColor("_ColorTop", topColor);
        skydomeMaterial.SetColor("_ColorBottom", bottomColor);
    }

    private void UpdateCloudColors()
    {
        if (cloudMaterial == null) return;

        Color targetColor;
        Color targetEmission;
        float t;

        if (timeOfDay < dawnStart)
        {
            // Night
            targetColor = cloudNightColor;
            targetEmission = cloudNightEmission;
        }
        else if (timeOfDay < dawn)
        {
            // Night to Dawn
            t = (timeOfDay - dawnStart) / (dawn - dawnStart);
            t = SmoothTransition(t);
            targetColor = Color.Lerp(cloudNightColor, cloudDawnColor, t);
            targetEmission = Color.Lerp(cloudNightEmission, cloudDawnEmission, t);
        }
        else if (timeOfDay < dayStart)
        {
            // Hold at Dawn
            targetColor = cloudDawnColor;
            targetEmission = cloudDawnEmission;
        }
        else if (timeOfDay < dayTime)
        {
            // Dawn to Day
            t = (timeOfDay - dayStart) / (dayTime - dayStart);
            t = SmoothTransition(t);
            targetColor = Color.Lerp(cloudDawnColor, cloudDayColor, t);
            targetEmission = Color.Lerp(cloudDawnEmission, cloudDayEmission, t);
        }
        else if (timeOfDay < duskStart)
        {
            // Day
            targetColor = cloudDayColor;
            targetEmission = cloudDayEmission;
        }
        else if (timeOfDay < dusk)
        {
            // Day to Dusk
            t = (timeOfDay - duskStart) / (dusk - duskStart);
            t = SmoothTransition(t);
            targetColor = Color.Lerp(cloudDayColor, cloudDuskColor, t);
            targetEmission = Color.Lerp(cloudDayEmission, cloudDuskEmission, t);
        }
        else if (timeOfDay < nightStart)
        {
            // Hold at Dusk
            targetColor = cloudDuskColor;
            targetEmission = cloudDuskEmission;
        }
        else if (timeOfDay < nightTime)
        {
            // Dusk to Night
            t = (timeOfDay - nightStart) / (nightTime - nightStart);
            t = SmoothTransition(t);
            targetColor = Color.Lerp(cloudDuskColor, cloudNightColor, t);
            targetEmission = Color.Lerp(cloudDuskEmission, cloudNightEmission, t);
        }
        else
        {
            // Night
            targetColor = cloudNightColor;
            targetEmission = cloudNightEmission;
        }

        // Update both the main color and emission color
        cloudMaterial.SetColor("_Color", targetColor);
        cloudMaterial.SetColor("_EmissionColor", targetEmission);
    }

    private float SmoothTransition(float t)
    {
        // Enhanced smoothing function for better transitions
        return t * t * (3f - 2f * t);
    }

    private void ValidateThresholds()
    {
        // Ensure thresholds are in correct order
        dawnStart = Mathf.Min(dawnStart, dawn);
        dawn = Mathf.Min(dawn, dayStart);
        dayStart = Mathf.Min(dayStart, dayTime);
        dayTime = Mathf.Min(dayTime, duskStart);
        duskStart = Mathf.Min(duskStart, dusk);
        dusk = Mathf.Min(dusk, nightStart);
        nightStart = Mathf.Min(nightStart, nightTime);
    }

    private string GetCurrentPhase()
    {
        if (timeOfDay < dawnStart) return "Night";
        if (timeOfDay < dawn) return "Night to Dawn";
        if (timeOfDay < dayStart) return "Dawn Hold";
        if (timeOfDay < dayTime) return "Dawn to Day";
        if (timeOfDay < duskStart) return "Day";
        if (timeOfDay < dusk) return "Day to Dusk";
        if (timeOfDay < nightStart) return "Dusk Hold";
        if (timeOfDay < nightTime) return "Dusk to Night";
        return "Night";
    }

    // Public methods for external control
    public void SetTimeOfDay(float newTime)
    {
        // If we're skipping from late night to early morning
        if (timeOfDay > nightTime && newTime < dayTransitionTime)
        {
            hasIncrementedDayToday = false;
        }
        
        timeOfDay = newTime;
    }

    public float GetTimeOfDay()
    {
        return timeOfDay;
    }

    public bool IsNight()
    {
        bool beforeDawn = timeOfDay <= dawnStart;
        bool afterNightStart = timeOfDay >= nightStart;
        bool isNighttime = beforeDawn || afterNightStart;
        
        if (isNighttime != lastNightState)
        {
            lastNightState = isNighttime;
        }
        
        return isNighttime;
    }

    private void UpdateParticleSystemGroup(ParticleSystem[] particles, ParticleSystemControls controls, string currentPhase)
    {
        if (particles == null) return;

        bool shouldPlay = false;

        // Determine if particles should play based on current phase and controls
        switch (currentPhase)
        {
            case "Night":
            case "Night to Dawn":
                shouldPlay = controls.playDuringNight;
                break;
            case "Dawn Hold":
            case "Dawn to Day":
                shouldPlay = controls.playDuringDawn;
                break;
            case "Day":
                shouldPlay = controls.playDuringDay;
                break;
            case "Day to Dusk":
            case "Dusk Hold":
                shouldPlay = controls.playDuringDusk;
                break;
            case "Dusk to Night":
                shouldPlay = controls.playDuringNight;
                break;
        }

        // Update particle systems
        foreach (var particle in particles)
        {
            if (particle == null) continue;

            if (shouldPlay && !particle.isPlaying)
            {
                particle.Play();
            }
            else if (!shouldPlay && particle.isPlaying)
            {
                particle.Stop();
            }
        }
    }

    public float GetDawnStartTime()
    {
        return dawnStart;
    }

    public bool IsFullNight()
    {
        // Only return true during actual night time (after nightTime 0.868), not during dusk or transition
        bool isNight = timeOfDay >= nightTime || timeOfDay <= dawnStart;
        return isNight;
    }

    // Add debug method to help verify timing
    private void LogTimePhase()
    {
        string phase;
        if (timeOfDay < dawnStart)
            phase = "Night";
        else if (timeOfDay < dawn)
            phase = "Night to Dawn";
        else if (timeOfDay < dayStart)
            phase = "Dawn";
        else if (timeOfDay < dayTime)
            phase = "Dawn to Day";
        else if (timeOfDay < duskStart)
            phase = "Day";
        else if (timeOfDay < dusk)
            phase = "Day to Dusk";
        else if (timeOfDay < nightStart)
            phase = "Dusk";
        else if (timeOfDay < nightTime)
            phase = "Dusk to Night";
        else
            phase = "Night";

        Debug.Log($"Time: {timeOfDay:F3}, Phase: {phase}, IsFullNight: {IsFullNight()}");
    }

    public float GetNightTime()
    {
        return nightTime;
    }

    public float GetDuskStartTime()
    {
        return duskStart;
    }

    public void SetTimeSpeedMultiplier(float _multiplier)
    {
        currentTimeMultiplier = Mathf.Max(0.1f, _multiplier);
        
        if (_multiplier > 0)
        {
            m_IsTimePaused = false;
        }
    }

    public float GetTimeSpeedMultiplier()
    {
        return currentTimeMultiplier;
    }

    public void PauseTime()
    {
        if (!m_IsTimePaused)
        {
            m_IsTimePaused = true;
        }
    }

    public void ResumeTime()
    {
        if (m_IsTimePaused)
        {
            m_IsTimePaused = false;
        }
    }

    public bool IsTimePaused()
    {
        return m_IsTimePaused;
    }

    // Add public methods to access day information
    public int GetCurrentDay()
    {
        return dayNumber;
    }

    // Add method to set day number (for initialization/testing)
    public void SetDayNumber(int day)
    {
        dayNumber = day;
        OnDayChanged?.Invoke(dayNumber);
    }

    public float GetAmbientNightIntensity()
    {
        return ambientNightIntensity;
    }

    public void SetAmbientNightIntensity(float intensity)
    {
        ambientNightIntensity = intensity;
        UpdateAmbientLight(); // Refresh the ambient lighting
    }

    // Add helper methods for getting current colors
    private void GetCurrentSkyColors(out Color topColor, out Color bottomColor)
    {
        float t;

        if (timeOfDay < dawnStart)
        {
            topColor = nightTopColor;
            bottomColor = nightBottomColor;
        }
        else if (timeOfDay < dawn)
        {
            t = SmoothTransition((timeOfDay - dawnStart) / (dawn - dawnStart));
            topColor = Color.Lerp(nightTopColor, dawnTopColor, t);
            bottomColor = Color.Lerp(nightBottomColor, dawnBottomColor, t);
        }
        else if (timeOfDay < dayStart)
        {
            topColor = dawnTopColor;
            bottomColor = dawnBottomColor;
        }
        else if (timeOfDay < dayTime)
        {
            t = SmoothTransition((timeOfDay - dayStart) / (dayTime - dayStart));
            topColor = Color.Lerp(dawnTopColor, dayTopColor, t);
            bottomColor = Color.Lerp(dawnBottomColor, dayBottomColor, t);
        }
        else if (timeOfDay < duskStart)
        {
            topColor = dayTopColor;
            bottomColor = dayBottomColor;
        }
        else if (timeOfDay < dusk)
        {
            t = SmoothTransition((timeOfDay - duskStart) / (dusk - duskStart));
            topColor = Color.Lerp(dayTopColor, duskTopColor, t);
            bottomColor = Color.Lerp(dayBottomColor, duskBottomColor, t);
        }
        else if (timeOfDay < nightStart)
        {
            topColor = duskTopColor;
            bottomColor = duskBottomColor;
        }
        else if (timeOfDay < nightTime)
        {
            t = SmoothTransition((timeOfDay - nightStart) / (nightTime - nightStart));
            topColor = Color.Lerp(duskTopColor, nightTopColor, t);
            bottomColor = Color.Lerp(duskBottomColor, nightBottomColor, t);
        }
        else
        {
            topColor = nightTopColor;
            bottomColor = nightBottomColor;
        }
    }

    private void GetCurrentCloudColors(out Color cloudColor, out Color cloudEmission)
    {
        float t;

        if (timeOfDay < dawnStart)
        {
            cloudColor = cloudNightColor;
            cloudEmission = cloudNightEmission;
        }
        else if (timeOfDay < dawn)
        {
            t = SmoothTransition((timeOfDay - dawnStart) / (dawn - dawnStart));
            cloudColor = Color.Lerp(cloudNightColor, cloudDawnColor, t);
            cloudEmission = Color.Lerp(cloudNightEmission, cloudDawnEmission, t);
        }
        else if (timeOfDay < dayStart)
        {
            cloudColor = cloudDawnColor;
            cloudEmission = cloudDawnEmission;
        }
        else if (timeOfDay < dayTime)
        {
            t = SmoothTransition((timeOfDay - dayStart) / (dayTime - dayStart));
            cloudColor = Color.Lerp(cloudDawnColor, cloudDayColor, t);
            cloudEmission = Color.Lerp(cloudDawnEmission, cloudDayEmission, t);
        }
        else if (timeOfDay < duskStart)
        {
            cloudColor = cloudDayColor;
            cloudEmission = cloudDayEmission;
        }
        else if (timeOfDay < dusk)
        {
            t = SmoothTransition((timeOfDay - duskStart) / (dusk - duskStart));
            cloudColor = Color.Lerp(cloudDayColor, cloudDuskColor, t);
            cloudEmission = Color.Lerp(cloudDayEmission, cloudDuskEmission, t);
        }
        else if (timeOfDay < nightStart)
        {
            cloudColor = cloudDuskColor;
            cloudEmission = cloudDuskEmission;
        }
        else if (timeOfDay < nightTime)
        {
            t = SmoothTransition((timeOfDay - nightStart) / (nightTime - nightStart));
            cloudColor = Color.Lerp(cloudDuskColor, cloudNightColor, t);
            cloudEmission = Color.Lerp(cloudDuskEmission, cloudNightEmission, t);
        }
        else
        {
            cloudColor = cloudNightColor;
            cloudEmission = cloudNightEmission;
        }
    }

    private void GetCurrentAmbientLight(out Color ambientColor, out float intensity)
    {
        float t;

        if (timeOfDay < dawnStart)
        {
            ambientColor = ambientNightColor;
            intensity = ambientNightIntensity;
        }
        else if (timeOfDay < dawn)
        {
            t = SmoothTransition((timeOfDay - dawnStart) / (dawn - dawnStart));
            ambientColor = Color.Lerp(ambientNightColor, ambientDawnColor, t);
            intensity = Mathf.Lerp(ambientNightIntensity, ambientDawnIntensity, t);
        }
        else if (timeOfDay < dayStart)
        {
            ambientColor = ambientDawnColor;
            intensity = ambientDawnIntensity;
        }
        else if (timeOfDay < dayTime)
        {
            t = SmoothTransition((timeOfDay - dayStart) / (dayTime - dayStart));
            ambientColor = Color.Lerp(ambientDawnColor, ambientDayColor, t);
            intensity = Mathf.Lerp(ambientDawnIntensity, ambientDayIntensity, t);
        }
        else if (timeOfDay < duskStart)
        {
            ambientColor = ambientDayColor;
            intensity = ambientDayIntensity;
        }
        else if (timeOfDay < dusk)
        {
            t = SmoothTransition((timeOfDay - duskStart) / (dusk - duskStart));
            ambientColor = Color.Lerp(ambientDayColor, ambientDuskColor, t);
            intensity = Mathf.Lerp(ambientDayIntensity, ambientDuskIntensity, t);
        }
        else if (timeOfDay < nightStart)
        {
            ambientColor = ambientDuskColor;
            intensity = ambientDuskIntensity;
        }
        else if (timeOfDay < nightTime)
        {
            t = SmoothTransition((timeOfDay - nightStart) / (nightTime - nightStart));
            ambientColor = Color.Lerp(ambientDuskColor, ambientNightColor, t);
            intensity = Mathf.Lerp(ambientDuskIntensity, ambientNightIntensity, t);
        }
        else
        {
            ambientColor = ambientNightColor;
            intensity = ambientNightIntensity;
        }
    }
}