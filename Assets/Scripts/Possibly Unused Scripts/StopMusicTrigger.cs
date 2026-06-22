using UnityEngine;
using System.Collections;

public class StopMusicTrigger : MonoBehaviour
{
    public AudioSource stopThisMusic; // Assign in the Inspector
    public float fadeTime = 2.0f; // Duration of fade, adjustable in Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && stopThisMusic != null)
        {
            // Start fade out coroutine
            StartCoroutine(FadeAudioSourceVolume(stopThisMusic, 0f, fadeTime));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && stopThisMusic != null)
        {
            // Start fade in coroutine
            StartCoroutine(FadeAudioSourceVolume(stopThisMusic, 1f, fadeTime));
        }
    }

    private IEnumerator FadeAudioSourceVolume(AudioSource source, float targetVolume, float duration)
    {
        float currentTime = 0;
        float startVolume = source.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }
}
