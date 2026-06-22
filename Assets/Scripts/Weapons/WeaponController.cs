using UnityEngine;
using System.Collections;

#pragma warning disable 0414 // waveCompletedDisplayTime is for future use

/*
 * WeaponController.cs
 * 
 * Purpose: Base class for all weapon behavior and management
 * Used by: Player weapon system, specific weapon implementations
 * 
 * Key Features:
 * - Weapon positioning and movement
 * - Recoil system
 * - Weapon bob animation
 * - Fire rate control
 * - Damage system
 * - Camera shake integration
 * 
 * Movement Features:
 * - Smooth weapon transitions
 * - Position/rotation interpolation
 * - Configurable bob patterns
 * - Walking/running states
 * 
 * Performance Considerations:
 * - Efficient state management
 * - Optimized transform updates
 * - Smart component caching
 * - Minimal physics checks
 * 
 * Dependencies:
 * - ReticleController for crosshair
 * - BulletImpactManager for hit effects
 * - WeaponAudioManager for sounds
 * - CameraShakeController for recoil
 */

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Properties")]
    [SerializeField] protected float fireRate = 0.5f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float range = 100f;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected ParticleSystem muzzleFlash;
    [SerializeField] protected WeaponAudioManager weaponAudioManager;
    [SerializeField] protected string weaponName;
    
    // Make this private and provide a property that checks weapon type
    private bool showsReticle = false;
    public bool ShowsReticle => weaponName.Contains("SMG") || weaponName.Contains("Revolver");

    [Header("Weapon Bob")]
    [Tooltip("Horizontal movement curve for weapon bobbing")]
    [SerializeField] private AnimationCurve bobCurveX;
    
    [Tooltip("Vertical movement curve for weapon bobbing")]
    [SerializeField] private AnimationCurve bobCurveY;
    
    [Tooltip("Amount of weapon bob while walking")]
    [SerializeField] private float bobAmountWalk = 0.05f;
    
    [Tooltip("Amount of weapon bob while running")]
    [SerializeField] private float bobAmountRun = 0.1f;
    
    [Tooltip("Speed of the weapon bob motion")]
    [SerializeField] private float bobSpeed = 10f;

    [Tooltip("Tilt rotation curve for weapon bobbing")]
    [SerializeField] private AnimationCurve bobCurveRotation;

    [Tooltip("Maximum rotation angle while walking")]
    [SerializeField] private float bobRotationAmountWalk = 2f;

    [Tooltip("Maximum rotation angle while running")]
    [SerializeField] private float bobRotationAmountRun = 4f;

    [Header("Weapon Recoil")]
    [Tooltip("How far back the weapon moves when fired")]
    [SerializeField] private float kickbackDistance = 0.1f;
    
    [Tooltip("How far up the weapon rotates when fired")]
    [SerializeField] private float recoilRotation = 10f;
    
    [Tooltip("How quickly the weapon returns to its original position")]
    [SerializeField] protected float returnSpeed = 10f;
    
    [Tooltip("How quickly the recoil motion is applied")]
    [SerializeField] private float kickbackSpeed = 20f;

    [Header("Weapon Position Toggle")]
    [Tooltip("Position when weapon is raised")]
    [SerializeField] protected Vector3 raisedPosition = new Vector3(0.2f, -0.2f, 0.4f);

    [Tooltip("Rotation when weapon is raised (in degrees)")]
    [SerializeField] protected Vector3 raisedRotation = new Vector3(0f, 0f, 0f);

    [Tooltip("Position offset when weapon is lowered (relative to raised position)")]
    [SerializeField] protected Vector3 loweredPositionOffset = new Vector3(0.4f, -0.6f, 0.2f);

    [Tooltip("Rotation offset when weapon is lowered (in degrees)")]
    [SerializeField] protected Vector3 loweredRotationOffset = new Vector3(-60f, 0f, 15f);

    [Tooltip("How fast the weapon moves when raising")]
    [SerializeField] protected float raiseSpeed = 8f;

    [Tooltip("How fast the weapon moves when lowering")]
    [SerializeField] protected float lowerSpeed = 6f;

    [Tooltip("If true, weapon will fire continuously while mouse button is held")]
    [SerializeField] private bool isAutomatic = false;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask shootableLayerMask;

    [Header("Camera Shake")]
    [Tooltip("Duration of camera shake when firing")]
    [SerializeField] private float m_ShakeDuration = 0.2f;

    [Tooltip("Intensity of camera shake")]
    [SerializeField] private float m_ShakeMagnitude = 0.05f;

    [Tooltip("How rough/noisy the camera shake is")]
    [SerializeField] private float m_ShakeRoughness = 10f;

    [Tooltip("Direction bias of the camera shake")]
    [SerializeField] private Vector3 m_ShakeDirection = new Vector3(0.1f, 0.3f, 0.1f);

    protected Vector3 weaponOriginalPosition;
    protected Quaternion weaponOriginalRotation;
    protected float bobTimer;
    protected float nextTimeToFire;
    protected bool isWalking;
    protected bool isRunning;
    protected float currentBobAmount;
    protected Vector3 targetKickbackPosition;
    protected Quaternion targetKickbackRotation;
    protected bool isInRecoil;
    protected bool isWeaponRaised = false;
    protected Vector3 targetPosition;
    protected Quaternion targetRotation;
    protected bool isInitialized = false;

    protected virtual void Start()
    {
        weaponOriginalPosition = transform.localPosition;
        weaponOriginalRotation = transform.localRotation;
        targetPosition = transform.localPosition;
        targetRotation = transform.localRotation;
        targetKickbackPosition = weaponOriginalPosition;
        targetKickbackRotation = weaponOriginalRotation;

        // Initialize as lowered if not already initialized
        if (!isInitialized)
        {
            InitializeWeapon(false);
        }
    }

    protected virtual void Update()
    {
        // Handle weapon position toggling and recoil recovery
        if (isInRecoil)
        {
            // Handle recoil recovery
            Vector3 recoveryPosition = isWeaponRaised ? 
                raisedPosition : // Return to raised position when raised
                raisedPosition + loweredPositionOffset; // Return to lowered position when lowered
            
            Quaternion recoveryRotation = isWeaponRaised ?
                Quaternion.Euler(raisedRotation) : // Return to raised rotation when raised
                Quaternion.Euler(raisedRotation + loweredRotationOffset); // Return to lowered rotation when lowered

            transform.localPosition = Vector3.Lerp(transform.localPosition, recoveryPosition, Time.deltaTime * returnSpeed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, recoveryRotation, Time.deltaTime * returnSpeed);

            if (Vector3.Distance(transform.localPosition, recoveryPosition) < 0.001f)
            {
                isInRecoil = false;
                transform.localPosition = recoveryPosition;
                transform.localRotation = recoveryRotation;
                
                // Reset targets after recoil
                targetPosition = recoveryPosition;
                targetRotation = recoveryRotation;
            }
        }
        else
        {
            // Handle weapon position toggling when not in recoil
            float currentToggleSpeed = isWeaponRaised ? raiseSpeed : lowerSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * currentToggleSpeed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * currentToggleSpeed);
        }
    }

    public virtual void Fire()
    {
        if (!isWeaponRaised) return;
        
        if (Time.time >= nextTimeToFire)
        {
            if (!isAutomatic && !Input.GetMouseButtonDown(0)) return;

            nextTimeToFire = Time.time + fireRate;
            
            ApplyRecoil();

            // Add reticle feedback
            if (ShowsReticle)
            {
                ReticleController.OnWeaponFired();
            }

            if (muzzleFlash != null)
                muzzleFlash.Play();
            
            if (weaponAudioManager != null)
                weaponAudioManager.PlayShootSound(weaponName);

            RaycastHit hit;
            if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range, shootableLayerMask))
            {
                if (BulletImpactManager.Instance != null)
                {
                    BulletImpactManager.Instance.PlayImpactEffect(hit);
                }

                IDamageable target = hit.transform.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                }
            }
        }
    }

    protected virtual void ApplyRecoil()
    {
        isInRecoil = true;

        targetKickbackPosition = raisedPosition - new Vector3(0, 0, kickbackDistance);
        targetKickbackRotation = Quaternion.Euler(raisedRotation + new Vector3(-recoilRotation, 0, 0));

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetKickbackPosition, Time.deltaTime * kickbackSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetKickbackRotation, Time.deltaTime * kickbackSpeed);

        // Apply camera shake
        if (CameraShakeController.Instance != null)
        {
            // Modify shake parameters based on weapon type
            float shakeMagnitude = m_ShakeMagnitude;
            float shakeDuration = m_ShakeDuration;
            
            // Increase shake for more powerful weapons
            if (weaponName.Contains("Revolver"))
            {
                shakeMagnitude *= 1.5f;
                shakeDuration *= 1.2f;
            }
            else if (weaponName.Contains("SMG"))
            {
                shakeMagnitude *= 0.7f;
                shakeDuration *= 0.8f;
            }
            
            CameraShakeController.Instance.ShakeCamera(
                shakeDuration,
                shakeMagnitude,
                m_ShakeRoughness,
                m_ShakeDirection
            );
        }
    }

    public virtual void ToggleWeaponPosition()
    {
        isWeaponRaised = !isWeaponRaised;
        
        // Restore reticle visibility control
        if (ShowsReticle)
        {
            ReticleController.Show(isWeaponRaised);
        }
        
        if (isWeaponRaised)
        {
            targetPosition = raisedPosition;
            targetRotation = Quaternion.Euler(raisedRotation);
            if (weaponAudioManager != null)
                weaponAudioManager.PlayWeaponRaiseSound(weaponName);
        }
        else
        {
            targetPosition = raisedPosition + loweredPositionOffset;
            targetRotation = Quaternion.Euler(raisedRotation + loweredRotationOffset);
            if (weaponAudioManager != null)
                weaponAudioManager.PlayWeaponLowerSound(weaponName);
        }
    }

    public void InitializeWeapon(bool startRaised)
    {
        if (!isInitialized)
        {
            isInitialized = true;
            isWeaponRaised = false;  // Always start lowered
            
            // Set initial position and rotation to lowered state
            transform.localPosition = raisedPosition + loweredPositionOffset;
            transform.localRotation = Quaternion.Euler(raisedRotation + loweredRotationOffset);
            targetPosition = transform.localPosition;
            targetRotation = transform.localRotation;
        }
    }

    public bool IsWeaponRaised()
    {
        return isWeaponRaised;
    }

    public virtual void UpdateWeaponBob(bool walking, bool running, float speed)
    {
        if (!isWeaponRaised || isInRecoil) return;

        isWalking = walking;
        isRunning = running;

        if (isWalking || isRunning)
        {
            // Update bob timer
            bobTimer += Time.deltaTime * bobSpeed * speed;
            
            // Calculate bob amounts based on movement state
            float currentBobAmount = isRunning ? bobAmountRun : bobAmountWalk;
            float currentRotationAmount = isRunning ? bobRotationAmountRun : bobRotationAmountWalk;

            // Calculate position offset
            float horizontalBob = bobCurveX.Evaluate(bobTimer) * currentBobAmount;
            float verticalBob = bobCurveY.Evaluate(bobTimer) * currentBobAmount;
            Vector3 bobOffset = new Vector3(horizontalBob, verticalBob, 0f);

            // Calculate rotation offset
            float rotationBob = bobCurveRotation.Evaluate(bobTimer) * currentRotationAmount;
            Quaternion bobRotation = Quaternion.Euler(0f, 0f, rotationBob);

            // Apply bob to target position and rotation
            if (!isInRecoil)
            {
                Vector3 targetPos = isWeaponRaised ? raisedPosition : (raisedPosition + loweredPositionOffset);
                targetPosition = targetPos + bobOffset;
                
                Quaternion baseRotation = isWeaponRaised ? 
                    Quaternion.Euler(raisedRotation) : 
                    Quaternion.Euler(raisedRotation + loweredRotationOffset);
                targetRotation = baseRotation * bobRotation;
            }

            // Reset bob timer to prevent floating point errors
            if (bobTimer > 1000f)
            {
                bobTimer = 0f;
            }
        }
        else
        {
            // Return to normal position when not moving
            bobTimer = 0f;
            if (!isInRecoil)
            {
                targetPosition = isWeaponRaised ? raisedPosition : (raisedPosition + loweredPositionOffset);
                targetRotation = isWeaponRaised ? 
                    Quaternion.Euler(raisedRotation) : 
                    Quaternion.Euler(raisedRotation + loweredRotationOffset);
            }
        }
    }
} 