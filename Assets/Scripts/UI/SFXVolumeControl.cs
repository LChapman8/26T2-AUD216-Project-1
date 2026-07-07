using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SFXVolumeControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;

    [Header("Exposed Mixer Parameter")]
    [Tooltip("The EXACT name of the exposed parameter in the Audio Mixer.")]
    [SerializeField] private string mixerParameterName;

    private const float MIN_DB = -80f;

    private void Start()
    {
        if (audioMixer == null)
        {
            Debug.LogError($"{name}: Audio Mixer is missing.");
            return;
        }

        if (volumeSlider == null)
        {
            Debug.LogError($"{name}: Slider is missing.");
            return;
        }

        // Read current mixer value
        if (audioMixer.GetFloat(mixerParameterName, out float currentDb))
        {
            float sliderValue = Mathf.Pow(10f, currentDb / 20f);
            volumeSlider.SetValueWithoutNotify(sliderValue);
        }
        else
        {
            Debug.LogError($"'{mixerParameterName}' is NOT an exposed parameter in the Audio Mixer.");
        }

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        if (value <= 0.0001f)
        {
            audioMixer.SetFloat(mixerParameterName, MIN_DB);
        }
        else
        {
            audioMixer.SetFloat(mixerParameterName, Mathf.Log10(value) * 20f);
        }
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
    }
}