using UnityEngine;

[System.Serializable]
public class SoundClip
{
    public AudioClip clip;
    public Vector2 volumeRange = new Vector2(0.8f, 1f);
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);
} 