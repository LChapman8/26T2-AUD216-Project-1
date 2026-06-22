using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSoundAtRandomPointInClip : MonoBehaviour
{
    public AudioClip clip;
    public AudioSource audioSource;

    void Start()
    {
    //audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.clip = clip;
    int randomStartTime = Random.Range(0, clip.samples - 1); //clip.samples is the lengh of the clip in samples
    audioSource.timeSamples = randomStartTime;
    audioSource.Play();
}
}
