using UnityEngine;

public class MusicController2 : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource audioSource1; // Assign in Inspector

    [SerializeField]
    private AudioSource audioSource2; // Assign in Inspector

    [Header("Audio Feedback (Optional)")]
    [SerializeField]
    private AudioClip toggleSound; // Assign your toggle sound clip in Inspector

    [SerializeField]
    private AudioSource feedbackAudioSource; // Assign an AudioSource for playing toggleSound

    // Private flag to track the music state
    private bool isPlaying = false;

    /// <summary>
    /// Toggles the music on or off.
    /// Plays or stops both audio sources synchronously.
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
        }
        else
        {
            Debug.LogWarning("AudioSource1 or AudioSource2 is not assigned in MusicController2.");
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

        }
        else
        {
            Debug.LogWarning("AudioSource1 or AudioSource2 is not assigned in MusicController2.");
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
