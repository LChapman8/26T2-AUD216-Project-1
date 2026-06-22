using UnityEngine;

public class FirstTimeInteractionTracker : MonoBehaviour
{
    public static FirstTimeInteractionTracker Instance { get; private set; }

    [Header("Power and Lights")]
    [SerializeField] private bool hasElectricalBoxBeenUsed;
    [SerializeField] private bool hasLightsBeenTurnedOnFirstTime;
    [SerializeField] private bool hasMixingDeskBeenUsedFirstTime;

    [Header("Firesticks")]
    [SerializeField] private int swampFiresticksLit = 0;
    [SerializeField] private int grasslandsFiresticksLit = 0;
    [SerializeField] private int requiredSwampFiresticks = 3;
    [SerializeField] private int requiredGrasslandsFiresticks = 2;

    [Header("Fishing")]
    [SerializeField] private bool hasFishingRodBeenPickedUp;
    [SerializeField] private bool hasFirstFishBeenCaught;

    [Header("Objective IDs")]
    [SerializeField] private string electricalBoxObjectiveId = "FixStudioPower";
    [SerializeField] private string lightsObjectiveId = "TurnOnLights";
    [SerializeField] private string mixingDeskObjectiveId = "TestMixingDesk";
    [SerializeField] private string swampFiresticksObjectiveId = "LightFiresticks";
    [SerializeField] private string grasslandsFiresticksObjectiveId = "LightFiresticksGrasslands";
    [SerializeField] private string fishingRodObjectiveId = "PickupFishingRod";
    [SerializeField] private string catchFishObjectiveId = "CatchAFish";

    [Header("Tools")]
    [SerializeField] private bool hasAxeBeenPickedUp;
    [SerializeField] private string axeObjectiveId = "PickupAxe";

    [Header("Campfire")]
    [SerializeField] private bool hasCampfireBeenLit;
    [SerializeField] private string campfireObjectiveId = "LightCampfire";

    private ActionHandler actionHandler;
    private bool hasCompletedElectricalObjective = false;
    private bool hasCompletedLightsObjective = false;
    private bool hasCompletedMixingDeskObjective = false;
    private bool hasCompletedSwampFiresticksObjective = false;
    private bool hasCompletedGrasslandsFiresticksObjective = false;
    private bool hasCompletedFishingRodObjective = false;
    private bool hasCompletedFirstFishObjective = false;
    private bool hasCompletedAxeObjective = false;
    private bool hasCompletedCampfireObjective = false;

    private ObjectiveManager objectiveManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        actionHandler = FindFirstObjectByType<ActionHandler>();
        if (actionHandler == null)
        {
            Debug.LogError("No ActionHandler found in the scene!");
        }

        objectiveManager = FindFirstObjectByType<ObjectiveManager>();

        // Subscribe to objective added event
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveAdded += HandleObjectiveAdded;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from objective added event
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveAdded -= HandleObjectiveAdded;
        }
    }

    private void HandleObjectiveAdded(string objectiveId)
    {
        // Check if any pre-completed objectives are being added
        if (objectiveId == fishingRodObjectiveId && hasFishingRodBeenPickedUp)
        {
            ObjectiveManager.Instance.CompleteObjective(fishingRodObjectiveId);
        }
        else if (objectiveId == catchFishObjectiveId && hasFirstFishBeenCaught)
        {
            ObjectiveManager.Instance.CompleteObjective(catchFishObjectiveId);
        }
        else if (objectiveId == axeObjectiveId && hasAxeBeenPickedUp)
        {
            ObjectiveManager.Instance.CompleteObjective(axeObjectiveId);
        }
        else if (objectiveId == campfireObjectiveId && hasCampfireBeenLit)
        {
            ObjectiveManager.Instance.CompleteObjective(campfireObjectiveId);
        }
    }

    private void Update()
    {
        if (actionHandler != null)
        {
            hasElectricalBoxBeenUsed = actionHandler.HasElectricalBoxBeenUsed;
            hasLightsBeenTurnedOnFirstTime = actionHandler.HasLightsBeenTurnedOnFirstTime;
            hasMixingDeskBeenUsedFirstTime = actionHandler.HasMixingDeskBeenUsedFirstTime;

            CheckAndCompleteObjectives();
        }
    }

    public void OnFishingRodPickup()
    {
        hasFishingRodBeenPickedUp = true;
        if (!hasCompletedFishingRodObjective)
        {
            ObjectiveManager.Instance?.CompleteObjective(fishingRodObjectiveId);
            hasCompletedFishingRodObjective = true;
        }
    }

    public void OnFirstFishCaught()
    {
        hasFirstFishBeenCaught = true;
        if (!hasCompletedFirstFishObjective)
        {
            ObjectiveManager.Instance?.CompleteObjective(catchFishObjectiveId);
            hasCompletedFirstFishObjective = true;
        }
    }

    public void IncrementSwampFiresticks()
    {
        swampFiresticksLit++;
        CheckAndCompleteObjectives();
    }

    public void IncrementGrasslandsFiresticks()
    {
        grasslandsFiresticksLit++;
        CheckAndCompleteObjectives();
    }

    private void CheckAndCompleteObjectives()
    {
        if (ObjectiveManager.Instance == null) return;

        if (hasElectricalBoxBeenUsed && !hasCompletedElectricalObjective)
        {
            ObjectiveManager.Instance.CompleteObjective(electricalBoxObjectiveId);
            hasCompletedElectricalObjective = true;
        }

        if (hasLightsBeenTurnedOnFirstTime && !hasCompletedLightsObjective)
        {
            ObjectiveManager.Instance.CompleteObjective(lightsObjectiveId);
            hasCompletedLightsObjective = true;
        }

        if (hasMixingDeskBeenUsedFirstTime && !hasCompletedMixingDeskObjective)
        {
            ObjectiveManager.Instance.CompleteObjective(mixingDeskObjectiveId);
            hasCompletedMixingDeskObjective = true;
        }

        if (swampFiresticksLit >= requiredSwampFiresticks && !hasCompletedSwampFiresticksObjective)
        {
            ObjectiveManager.Instance.CompleteObjective(swampFiresticksObjectiveId);
            hasCompletedSwampFiresticksObjective = true;
        }

        if (grasslandsFiresticksLit >= requiredGrasslandsFiresticks && !hasCompletedGrasslandsFiresticksObjective)
        {
            ObjectiveManager.Instance.CompleteObjective(grasslandsFiresticksObjectiveId);
            hasCompletedGrasslandsFiresticksObjective = true;
        }
    }

    // Public methods to check states
    public bool HasUsedElectricalBox() => hasElectricalBoxBeenUsed;
    public bool HasTurnedOnLights() => hasLightsBeenTurnedOnFirstTime;
    public bool HasUsedMixingDesk() => hasMixingDeskBeenUsedFirstTime;
    public bool HasPickedUpFishingRod() => hasFishingRodBeenPickedUp;
    public bool HasCaughtFirstFish() => hasFirstFishBeenCaught;
    public int GetSwampFiresticksLit() => swampFiresticksLit;
    public int GetGrasslandsFiresticksLit() => grasslandsFiresticksLit;

    public void OnAxePickup()
    {
        hasAxeBeenPickedUp = true;
        if (!hasCompletedAxeObjective)
        {
            ObjectiveManager.Instance?.CompleteObjective(axeObjectiveId);
            hasCompletedAxeObjective = true;
        }
    }

    public bool HasPickedUpAxe() => hasAxeBeenPickedUp;

    public void OnCampfireLit()
    {
        hasCampfireBeenLit = true;
        if (!hasCompletedCampfireObjective)
        {
            ObjectiveManager.Instance?.CompleteObjective(campfireObjectiveId);
            hasCompletedCampfireObjective = true;
        }
    }

    public bool HasLitCampfire() => hasCampfireBeenLit;

    public void ResetAllTracking()
    {
        hasElectricalBoxBeenUsed = false;
        hasLightsBeenTurnedOnFirstTime = false;
        hasMixingDeskBeenUsedFirstTime = false;
        hasFishingRodBeenPickedUp = false;
        hasFirstFishBeenCaught = false;
        swampFiresticksLit = 0;
        grasslandsFiresticksLit = 0;
        hasCompletedElectricalObjective = false;
        hasCompletedLightsObjective = false;
        hasCompletedMixingDeskObjective = false;
        hasCompletedSwampFiresticksObjective = false;
        hasCompletedGrasslandsFiresticksObjective = false;
        hasCompletedFishingRodObjective = false;
        hasCompletedFirstFishObjective = false;
        hasAxeBeenPickedUp = false;
        hasCompletedAxeObjective = false;
        hasCampfireBeenLit = false;
        hasCompletedCampfireObjective = false;
    }
}