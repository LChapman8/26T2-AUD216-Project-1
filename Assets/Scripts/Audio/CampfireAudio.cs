using UnityEngine;

public class CampfireAudio : FireSourceAudio
{
   // [Header("Campfire Specific Settings")]
   // [SerializeField] private float minDistance = 2f;
    //[SerializeField] private float maxDistance = 15f;
    
    protected override void InitializeAudioSource()
    {
        base.InitializeAudioSource();
        
        if (loopAudioSource != null)
        {
            loopAudioSource.minDistance = 2f;
            loopAudioSource.maxDistance = 15f;
            loopAudioSource.priority = 64;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        if (loopAudioSource != null)
        {
            loopAudioSource.spatialBlend = 1f;
            loopAudioSource.rolloffMode = AudioRolloffMode.Linear;
        }
    }
} 