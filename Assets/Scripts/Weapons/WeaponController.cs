using UnityEngine;
using System.Collections;
using TMPro;

#pragma warning disable 0414

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

    private bool showsReticle = false;
    public bool ShowsReticle => weaponName.Contains("SMG") || weaponName.Contains("Revolver");

    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 12;
    [SerializeField] private float reloadTime = 1.5f;
    [SerializeField] private KeyCode reloadKey = KeyCode.R;
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Reload Audio")]
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioSource reloadAudioSource;

    private int currentAmmo;
    private bool isReloading = false;

    [Header("Aim Down Sights / Zoom")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private KeyCode aimKey = KeyCode.Mouse1;
    [SerializeField] private bool holdToAim = true;
    [SerializeField] private bool allowAimWhileReloading = false;
    [SerializeField] private float aimFOV = 20f;
    [SerializeField] private float aimZoomSpeed = 10f;
    [SerializeField] private Vector3 aimPosition = new Vector3(0f, -0.15f, 0.25f);
    [SerializeField] private Vector3 aimRotation = new Vector3(0f, 0f, 0f);
    [SerializeField][Range(0f, 1f)] private float aimBobMultiplier = 0.25f;
    [SerializeField][Range(0f, 1f)] private float aimRecoilMultiplier = 0.65f;

    private float normalFOV;
    private bool isAiming = false;

    [Header("Weapon Bob")]
    [SerializeField] private AnimationCurve bobCurveX;
    [SerializeField] private AnimationCurve bobCurveY;
    [SerializeField] private float bobAmountWalk = 0.05f;
    [SerializeField] private float bobAmountRun = 0.1f;
    [SerializeField] private float bobSpeed = 10f;
    [SerializeField] private AnimationCurve bobCurveRotation;
    [SerializeField] private float bobRotationAmountWalk = 2f;
    [SerializeField] private float bobRotationAmountRun = 4f;

    [Header("Weapon Recoil")]
    [SerializeField] private float kickbackDistance = 0.1f;
    [SerializeField] private float recoilRotation = 10f;
    [SerializeField] protected float returnSpeed = 10f;
    [SerializeField] private float kickbackSpeed = 20f;

    [Header("Weapon Position Toggle")]
    [SerializeField] protected Vector3 raisedPosition = new Vector3(0.2f, -0.2f, 0.4f);
    [SerializeField] protected Vector3 raisedRotation = new Vector3(0f, 0f, 0f);
    [SerializeField] protected Vector3 loweredPositionOffset = new Vector3(0.4f, -0.6f, 0.2f);
    [SerializeField] protected Vector3 loweredRotationOffset = new Vector3(-60f, 0f, 15f);
    [SerializeField] protected float raiseSpeed = 8f;
    [SerializeField] protected float lowerSpeed = 6f;
    [SerializeField] private bool isAutomatic = false;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask shootableLayerMask;

    [Header("Camera Shake")]
    [SerializeField] private float m_ShakeDuration = 0.2f;
    [SerializeField] private float m_ShakeMagnitude = 0.05f;
    [SerializeField] private float m_ShakeRoughness = 10f;
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

        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        HideAmmoUI();

        if (reloadAudioSource == null)
        {
            reloadAudioSource = GetComponent<AudioSource>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera != null)
        {
            normalFOV = playerCamera.fieldOfView;
        }

        if (!isInitialized)
        {
            InitializeWeapon(false);
        }
    }

    protected virtual void Update()
    {
        HandleCameraZoom();

        if (Input.GetKeyDown(reloadKey))
        {
            TryReload();
        }

        if (isInRecoil)
        {
            Vector3 recoveryPosition = GetCurrentBaseWeaponPosition();
            Quaternion recoveryRotation = GetCurrentBaseWeaponRotation();

            transform.localPosition = Vector3.Lerp(transform.localPosition, recoveryPosition, Time.deltaTime * returnSpeed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, recoveryRotation, Time.deltaTime * returnSpeed);

            if (Vector3.Distance(transform.localPosition, recoveryPosition) < 0.001f)
            {
                isInRecoil = false;
                transform.localPosition = recoveryPosition;
                transform.localRotation = recoveryRotation;

                targetPosition = recoveryPosition;
                targetRotation = recoveryRotation;
            }
        }
        else
        {
            float currentToggleSpeed = isWeaponRaised ? raiseSpeed : lowerSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * currentToggleSpeed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * currentToggleSpeed);
        }
    }

    protected void LateUpdate()
    {
        HandleAimInput();
    }

    private void HandleAimInput()
    {
        bool canAim = isWeaponRaised && (!isReloading || allowAimWhileReloading);

        if (!canAim)
        {
            isAiming = false;
            targetPosition = GetCurrentBaseWeaponPosition();
            targetRotation = GetCurrentBaseWeaponRotation();
            return;
        }

        if (holdToAim)
        {
            isAiming = Input.GetKey(aimKey);
        }
        else if (Input.GetKeyDown(aimKey))
        {
            isAiming = !isAiming;
        }

        if (!isWalking && !isRunning && !isInRecoil)
        {
            targetPosition = GetCurrentBaseWeaponPosition();
            targetRotation = GetCurrentBaseWeaponRotation();
        }
    }

    private void HandleCameraZoom()
    {
        if (playerCamera == null) return;

        float targetFOV = isAiming ? aimFOV : normalFOV;

    }

    private Vector3 GetCurrentBaseWeaponPosition()
    {
        if (!isWeaponRaised)
            return raisedPosition + loweredPositionOffset;

        return isAiming ? aimPosition : raisedPosition;
    }

    private Quaternion GetCurrentBaseWeaponRotation()
    {
        if (!isWeaponRaised)
            return Quaternion.Euler(raisedRotation + loweredRotationOffset);

        return Quaternion.Euler(isAiming ? aimRotation : raisedRotation);
    }

    public virtual void Fire()
    {
        if (!isWeaponRaised) return;
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            TryReload();
            return;
        }

        if (Time.time >= nextTimeToFire)
        {
            if (!isAutomatic && !Input.GetMouseButtonDown(0)) return;

            currentAmmo--;
            UpdateAmmoUI();

            nextTimeToFire = Time.time + fireRate;

            ApplyRecoil();

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

    private void TryReload()
    {
        if (isReloading) return;
        if (!isWeaponRaised) return;
        if (currentAmmo >= maxAmmo) return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        if (!allowAimWhileReloading)
        {
            isAiming = false;
        }

        if (reloadAudioSource != null && reloadSound != null)
        {
            reloadAudioSource.PlayOneShot(reloadSound);
        }

        UpdateAmmoUI(true);

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;

        UpdateAmmoUI();
    }

    private void UpdateAmmoUI(bool reloading = false)
    {
        if (ammoText == null) return;

        if (!isWeaponRaised)
        {
            ammoText.gameObject.SetActive(false);
            return;
        }

        ammoText.gameObject.SetActive(true);

        if (reloading)
        {
            ammoText.text = "Reloading...";
        }
        else
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
        }
    }

    public void RefreshAmmoUI()
    {
        UpdateAmmoUI();
    }

    public void ShowAmmoUI()
    {
        UpdateAmmoUI();
    }

    public void HideAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(false);
        }
    }

    protected virtual void ApplyRecoil()
    {
        isInRecoil = true;

        float recoilScale = isAiming ? aimRecoilMultiplier : 1f;
        Vector3 basePosition = GetCurrentBaseWeaponPosition();
        Quaternion baseRotation = GetCurrentBaseWeaponRotation();

        targetKickbackPosition = basePosition - new Vector3(0, 0, kickbackDistance * recoilScale);
        targetKickbackRotation = baseRotation * Quaternion.Euler(new Vector3(-recoilRotation * recoilScale, 0, 0));

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetKickbackPosition, Time.deltaTime * kickbackSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetKickbackRotation, Time.deltaTime * kickbackSpeed);

        if (CameraShakeController.Instance != null)
        {
            float shakeMagnitude = m_ShakeMagnitude;
            float shakeDuration = m_ShakeDuration;

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

            if (isAiming)
            {
                shakeMagnitude *= aimRecoilMultiplier;
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

        if (!isWeaponRaised)
        {
            isAiming = false;
            HideAmmoUI();
        }

        if (ShowsReticle)
        {
            ReticleController.Show(isWeaponRaised);
        }

        if (isWeaponRaised)
        {
            targetPosition = GetCurrentBaseWeaponPosition();
            targetRotation = GetCurrentBaseWeaponRotation();
            ShowAmmoUI();

            if (weaponAudioManager != null)
                weaponAudioManager.PlayWeaponRaiseSound(weaponName);
        }
        else
        {
            targetPosition = raisedPosition + loweredPositionOffset;
            targetRotation = Quaternion.Euler(raisedRotation + loweredRotationOffset);
            HideAmmoUI();

            if (weaponAudioManager != null)
                weaponAudioManager.PlayWeaponLowerSound(weaponName);
        }
    }

    public void InitializeWeapon(bool startRaised)
    {
        if (!isInitialized)
        {
            isInitialized = true;
            isWeaponRaised = false;
            isAiming = false;

            transform.localPosition = raisedPosition + loweredPositionOffset;
            transform.localRotation = Quaternion.Euler(raisedRotation + loweredRotationOffset);
            targetPosition = transform.localPosition;
            targetRotation = transform.localRotation;
            HideAmmoUI();
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
            bobTimer += Time.deltaTime * bobSpeed * speed;

            float bobScale = isAiming ? aimBobMultiplier : 1f;
            float currentBobAmount = (isRunning ? bobAmountRun : bobAmountWalk) * bobScale;
            float currentRotationAmount = (isRunning ? bobRotationAmountRun : bobRotationAmountWalk) * bobScale;

            float horizontalBob = bobCurveX.Evaluate(bobTimer) * currentBobAmount;
            float verticalBob = bobCurveY.Evaluate(bobTimer) * currentBobAmount;
            Vector3 bobOffset = new Vector3(horizontalBob, verticalBob, 0f);

            float rotationBob = bobCurveRotation.Evaluate(bobTimer) * currentRotationAmount;
            Quaternion bobRotation = Quaternion.Euler(0f, 0f, rotationBob);

            if (!isInRecoil)
            {
                targetPosition = GetCurrentBaseWeaponPosition() + bobOffset;
                targetRotation = GetCurrentBaseWeaponRotation() * bobRotation;
            }

            if (bobTimer > 1000f)
            {
                bobTimer = 0f;
            }
        }
        else
        {
            bobTimer = 0f;

            if (!isInRecoil)
            {
                targetPosition = GetCurrentBaseWeaponPosition();
                targetRotation = GetCurrentBaseWeaponRotation();
            }
        }
    }
}