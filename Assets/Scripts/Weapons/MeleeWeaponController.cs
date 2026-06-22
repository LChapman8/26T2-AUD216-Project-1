using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * MeleeWeaponController.cs
 * 
 * Purpose: Base class for all melee weapon behavior and management
 * Used by: Axe, sword, and other melee weapon implementations
 * 
 * Key Features:
 * - Melee attack patterns
 * - Swing animations
 * - Hit detection
 * - Damage zones
 * - Visual effects
 * 
 * Combat Mechanics:
 * - Configurable swing arcs
 * - Attack timing
 * - Damage application
 * - Impact effects
 * - Trail rendering
 * 
 * Performance Considerations:
 * - Efficient collision checks
 * - Optimized animation curves
 * - Smart effect management
 * - Coroutine-based timing
 * 
 * Dependencies:
 * - Extends WeaponController
 * - Animation system
 * - Trail renderer
 * - Particle system
 * - Editor tools for setup
 */

public class MeleeWeaponController : WeaponController
{
    [Header("Melee Settings")]
    [SerializeField] protected float swingDuration = 0.5f;
    [SerializeField] protected float swingRadius = 1f;
    [SerializeField] protected LayerMask hitLayers;
    [SerializeField] protected float swingAngle = 90f;
    [SerializeField] protected float swingSpeed = 5f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackArc = 60f;
    [SerializeField] protected AnimationCurve swingCurve;
    [SerializeField] protected Transform weaponModel;
    [SerializeField] protected ParticleSystem swingEffect;
    [SerializeField] protected TrailRenderer swingTrail;
    
    protected bool isSwinging = false;
    protected float swingStartTime;
    protected Vector3 originalModelRotation;

    public void SetAttackRotationFromCurrentTransform()
    {
        if (weaponModel != null)
        {
            originalModelRotation = weaponModel.localEulerAngles;
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }

    protected override void Start()
    {
        base.Start();
        if (weaponModel != null)
            originalModelRotation = weaponModel.localEulerAngles;
    }

    public override void Fire()
    {
        if (!isWeaponRaised) return;

        if (!isSwinging && Time.time >= nextTimeToFire)
        {
            isSwinging = true;
            swingStartTime = Time.time;
            nextTimeToFire = Time.time + fireRate;

            if (weaponAudioManager != null)
                weaponAudioManager.PlayShootSound(weaponName);

            StartCoroutine(PerformSwing());
        }
    }

    protected virtual void PerformMeleeAttack()
    {
        // Perform melee attack logic
        Collider[] hitColliders = Physics.OverlapSphere(firePoint.position, swingRadius, hitLayers);
        foreach (var hitCollider in hitColliders)
        {
            IDamageable target = hitCollider.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
    }

    private IEnumerator PerformSwing()
    {
        if (swingTrail != null)
            swingTrail.enabled = true;
            
        if (swingEffect != null)
            swingEffect.Play();

        PerformMeleeAttack();

        ApplyRecoil();
        yield return new WaitForSeconds(swingDuration);
        
        if (swingTrail != null)
            swingTrail.enabled = false;
            
        isSwinging = false;
    }

    protected override void Update()
    {
        base.Update();
        
        if (isSwinging && weaponModel != null)
        {
            float swingProgress = (Time.time - swingStartTime) / swingDuration;
            float curveValue = swingCurve.Evaluate(swingProgress);
            
            Vector3 newRotation = originalModelRotation;
            newRotation.y += swingAngle * curveValue;
            weaponModel.localEulerAngles = newRotation;
        }
    }
} 