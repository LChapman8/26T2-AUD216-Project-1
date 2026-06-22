using UnityEngine;

/*
 * AxeController.cs
 * 
 * Purpose: Controls melee axe weapon behavior and animations
 * Used by: Player weapon system, melee combat
 * 
 * Key Features:
 * - Melee attack animations
 * - Position transitions
 * - Attack collision detection
 * - Damage application
 * - Smooth state transitions
 * 
 * Combat Mechanics:
 * - Attack swing system
 * - Hit detection
 * - Damage zones
 * - Animation control
 * 
 * Performance Considerations:
 * - Optimized collision checks
 * - Efficient state management
 * - Smart animation transitions
 * - Position interpolation
 * 
 * Dependencies:
 * - Extends MeleeWeaponController
 * - Animation system
 * - Physics collision system
 * - Impact effect manager
 */

public class AxeController : MeleeWeaponController
{
    [Header("Attack Swing Settings")]
    [SerializeField] private Vector3 attackSwingPosition;
    [SerializeField] private Vector3 attackSwingRotation;
    [SerializeField] private float attackSwingSpeed = 15f;
    [SerializeField] private float attackReturnSpeed = 10f;
    
    private bool isPositionTransitioning;
    private bool isAttackingForward;
    private Vector3 currentTargetPosition;
    private Vector3 currentTargetRotation;
    private float currentTransitionTime;
    private const float POSITION_THRESHOLD = 0.01f;
    private const float ROTATION_THRESHOLD = 0.1f;

    protected override void Start()
    {
        base.Start();
        swingDuration = 0.5f;
        swingAngle = 80f;
        swingSpeed = 15f;
        MoveToLoweredPosition();
    }

    protected override void PerformMeleeAttack()
    {
        // Only allow attacks when weapon is completely still in raised position
        // and not in ANY phase of the attack animation
        if (!isWeaponRaised || isPositionTransitioning) return;

        isPositionTransitioning = true;
        isAttackingForward = true;
        currentTransitionTime = 0f;
        currentTargetPosition = attackSwingPosition;
        currentTargetRotation = attackSwingRotation;

        Collider[] hitColliders = Physics.OverlapSphere(
            firePoint.position, 
            attackRange, 
            hitLayers
        );

        foreach (var hitCollider in hitColliders)
        {
            // Instead of using ClosestPoint, do a direct raycast to the collider's center
            Vector3 directionToTarget = (hitCollider.bounds.center - firePoint.position).normalized;

            RaycastHit hit;
            if (Physics.Raycast(firePoint.position, directionToTarget, out hit, attackRange, hitLayers))
            {
                // Apply damage if it's damageable
                IDamageable target = hit.collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                }

                // Play impact effect at the actual point of impact
                if (BulletImpactManager.Instance != null)
                {
                    BulletImpactManager.Instance.PlayImpactEffect(hit);
                }
            }
        }
    }

    protected override void Update()
    {
        // Only handle attack animations when weapon is raised
        if (!isWeaponRaised || !isPositionTransitioning) 
        {
            base.Update();
            return;
        }

        currentTransitionTime += Time.deltaTime;
        float speed = isAttackingForward ? attackSwingSpeed : attackReturnSpeed;
        
        // Use SmoothDamp for more controlled motion
        transform.localPosition = Vector3.Lerp(
            transform.localPosition, 
            currentTargetPosition, 
            currentTransitionTime * speed
        );
        
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation, 
            Quaternion.Euler(currentTargetRotation), 
            currentTransitionTime * speed
        );

        float positionDistance = Vector3.Distance(transform.localPosition, currentTargetPosition);
        float rotationDistance = Quaternion.Angle(transform.localRotation, Quaternion.Euler(currentTargetRotation));
        
        // Check if we've reached our target
        if (positionDistance < POSITION_THRESHOLD && rotationDistance < ROTATION_THRESHOLD)
        {
            if (isAttackingForward)
            {
                // Start return to raised position
                isAttackingForward = false;
                currentTransitionTime = 0f;
                currentTargetPosition = raisedPosition;
                currentTargetRotation = raisedRotation;
            }
            else
            {
                // Attack complete, ensure we're exactly at raised position
                isPositionTransitioning = false;
                transform.localPosition = raisedPosition;
                transform.localRotation = Quaternion.Euler(raisedRotation);
            }
        }
    }

    public void MoveToRaisedPosition()
    {
        transform.localPosition = raisedPosition;
        transform.localRotation = Quaternion.Euler(raisedRotation);
        currentTargetPosition = raisedPosition;
        currentTargetRotation = raisedRotation;
    }

    public void MoveToLoweredPosition()
    {
        transform.localPosition = raisedPosition + loweredPositionOffset;
        transform.localRotation = Quaternion.Euler(raisedRotation + loweredRotationOffset);
        currentTargetPosition = raisedPosition + loweredPositionOffset;
        currentTargetRotation = raisedRotation + loweredRotationOffset;
    }

    private void OnEnable()
    {
        // Just set target to raised position
        targetPosition = raisedPosition;
        targetRotation = Quaternion.Euler(raisedRotation);
        isWeaponRaised = true;
    }

    private void OnDisable()
    {
        // Just set target to lowered position
        targetPosition = raisedPosition + loweredPositionOffset;
        targetRotation = Quaternion.Euler(raisedRotation + loweredRotationOffset);
        isWeaponRaised = false;
    }
} 