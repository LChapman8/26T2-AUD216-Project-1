using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class MushroomTripTrigger : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Audio Snapshots")]
    [SerializeField] private AudioMixerSnapshot normalSnapshot;
    [SerializeField] private AudioMixerSnapshot mushroomSnapshot;
    [SerializeField] private float snapshotTransitionTime = 2f;

    [Header("Screen Overlay")]
    [SerializeField] private Transform overlayTransform;
    [SerializeField] private float normalOverlayZ = 1f;
    [SerializeField] private float tripOverlayZ = 1.98f;

    [Header("Trip Settings")]
    [SerializeField] private float tripDuration = 20f;
    [SerializeField] private bool destroyMushroomAfterUse = true;

    [Header("Camera Effects")]
    [SerializeField] private Camera playerCamera;

    [SerializeField] private float swayAmount = 0.03f;
    [SerializeField] private float swaySpeed = 0.4f;

    [SerializeField] private float rollAmount = 1.5f;
    [SerializeField] private float rollSpeed = 0.2f;

    [SerializeField] private float fovAmplitude = 2f;
    [SerializeField] private float fovSpeed = 0.3f;

    private Vector3 cameraStartPos;
    private Quaternion cameraStartRot;
    private float cameraStartFOV;

    private bool playerInRange;
    private bool isTripping;

    private void Start()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        SetOverlayZ(normalOverlayZ);

        if (playerCamera != null)
        {
            cameraStartPos = playerCamera.transform.localPosition;
            cameraStartRot = playerCamera.transform.localRotation;
            cameraStartFOV = playerCamera.fieldOfView;
        }
    }

    private void Update()
    {
        if (playerInRange && !isTripping && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(TripRoutine());
        }

        if (isTripping && playerCamera != null)
        {
            float swayX =
                Mathf.Sin(Time.time * swaySpeed) * swayAmount;

            float swayY =
                Mathf.Cos(Time.time * swaySpeed * 0.8f) * swayAmount;

            playerCamera.transform.localPosition =
                cameraStartPos + new Vector3(swayX, swayY, 0);

            float roll =
                Mathf.Sin(Time.time * rollSpeed) * rollAmount;

            playerCamera.transform.localRotation =
                cameraStartRot * Quaternion.Euler(0, 0, roll);

            playerCamera.fieldOfView =
                cameraStartFOV +
                Mathf.Sin(Time.time * fovSpeed) * fovAmplitude;
        }
        else if (playerCamera != null)
        {
            playerCamera.transform.localPosition = cameraStartPos;
            playerCamera.transform.localRotation = cameraStartRot;
            playerCamera.fieldOfView = cameraStartFOV;
        }
    }

    private IEnumerator TripRoutine()
    {
        isTripping = true;

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        mushroomSnapshot.TransitionTo(snapshotTransitionTime);

        SetOverlayZ(tripOverlayZ);

        yield return new WaitForSeconds(tripDuration);

        normalSnapshot.TransitionTo(snapshotTransitionTime);

        SetOverlayZ(normalOverlayZ);

        isTripping = false;

        if (destroyMushroomAfterUse)
            Destroy(gameObject);
    }

    private void SetOverlayZ(float zValue)
    {
        if (overlayTransform == null) return;

        Vector3 pos = overlayTransform.localPosition;
        pos.z = zValue;
        overlayTransform.localPosition = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (promptText != null && !isTripping)
        {
            promptText.text = "Press E to Consume";
            promptText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }
}