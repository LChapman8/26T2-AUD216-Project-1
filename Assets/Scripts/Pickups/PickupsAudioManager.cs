using UnityEngine;
using System.Collections.Generic;

/*
 * PickupsAudioManager.cs
 * 
 * Purpose: Manages audio feedback for pickup collection
 * Used by: Pickup system, sound effects
 * 
 * Key Features:
 * - Configurable sound variations
 * - Pitch randomization
 * - Sequential sound playback
 * - Special chest sounds
 * - Sound pooling
 * 
 * Audio Management:
 * - Sound cycling
 * - Volume control
 * - Pitch variation
 * - Sound categories
 * 
 * Dependencies:
 * - AudioSource component
 * - Sound effect assets
 * - Singleton pattern
 * - Scene persistence
 */

public class PickupsAudioManager : MonoBehaviour
{
    public static PickupsAudioManager Instance { get; private set; }

    [System.Serializable]
    public class PickupSound
    {
        public AudioClip clip;
        [Range(0.5f, 2.0f)]
        public float pitch = 1f;
    }

    [SerializeField] private List<PickupSound> pickupSounds = new List<PickupSound>();
    [SerializeField] private PickupSound chestPickupSound;

    private AudioSource audioSource;
    private int currentPickupSoundIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Ensure this GameObject is at the root of the hierarchy
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void PlayPickupSound()
    {
        if (audioSource != null && pickupSounds.Count > 0)
        {
            PickupSound soundToPlay = GetNextPickupSound();
            PlaySound(soundToPlay);
        }
    }

    public void PlayChestPickupSound()
    {
        if (audioSource != null && chestPickupSound.clip != null)
        {
            PlaySound(chestPickupSound);
        }
    }

    private void PlaySound(PickupSound sound)
    {
        audioSource.pitch = sound.pitch;
        audioSource.PlayOneShot(sound.clip);
    }

    private PickupSound GetNextPickupSound()
    {
        if (currentPickupSoundIndex >= pickupSounds.Count)
        {
            currentPickupSoundIndex = 0;
        }

        return pickupSounds[currentPickupSoundIndex++];
    }

    public void ResetPickupSounds()
    {
        currentPickupSoundIndex = 0;
    }
}
