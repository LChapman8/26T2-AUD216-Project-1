using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RandomSoundClip
{
    public AudioClip clip;

    [Tooltip("Volume range for the sound clip. Min and max values between 0.0 and 1.0.")]
    [RangeSlider(0.0f, 1.0f)]
    public RangeFloat volumeRange = new RangeFloat(0.8f, 1.0f);

    [Tooltip("Pitch range for the sound clip. Min and max values typically between 0.8 and 1.2.")]
    [RangeSlider(0.8f, 1.2f)]
    public RangeFloat pitchRange = new RangeFloat(0.95f, 1.05f);
}

public class AudioRandomizer : MonoBehaviour
{
    [Header("Sound Settings")]
    [Tooltip("List of sound clips with their respective volume and pitch ranges.")]
    public List<RandomSoundClip> sounds = new List<RandomSoundClip>();

    [Tooltip("Interval range in seconds between sound plays. Min and max values.")]
    [RangeSlider(0, 60)]
    public RangeFloat intervalRange = new RangeFloat(1, 30);

    [Header("Area Settings")]
    [Tooltip("Maximum distance in meters from the player at which the sounds can be heard.")]
    public float maxDistance = 50f;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;  // Make sound 3D
        StartCoroutine(PlayRandomSounds());
    }

    private IEnumerator PlayRandomSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(intervalRange.min, intervalRange.max));
            if (Vector3.Distance(Camera.main.transform.position, transform.position) <= maxDistance)
            {
                PlaySound();
            }
        }
    }

    private void PlaySound()
    {
        if (sounds.Count == 0) return;

        RandomSoundClip soundClip = sounds[Random.Range(0, sounds.Count)];
        audioSource.clip = soundClip.clip;
        audioSource.volume = Random.Range(soundClip.volumeRange.min, soundClip.volumeRange.max);
        audioSource.pitch = Random.Range(soundClip.pitchRange.min, soundClip.pitchRange.max);
        audioSource.Play();
    }
}
