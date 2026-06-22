using UnityEngine;

/*
 * AudioOcclusion.cs
 * 
 * Purpose: Simulates audio occlusion effects when obstacles are between source and listener
 * Used by: Audio sources that need realistic obstacle interaction
 * 
 * Key Features:
 * - Raycast-based obstacle detection
 * - Dynamic low-pass filter adjustment
 * - Smooth volume transitions
 * - Visual debugging in scene view
 * 
 * Performance Considerations:
 * - Efficient raycast usage
 * - Smooth parameter interpolation
 * - Caches component references
 * 
 * Dependencies:
 * - Requires AudioSource component
 * - Uses AudioLowPassFilter
 * - Needs properly configured layer masks
 * - Requires AudioListener in scene
 */


[RequireComponent(typeof(AudioSource))]
public class AudioOcclusion : MonoBehaviour
{
    [HideInInspector]
    public Transform player; // The GameObject with an AudioListener component will be assigned automatically
    
    [Tooltip("Layers that will occlude the audio")]
    public LayerMask occlusionLayers; // Assign layers which will be considered for occlusion
    [Tooltip("Max distance to check for occlusion. This is the maximum distance within which the occlusion system will check for updates to the occlusion of the audio.")]
    [HideInInspector]
    public float maxDistance = 50f; // Max distance to check for occlusion
    [HideInInspector]
    public AudioLowPassFilter lowPassFilter; // The low pass filter component
    [HideInInspector]
    public float transitionSpeed = 8f; // Speed at which the cutoff frequency changes
    [Range(0f, 1f)]
    public float occludedVolume = 0.5f; // Volume level when occluded (between 0 and 1)
    [Tooltip("Frequency to apply when the audio is occluded.")]
    public float occludedFrequency = 10000f; // Frequency when occluded

    private AudioSource audioSource;
    private float targetCutoffFrequency = 20000f; // Target cutoff frequency (not occluded state)
    private float targetVolume = 1f; // Target volume (not occluded state)

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Find and assign the AudioListener in the scene using the newer method
        AudioListener listener = FindAnyObjectByType<AudioListener>();
        if (listener != null)
        {
            player = listener.transform;
        }
        else
        {
            Debug.LogError("No AudioListener found in the scene.");
        }

        // Check if an AudioLowPassFilter is already attached, if not, add one
        lowPassFilter = GetComponent<AudioLowPassFilter>();
        if (lowPassFilter == null)
        {
            lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        }
        lowPassFilter.enabled = true; // Ensure it is enabled
        lowPassFilter.cutoffFrequency = 20000f; // Setting the LPF frequency to 20000Hz

        // Set the initial target volume
        targetVolume = audioSource.volume;

        // Set the occlusionLayers to 'Occluder' layer by default
        occlusionLayers = 1 << LayerMask.NameToLayer("Occluder");
    }

    void Update()
    {
        // Cast a ray from the audio source to the listener
        RaycastHit hit;
        Vector3 direction = player.position - transform.position;
        bool isOccluded = Physics.Raycast(transform.position, direction, out hit, maxDistance, occlusionLayers);

        // Check if the ray hits an object on the occlusion layer and the hit point is between the audio source and the player
        bool isActuallyOccluded = isOccluded && ((1 << hit.collider.gameObject.layer) & occlusionLayers) != 0 && hit.distance < direction.magnitude;

        // Draw the ray in the Scene view
        Debug.DrawLine(transform.position, player.position, isActuallyOccluded ? Color.red : Color.green);

        if (isActuallyOccluded)
        {
            //Debug.Log($"Ray hit {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            targetCutoffFrequency = occludedFrequency; // Set target cutoff frequency to occludedFrequency
            targetVolume = occludedVolume; // Set target volume to occludedVolume
        }
        else
        {
            targetCutoffFrequency = 20000f; // Set target cutoff frequency to 20000Hz
            targetVolume = 1f; // Set target volume to 1
        }

        // Gradually change the cutoff frequency towards the target
        lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, targetCutoffFrequency, Time.deltaTime * transitionSpeed);

        // Gradually change the volume towards the target
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * transitionSpeed);
    }




}
