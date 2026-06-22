using UnityEngine;
using System.Collections.Generic;

public class WaveAudioManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveAudioProfile
    {
        [Header("Wave State Sounds")]
        public AudioClip waveStartSound;
        public AudioClip waveEndSound;
        public AudioClip waveClearedSound;
        public AudioClip waveFailedSound;
        [Range(0f, 1f)] public float stateVolume = 1f;

        [Header("Wave Countdown")]
        public AudioClip countdownBeepSound;
        public AudioClip countdownFinalBeepSound;
        [Range(0f, 1f)] public float countdownVolume = 1f;

        [Header("Wave Ambient")]
        public AudioClip waveAmbientLoop;
        public AudioClip intensityLoop;
        [Range(0f, 1f)] public float ambientVolume = 0.7f;
        [Range(0f, 1f)] public float intensityVolume = 0.5f;
    }

    [SerializeField] private WaveAudioProfile audioProfile;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource stateAudioSource;
    [SerializeField] private AudioSource countdownAudioSource;
    [SerializeField] private AudioSource ambientLoopSource;
    [SerializeField] private AudioSource intensityLoopSource;

    private void Awake()
    {
        InitializeAudioSources();
    }

    private void InitializeAudioSources()
    {
        // Initialize state audio source
        if (stateAudioSource == null)
        {
            stateAudioSource = gameObject.AddComponent<AudioSource>();
            stateAudioSource.playOnAwake = false;
        }

        // Initialize countdown audio source
        if (countdownAudioSource == null)
        {
            countdownAudioSource = gameObject.AddComponent<AudioSource>();
            countdownAudioSource.playOnAwake = false;
        }

        // Initialize ambient loop source
        if (ambientLoopSource == null)
        {
            ambientLoopSource = gameObject.AddComponent<AudioSource>();
            ambientLoopSource.loop = true;
            ambientLoopSource.playOnAwake = false;
        }

        // Initialize intensity loop source
        if (intensityLoopSource == null)
        {
            intensityLoopSource = gameObject.AddComponent<AudioSource>();
            intensityLoopSource.loop = true;
            intensityLoopSource.playOnAwake = false;
        }
    }

    public void PlayWaveStartSound()
    {
        if (audioProfile.waveStartSound != null)
        {
            stateAudioSource.PlayOneShot(audioProfile.waveStartSound, audioProfile.stateVolume);
        }
    }

    public void PlayWaveEndSound()
    {
        if (audioProfile.waveEndSound != null)
        {
            stateAudioSource.PlayOneShot(audioProfile.waveEndSound, audioProfile.stateVolume);
        }
    }

    public void PlayWaveClearedSound()
    {
        if (audioProfile.waveClearedSound != null)
        {
            stateAudioSource.PlayOneShot(audioProfile.waveClearedSound, audioProfile.stateVolume);
        }
    }

    public void PlayWaveFailedSound()
    {
        if (audioProfile.waveFailedSound != null)
        {
            stateAudioSource.PlayOneShot(audioProfile.waveFailedSound, audioProfile.stateVolume);
        }
    }

    public void PlayCountdownBeep(bool isFinal = false)
    {
        AudioClip beepSound = isFinal ? audioProfile.countdownFinalBeepSound : audioProfile.countdownBeepSound;
        if (beepSound != null)
        {
            countdownAudioSource.PlayOneShot(beepSound, audioProfile.countdownVolume);
        }
    }

    public void StartWaveAmbient()
    {
        if (audioProfile.waveAmbientLoop != null)
        {
            ambientLoopSource.clip = audioProfile.waveAmbientLoop;
            ambientLoopSource.volume = audioProfile.ambientVolume;
            ambientLoopSource.Play();
        }
    }

    public void StopWaveAmbient()
    {
        ambientLoopSource.Stop();
    }

    public void StartIntensityLoop()
    {
        if (audioProfile.intensityLoop != null)
        {
            intensityLoopSource.clip = audioProfile.intensityLoop;
            intensityLoopSource.volume = audioProfile.intensityVolume;
            intensityLoopSource.Play();
        }
    }

    public void StopIntensityLoop()
    {
        intensityLoopSource.Stop();
    }

    public void SetIntensityVolume(float normalizedIntensity)
    {
        intensityLoopSource.volume = audioProfile.intensityVolume * normalizedIntensity;
    }
} 