using UnityEngine;
using System.Collections;

public class FireSourceAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] protected AudioSource loopAudioSource;
    [SerializeField] protected AudioSource ignitionAudioSource;
    [SerializeField] protected AudioClip fireLoop;
    [SerializeField] protected AudioClip igniteSound;
    
    [Header("Volume Settings")]
    [SerializeField] protected float maxVolume = 1f;
    [SerializeField] protected float fadeInDuration = 1f;

    [Header("Distance Settings")]
    [SerializeField] protected float minDistance = 1f;
    [SerializeField] protected float maxDistance = 8f;
    [SerializeField] protected bool isCampfire = false;
    
    protected bool isLit = false;
    protected float currentVolume = 0f;
    protected Coroutine fadeCoroutine;
    
    protected virtual void Awake()
    {
        if (loopAudioSource == null)
        {
            loopAudioSource = GetComponent<AudioSource>();
        }
        
        if (ignitionAudioSource == null)
        {
            ignitionAudioSource = gameObject.AddComponent<AudioSource>();
            ignitionAudioSource.playOnAwake = false;
            ignitionAudioSource.spatialBlend = 1f;
            ignitionAudioSource.minDistance = minDistance;
            ignitionAudioSource.maxDistance = maxDistance;
        }
        
        InitializeAudioSource();
        Reset();
    }
    
    protected virtual void InitializeAudioSource()
    {
        if (loopAudioSource != null)
        {
            loopAudioSource.clip = fireLoop;
            loopAudioSource.loop = true;
            loopAudioSource.volume = 0f;
            loopAudioSource.playOnAwake = false;
            loopAudioSource.spatialBlend = 1f;
            loopAudioSource.dopplerLevel = 0f;
            loopAudioSource.minDistance = minDistance;
            loopAudioSource.maxDistance = maxDistance;
            loopAudioSource.rolloffMode = AudioRolloffMode.Linear;
            loopAudioSource.priority = isCampfire ? 64 : 128;
        }
    }
    
    public virtual void Ignite()
    {
        if (isLit || loopAudioSource == null) return;
        
        isLit = true;
        
        if (igniteSound != null)
        {
            ignitionAudioSource.PlayOneShot(igniteSound);
        }
        
        if (fireLoop != null)
        {
            loopAudioSource.time = Random.Range(0f, fireLoop.length);
            loopAudioSource.Play();
            
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeInVolume());
        }
    }

    public virtual void Reset()
    {
        isLit = false;
        if (loopAudioSource != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            
            loopAudioSource.Stop();
            loopAudioSource.volume = 0f;
            currentVolume = 0f;
        }
    }
    
    protected IEnumerator FadeInVolume()
    {
        float elapsedTime = 0f;
        float startVolume = currentVolume;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            currentVolume = Mathf.Lerp(startVolume, maxVolume, elapsedTime / fadeInDuration);
            loopAudioSource.volume = currentVolume;
            yield return null;
        }
        
        loopAudioSource.volume = maxVolume;
        currentVolume = maxVolume;
    }

    private void OnValidate()
    {
        if (fireLoop == null)
        {
            //Debug.LogWarning($"[{gameObject.name}] No fire loop audio clip assigned!");
        }
        if (loopAudioSource == null)
        {
            loopAudioSource = GetComponent<AudioSource>();
            if (loopAudioSource == null)
            {
                //Debug.LogWarning($"[{gameObject.name}] No AudioSource component found!");
            }
        }

        // Adjust distances based on type
        if (isCampfire)
        {
            minDistance = 2f;
            maxDistance = 15f;
        }
        else
        {
            minDistance = 1f;
            maxDistance = 8f;
        }
    }

    private void OnDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
} 