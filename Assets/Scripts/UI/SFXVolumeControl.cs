using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/*
 * SFXVolumeControl.cs
 * 
 * Purpose: Controls sound effect volume through UI
 * Used by: Audio settings, volume management
 * 
 * Key Features:
 * - Real-time volume adjustment
 * - Decibel conversion handling
 * - Slider-based control
 * - Initial state restoration
 * 
 * Performance Considerations:
 * - Efficient volume calculations
 * - Event-driven updates
 * - Smart parameter caching
 * 
 * Dependencies:
 * - AudioMixer system
 * - UI Slider component
 * - Requires proper mixer setup
 */
public class SFXVolumeControl : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string sfxVolumeParameter = "SFXVolume";
    [SerializeField] private Slider sfxVolumeSlider;

    private const float MIN_DB = -80f;

    private void Start()
    {
        if (audioMixer == null || sfxVolumeSlider == null)
        {
            Debug.LogError("SFXVolumeControl: Missing required references!");
            return;
        }

        // Initialize the slider value from the current mixer setting
        if (audioMixer.GetFloat(sfxVolumeParameter, out float currentVolume))
        {
            // Convert from decibels back to slider value (0-1)
            float sliderValue = Mathf.Pow(10f, currentVolume / 20f);
            sfxVolumeSlider.value = sliderValue;
        }

        // Add listener for slider changes
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetSFXVolume(float sliderValue)
    {
        if (audioMixer == null) return;

        // Protect against log10(0)
        if (sliderValue <= 0)
        {
            audioMixer.SetFloat(sfxVolumeParameter, MIN_DB);
            return;
        }

        // Convert slider value (0-1) to decibels
        float dbValue = Mathf.Log10(sliderValue) * 20f;
        
        // Clamp to minimum dB value
        dbValue = Mathf.Max(dbValue, MIN_DB);
        
        audioMixer.SetFloat(sfxVolumeParameter, dbValue);
    }

    private void OnValidate()
    {
        if (audioMixer == null)
            Debug.LogWarning("SFXVolumeControl: AudioMixer reference is missing!");
        if (sfxVolumeSlider == null)
            Debug.LogWarning("SFXVolumeControl: Slider reference is missing!");
    }
}
