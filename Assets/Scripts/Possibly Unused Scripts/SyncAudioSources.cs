using UnityEngine;

public class MusicController : MonoBehaviour
{
    // Public fields to assign the audio sources in the inspector
    [Header("Audio Sources")]
    public AudioSource audioSource1;
    public AudioSource audioSource2;

    // Public field to assign the toggle sound (optional)
    [Header("Audio Feedback")]
    public AudioClip toggleSound;
    public AudioSource feedbackAudioSource; // Assign an AudioSource for playing toggleSound

    // Private flag to track the music state
    private bool isPlaying = false;

    /// <summary>
    /// Toggles the music on or off.
    /// Plays both audio sources synchronously.
    /// </summary>
    public void ToggleMusic()
    {
        if (isPlaying)
        {
            StopMusic();
        }
        else
        {
            PlayMusic();
        }
    }

    /// <summary>
    /// Starts playing both audio sources synchronously.
    /// </summary>
    private void PlayMusic()
    {
        if (audioSource1 != null && audioSource2 != null)
        {
            double startTime = AudioSettings.dspTime + 0.1; // Schedule to start after 0.1 seconds

            // Ensure both audio sources start from the beginning
            audioSource1.Stop();
            audioSource2.Stop();
            audioSource1.time = 0;
            audioSource2.time = 0;

            // Schedule both audio sources to play at the same time
            audioSource1.PlayScheduled(startTime);
            audioSource2.PlayScheduled(startTime);

            isPlaying = true;

            // Optional: Play toggle sound for feedback
            if (toggleSound != null && feedbackAudioSource != null)
            {
                feedbackAudioSource.PlayOneShot(toggleSound);
            }

            Debug.Log("Music started playing synchronously.");
        }
        else
        {
            Debug.LogWarning("AudioSource1 or AudioSource2 is not assigned in MusicController.");
        }
    }

    /// <summary>
    /// Stops both audio sources.
    /// </summary>
    private void StopMusic()
    {
        if (audioSource1 != null && audioSource2 != null)
        {
            audioSource1.Stop();
            audioSource2.Stop();

            isPlaying = false;

            // Optional: Play toggle sound for feedback
            if (toggleSound != null && feedbackAudioSource != null)
            {
                feedbackAudioSource.PlayOneShot(toggleSound);
            }

            Debug.Log("Music stopped.");
        }
        else
        {
            Debug.LogWarning("AudioSource1 or AudioSource2 is not assigned in MusicController.");
        }
    }

    /// <summary>
    /// Returns the current music state.
    /// </summary>
    public bool IsMusicPlaying()
    {
        return isPlaying;
    }
}
