using UnityEngine;

public class CampfireInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem campfireParticleSystem;
    [SerializeField] private GameObject campfireLight;
    [SerializeField] private AudioSource campfireAudioSource;
    [SerializeField] private InteractionPromptUI promptUI;

    [Header("Interaction Settings")]
    [SerializeField] private string objectDisplayName = "Campfire";
    [SerializeField] private string inputKey = "E";
    [SerializeField] private string actionMessage = "light fire";

    [Header("Audio Settings")]
    [SerializeField] private string audioProfileName = "Campfire";

    private bool playerInRange = false;
    private bool isCampfireLit = false;
    private InteractionAudioManager audioManager;

    void Start()
    {
        if (promptUI == null)
            promptUI = FindFirstObjectByType<InteractionPromptUI>();

        audioManager = FindFirstObjectByType<InteractionAudioManager>();
        
        if (audioManager != null && campfireAudioSource != null)
        {
            audioManager.RegisterAudioSource(audioProfileName, campfireAudioSource);
        }

        if (campfireParticleSystem != null)
            campfireParticleSystem.Stop();

        if (campfireLight != null)
            campfireLight.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isCampfireLit)
        {
            LightCampfire();
        }
    }

    private void LightCampfire()
    {
        if (campfireParticleSystem != null)
            campfireParticleSystem.Play();

        if (campfireLight != null)
            campfireLight.SetActive(true);

        if (audioManager != null)
        {
            audioManager.IgniteFirestick(audioProfileName, campfireAudioSource);
        }

        isCampfireLit = true;
        
        // Notify tracker that campfire has been lit
        if (FirstTimeInteractionTracker.Instance != null)
        {
            FirstTimeInteractionTracker.Instance.OnCampfireLit();
        }
        
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.CompleteObjective("LightCampfire");
            
            if (WorldSpaceObjectiveManager.Instance != null)
            {
                WorldSpaceObjectiveManager.Instance.RemoveObjectiveMarker("LightCampfire", transform);
            }
        }
        
        EventManager.Instance.HideInteractionPrompt();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCampfireLit)
        {
            playerInRange = true;
            
            if (promptUI != null)
            {
                promptUI.ShowPrompt(objectDisplayName, inputKey, actionMessage);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null)
            {
                promptUI.HidePrompt();
            }
        }
    }
}