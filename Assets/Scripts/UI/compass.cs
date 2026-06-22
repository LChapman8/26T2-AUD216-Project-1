using UnityEngine;

/*
 * compass.cs
 * 
 * Purpose: Controls compass UI element rotation based on player view
 * Used by: Navigation system, UI
 * 
 * Key Features:
 * - View direction tracking
 * - Smooth compass rotation
 * - UI element positioning
 * - Ground plane projection
 * 
 * Performance Considerations:
 * - Efficient angle calculations
 * - Late update timing
 * - Minimal vector operations
 * 
 * Dependencies:
 * - Requires view direction transform
 * - UI RectTransform setup
 * - Compass size configuration
 */

public class compass : MonoBehaviour
{

    public Transform viewDirection;
    public RectTransform compassElement;
    public float compassSize;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void LateUpdate()
    {
        Vector3 forwardVector = Vector3.ProjectOnPlane(viewDirection.forward, Vector3.up).normalized;
        float forwardSignedAngle = Vector3.SignedAngle(forwardVector, Vector3.forward, Vector3.up);
        float compassOffset = (forwardSignedAngle / 180f) * compassSize;
        compassElement.anchoredPosition = new Vector3(compassOffset, 0);
    }
}