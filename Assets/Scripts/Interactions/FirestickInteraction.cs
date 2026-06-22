using UnityEngine;

public class FirestickInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem fireParticleSystem;
    [SerializeField] private AudioSource firestickAudioSource;
    [SerializeField] private InteractionPromptUI promptUI;

    [Header("Interaction Settings")]
    [SerializeField] private string objectDisplayName = "Firesticks";
    [SerializeField] private string inputKey = "E";
    [SerializeField] private string actionMessage = "light fire";
    [SerializeField] private bool isSwampFirestick = false;

    [Header("Audio Settings")]
    [SerializeField] private string audioProfileName = "Firesticks";

    private bool playerInRange = false;
    private bool isLit = false;
    private ParticleSystem.EmissionModule emission;
    private string objectiveId;
    private WorldSpaceObjectiveManager worldSpaceManager;
    private ObjectiveManager objectiveManager;
    private InteractionAudioManager audioManager;

    private void Awake()
    {
        // Cache the objective ID
        objectiveId = isSwampFirestick ? "LightFiresticks" : "LightFiresticksGrasslands";
        
        if (fireParticleSystem != null)
        {
            emission = fireParticleSystem.emission;
            emission.enabled = false;
            fireParticleSystem.Stop();
        }
    }

    private void Start()
    {
        // Cache references
        if (promptUI == null)
            promptUI = FindFirstObjectByType<InteractionPromptUI>();
            
        audioManager = FindFirstObjectByType<InteractionAudioManager>();
        
        // Add this firestick's AudioSource to the manager's profile
        if (audioManager != null && firestickAudioSource != null)
        {
            audioManager.RegisterAudioSource(audioProfileName, firestickAudioSource);
        }

        worldSpaceManager = WorldSpaceObjectiveManager.Instance;
        objectiveManager = ObjectiveManager.Instance;
    }

    private void Update()
    {
        if (playerInRange && !isLit && Input.GetKeyDown(KeyCode.E))
        {
            LightFirestick();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isLit && promptUI != null)
        {
            playerInRange = true;
            promptUI.ShowPrompt(objectDisplayName, inputKey, actionMessage);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && promptUI != null)
        {
            playerInRange = false;
            promptUI.HidePrompt();
        }
    }

    private void LightFirestick()
    {
        if (fireParticleSystem == null || isLit) return;

        // Light the fire
        emission.enabled = true;
        fireParticleSystem.Play();
        
        // Play audio through the InteractionAudioManager
        if (audioManager != null)
        {
            audioManager.IgniteFirestick(audioProfileName, firestickAudioSource);
        }
        
        isLit = true;

        // Handle objectives
        if (worldSpaceManager != null)
        {
            worldSpaceManager.RemoveObjectiveMarker(objectiveId, transform);
        }

        if (objectiveManager != null)
        {
            objectiveManager.IncrementFirestickProgress(objectiveId);
        }

        if (promptUI != null)
        {
            promptUI.HidePrompt();
        }
    }
} 