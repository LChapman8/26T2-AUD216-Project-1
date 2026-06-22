using UnityEngine;
using System.Collections;

public class CameraShakeController : MonoBehaviour
{
    private static CameraShakeController s_Instance;
    public static CameraShakeController Instance => s_Instance;

    [Header("References")]
    [Tooltip("Reference to the camera that will be shaken")]
    [SerializeField] private Camera m_Camera;
    
    [Tooltip("Reference to the FirstPersonController")]
    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.FirstPersonController m_FirstPersonController;

    private Transform m_CameraTransform;
    private Vector3 m_OriginalPosition;
    private bool m_IsShaking;

    private void Awake()
    {
        if (s_Instance == null)
        {
            s_Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Validate required references
        if (m_Camera == null)
        {
            Debug.LogError("CameraShakeController: Camera reference not set in inspector!");
            enabled = false;
            return;
        }

        m_CameraTransform = m_Camera.transform;
        m_OriginalPosition = m_CameraTransform.localPosition;
    }

    public void ShakeCamera(float _duration, float _magnitude, float _roughness, Vector3 _direction)
    {
        if (m_IsShaking || m_CameraTransform == null)
        {
            return;
        }
        
        StartCoroutine(ShakeCoroutine(_duration, _magnitude, _roughness, _direction));
    }

    private IEnumerator ShakeCoroutine(float _duration, float _magnitude, float _roughness, Vector3 _direction)
    {
        m_IsShaking = true;
        float elapsed = 0f;
        
        while (elapsed < _duration)
        {
            float dampingFactor = 1f - (elapsed / _duration);
            float noiseOffset = Random.Range(0f, 1000f);
            
            Vector3 noise = new Vector3(
                Mathf.PerlinNoise(Time.time * _roughness, noiseOffset) * 2f - 1f,
                Mathf.PerlinNoise(Time.time * _roughness, noiseOffset + 1f) * 2f - 1f,
                Mathf.PerlinNoise(Time.time * _roughness, noiseOffset + 2f) * 2f - 1f
            );

            Vector3 shake = Vector3.Scale(_direction, noise) * _magnitude * dampingFactor;
            m_CameraTransform.localPosition = m_OriginalPosition + shake;

            elapsed += Time.deltaTime;
            yield return null;
        }

        m_CameraTransform.localPosition = m_OriginalPosition;
        m_IsShaking = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (m_Camera == null)
        {
            Debug.LogWarning("CameraShakeController: Please assign the Camera reference in the inspector!");
        }
    }
#endif
} 