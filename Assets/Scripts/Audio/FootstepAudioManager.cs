using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace Audio
{
    public class FootstepAudioManager : MonoBehaviour
    {
        [System.Serializable]
        public class SurfaceAudioProfile
        {
            public string surfaceType;
            public AudioClip[] footstepSounds;
            [Range(0f, 1f)] public float baseVolume = 1f;
        }

        [System.Serializable]
        public class MovementAudioProfile
        {
            public AudioClip[] jumpSounds;
            [Range(0f, 1f)] public float jumpVolume = 1f;

            public AudioClip[] landSounds;
            [Range(0f, 1f)] public float landVolume = 1f;
        }

        [Header("Surface Profiles")]
        [SerializeField] private SurfaceAudioProfile[] surfaceProfiles;
        [SerializeField] private MovementAudioProfile movementProfile;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;

        private Dictionary<string, SurfaceAudioProfile> surfaceLookup;

        private void Awake()
        {
            InitializeAudioSources();
            InitializeSurfaceLookup();
        }

        private void InitializeAudioSources()
        {
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;

            if (sfxMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = sfxMixerGroup;
            }
        }

        private void InitializeSurfaceLookup()
        {
            surfaceLookup = new Dictionary<string, SurfaceAudioProfile>();
            foreach (var profile in surfaceProfiles)
            {
                surfaceLookup[profile.surfaceType] = profile;
            }
        }

        public void PlayFootstep(string surfaceType)
        {
            if (!surfaceLookup.TryGetValue(surfaceType, out SurfaceAudioProfile profile))
                return;

            if (profile.footstepSounds == null || profile.footstepSounds.Length == 0) return;
            
            int randomIndex = Random.Range(0, profile.footstepSounds.Length);
            AudioClip soundClip = profile.footstepSounds[randomIndex];
            
            if (soundClip != null)
            {
                audioSource.volume = profile.baseVolume;
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(soundClip);
            }
        }

        public void PlayJumpSound()
        {
            if (movementProfile.jumpSounds == null || movementProfile.jumpSounds.Length == 0) return;
            
            int randomIndex = Random.Range(0, movementProfile.jumpSounds.Length);
            AudioClip soundClip = movementProfile.jumpSounds[randomIndex];
            
            if (soundClip != null)
            {
                audioSource.volume = movementProfile.jumpVolume;
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(soundClip);
            }
        }

        public void PlayLandSound()
        {
            if (movementProfile.landSounds == null || movementProfile.landSounds.Length == 0) return;
            
            int randomIndex = Random.Range(0, movementProfile.landSounds.Length);
            AudioClip soundClip = movementProfile.landSounds[randomIndex];
            
            if (soundClip != null)
            {
                audioSource.volume = movementProfile.landVolume;
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(soundClip);
            }
        }
    }
} 