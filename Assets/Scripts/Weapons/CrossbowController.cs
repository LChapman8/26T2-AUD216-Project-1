using UnityEngine;

/*
 * CrossbowController.cs
 * 
 * Purpose: Handles crossbow weapon mechanics and arrow firing.
 * Used by: PlayerWeaponSystem, Arrow
 * 
 * Controls crossbow firing mechanics, including drawing, aiming, and releasing arrows.
 * Manages weapon states and animations, integrating with the arrow pooling system
 * for efficient projectile management.
 * 
 * Performance Considerations:
 * - Uses pooled arrows instead of instantiation
 * - Optimizes physics calculations for arrows
 * - Manages weapon state transitions
 * 
 * Dependencies:
 * - ArrowPool for projectile management
 * - WeaponController base class
 * - WeaponAudioManager for sound effects
 */

public class CrossbowController : WeaponController
{
    [Header("Crossbow Settings")]
    [SerializeField] private float arrowForce = 20f;
    [SerializeField] private float drawTime = 1f;
    private bool isDrawing = false;
    private float drawStartTime;

    public override void Fire()
    {
        if (!isWeaponRaised) return;

        if (!isDrawing && Time.time >= nextTimeToFire)
        {
            isDrawing = true;
            drawStartTime = Time.time;
            
            if (weaponAudioManager != null)
                weaponAudioManager.PlayReloadStartSound(weaponName);
        }
        else if (isDrawing && Time.time >= drawStartTime + drawTime)
        {
            nextTimeToFire = Time.time + fireRate;
            isDrawing = false;

            if (muzzleFlash != null)
                muzzleFlash.Play();

            if (weaponAudioManager != null)
                weaponAudioManager.PlayShootSound(weaponName);

            Arrow arrow = ArrowPool.Instance.GetArrow();
            arrow.transform.position = firePoint.position;
            arrow.transform.rotation = firePoint.rotation;
            
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(firePoint.forward * arrowForce, ForceMode.Impulse);
            }

            ApplyRecoil();
        }
    }
} 