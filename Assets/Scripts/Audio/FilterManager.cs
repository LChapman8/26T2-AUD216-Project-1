using UnityEngine;

[RequireComponent(typeof(AudioLowPassFilter))]
public class FilterManager : MonoBehaviour
{
    private AudioLowPassFilter lowPassFilter;

    // Desired cutoff frequencies from different sources
    private float directionalCutoff = 22000f; // Default to no filtering
    private float occlusionCutoff = 22000f;   // Default to no filtering

    // Flag to determine if occlusion is active
    private bool isOccluded = false;

    void Awake()
    {
        lowPassFilter = GetComponent<AudioLowPassFilter>();
        if (lowPassFilter == null)
        {
            lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        }

        // Initialize with default values
        lowPassFilter.cutoffFrequency = 22000f;
    }

    void Update()
    {
        if (isOccluded)
        {
            // Occlusion takes priority
            lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, occlusionCutoff, Time.deltaTime * 10f);
        }
        else
        {
            // Apply directional audio settings
            lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, directionalCutoff, Time.deltaTime * 10f);
        }
    }

    /// <summary>
    /// Called by DirectionalAudioSource to set the desired cutoff frequency.
    /// </summary>
    public void SetDirectionalCutoff(float frequency)
    {
        directionalCutoff = Mathf.Clamp(frequency, 10f, 22000f);
    }

    /// <summary>
    /// Called by AudioOcclusion to set the desired cutoff frequency and occlusion state.
    /// </summary>
    public void SetOcclusionCutoff(float frequency, bool occluded)
    {
        occlusionCutoff = Mathf.Clamp(frequency, 10f, 22000f);
        isOccluded = occluded;
    }
}
