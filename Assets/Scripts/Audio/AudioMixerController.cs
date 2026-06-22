using UnityEngine;
using UnityEngine.Audio;

/*
 * AudioMixerController.cs
 * 
 * Purpose: Manages audio mixer volume levels and parameter control.
 * Used by: All audio systems, UI volume controls
 * 
 * Provides centralized control over audio mixer groups including master,
 * music, ambience, SFX, and UI sounds. Handles logarithmic volume conversion
 * for natural-feeling volume control.
 * 
 * Performance Considerations:
 * - Caches mixer references
 * - Uses efficient parameter setting
 * - Minimizes redundant mixer updates
 * 
 * Dependencies:
 * - Requires properly configured AudioMixer asset
 * - UI volume control system
 * - Persistent audio settings
 */

public class AudioMixerController : MonoBehaviour
{
    [SerializeField] private AudioMixer mainMixer;
    
    // Mixer group volume parameters
    private const string MASTER_VOL = "MasterVolume";
    private const string MUSIC_VOL = "MusicVolume";
    private const string AMBIENCE_VOL = "AmbienceVolume";
    private const string SFX_VOL = "SFXVolume";
    private const string UI_VOL = "UIVolume";
    
    public void SetMasterVolume(float volume)
    {
        SetVolume(MASTER_VOL, volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        SetVolume(MUSIC_VOL, volume);
    }
    
    public void SetAmbienceVolume(float volume)
    {
        SetVolume(AMBIENCE_VOL, volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        SetVolume(SFX_VOL, volume);
    }
    
    public void SetUIVolume(float volume)
    {
        SetVolume(UI_VOL, volume);
    }
    
    private void SetVolume(string parameterName, float volume)
    {
        float mixerVolume = volume <= 0 ? -80f : Mathf.Log10(volume) * 20f;
        mainMixer.SetFloat(parameterName, mixerVolume);
    }
} 