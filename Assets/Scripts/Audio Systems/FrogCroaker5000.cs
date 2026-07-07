using UnityEngine;
using System.Collections;

public class FrogCroaker5000 : MonoBehaviour
{
    [Header("Frog Sounds")]
    [SerializeField] private AudioClip[] frogCroaks;
    [SerializeField] private AudioSource audioSource;

    [Header("Croak Area")]
    [SerializeField] private Vector3 areaSize = new Vector3(30f, 0f, 30f);

    [Header("Timing")]
    [SerializeField] private float minTimeBetweenCroaks = 3f;
    [SerializeField] private float maxTimeBetweenCroaks = 8f;

    [Header("Variation")]
    [SerializeField] private float minVolume = 0.3f;
    [SerializeField] private float maxVolume = 0.7f;
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    private Coroutine croakRoutine;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 1f;
        }
    }

    private void OnEnable()
    {
        croakRoutine = StartCoroutine(CroakLoop());
    }

    private void OnDisable()
    {
        if (croakRoutine != null)
        {
            StopCoroutine(croakRoutine);
            croakRoutine = null;
        }
    }

    private IEnumerator CroakLoop()
    {
        while (enabled && gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenCroaks, maxTimeBetweenCroaks));

            PlayRandomCroak();
        }
    }

    private void PlayRandomCroak()
    {
        if (frogCroaks == null || frogCroaks.Length == 0 || audioSource == null)
            return;

        Vector3 randomPos = transform.position + new Vector3(
            Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
            0,
            Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f));

        audioSource.transform.position = randomPos;

        audioSource.pitch = Random.Range(minPitch, maxPitch);

        AudioClip clip = frogCroaks[Random.Range(0, frogCroaks.Length)];

        audioSource.PlayOneShot(clip, Random.Range(minVolume, maxVolume));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
}