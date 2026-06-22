using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/*
 * SettingsMenu.cs
 * 
 * Purpose: Manages game settings and configuration options
 * Used by: Options menu, game configuration
 * 
 * Key Features:
 * - Audio volume control
 * - Graphics quality settings
 * - Fullscreen toggle
 * - Settings persistence
 * 
 * Performance Considerations:
 * - Efficient settings application
 * - Minimal update overhead
 * 
 * Dependencies:
 * - AudioMixer for volume control
 * - Unity Quality Settings system
 * - Screen system integration
 */

public class SettingsMenu : MonoBehaviour
{

    public AudioMixer audioMixer;

    private void Start ()
    {
       
    }
    public void SetVolume (float volume)
    {
        audioMixer.SetFloat("volume", volume);
    }
    public void SetQuality (int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen (bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}
