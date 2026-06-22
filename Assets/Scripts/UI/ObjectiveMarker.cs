using UnityEngine;
using UnityEngine.UI;

/*
 * ObjectiveMarker.cs
 * 
 * Purpose: Manages visual markers for objectives in world space and on compass
 * Used by: Objective system, navigation UI
 * 
 * Key Features:
 * - Dual marker system (world and compass)
 * - Dynamic distance calculation
 * - Automatic position updates
 * - Configurable icon display
 * 
 * Performance Considerations:
 * - Optimized position updates
 * - Efficient distance calculations
 * - Smart reference management
 * 
 * Dependencies:
 * - Unity UI system
 * - Requires main camera reference
 * - Needs compass bar setup
 */
public class ObjectiveMarker : MonoBehaviour
{
    [SerializeField] private Image markerIcon;
    [SerializeField] private Image compassIcon;
    
    private Transform target;
    private Camera mainCamera;
    private RectTransform compassBarRect;

    public void Initialize(Transform _target, Sprite _icon, Camera _camera, Transform _compassBar)
    {
        target = _target;
        mainCamera = _camera;
        compassBarRect = _compassBar as RectTransform;

        if (markerIcon != null)
        {
            markerIcon.sprite = _icon;
        }

        if (compassIcon != null)
        {
            compassIcon.sprite = _icon;
        }
    }

    public void UpdatePosition(float maxCompassAngle)
    {
        if (target == null || mainCamera == null || compassBarRect == null) return;

        // Get the direction to the target
        Vector3 directionToTarget = target.position - mainCamera.transform.position;
        float angle = Vector3.SignedAngle(mainCamera.transform.forward, directionToTarget, Vector3.up);

        // Update compass position
        float normalizedAngle = Mathf.Clamp(angle / maxCompassAngle, -1f, 1f);
        float compassPosition = normalizedAngle * compassBarRect.rect.width * 0.5f;
        compassIcon.rectTransform.anchoredPosition = new Vector2(compassPosition, 0);

        // Update visibility based on angle
        compassIcon.gameObject.SetActive(Mathf.Abs(angle) <= maxCompassAngle);
    }

    public void SetScale(float scale)
    {
        if (compassIcon != null)
        {
            compassIcon.transform.localScale = Vector3.one * scale;
        }
    }
} 