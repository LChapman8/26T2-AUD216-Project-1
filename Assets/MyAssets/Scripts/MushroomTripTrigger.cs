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
    [SerializeField] private Transform tripCameraPivot;
    [SerializeField] private float swayAmount = 0.03f;
    [SerializeField] private float swaySpeed = 0.4f;
    [SerializeField] private float rollAmount = 1.5f;
    [SerializeField] private float rollSpeed = 0.2f;

    private Vector3 pivotStartPos;
    private Quaternion pivotStartRot;

    private bool playerInRange;
    private bool isTripping;

    private void Start()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        SetOverlayZ(normalOverlayZ);

        if (tripCameraPivot != null)
        {
            pivotStartPos = tripCameraPivot.localPosition;
            pivotStartRot = tripCameraPivot.localRotation;
        }
    }

    private void Update()
    {
        if (playerInRange && !isTripping && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(TripRoutine());
        }
    }

    private void LateUpdate()
    {
        // IMPORTANT:
        // Only the mushroom currently being consumed controls the camera.
        if (isTripping)
        {
            ApplyTripCameraEffects();
        }
    }

    private void ApplyTripCameraEffects()
    {
        if (tripCameraPivot == null) return;

        float swayX = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        float swayY = Mathf.Cos(Time.time * swaySpeed * 0.8f) * swayAmount;

        tripCameraPivot.localPosition =
            pivotStartPos + new Vector3(swayX, swayY, 0f);

        float roll = Mathf.Sin(Time.time * rollSpeed) * rollAmount;

        tripCameraPivot.localRotation =
            pivotStartRot * Quaternion.Euler(0f, 0f, roll);
    }

    private void ResetTripCameraEffects()
    {
        if (tripCameraPivot == null) return;

        tripCameraPivot.localPosition = pivotStartPos;
        tripCameraPivot.localRotation = pivotStartRot;
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

        // Reset ONCE when the trip ends.
        ResetTripCameraEffects();

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