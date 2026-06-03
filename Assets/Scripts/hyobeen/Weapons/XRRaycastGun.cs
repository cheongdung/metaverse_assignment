using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRRaycastGun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource fireAudio;
    [SerializeField] private Camera aimCamera;

    [Header("Bullet")]
    [SerializeField] private string bulletTag = "Bullet";
    [SerializeField] private float spawnForwardOffset = 0.1f;
    [SerializeField] private float debugLineLength = 3f;
    [SerializeField] private float aimDistance = 100f;

    [Header("Fire Control")]
    [SerializeField] private float fireInterval = 0.2f;
    [SerializeField] private bool holdToFire;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;

    private readonly List<InputDevice> rightHandDevices = new List<InputDevice>();

    private InputDevice rightHandDevice;
    private float nextFireTime;
    private bool wasTriggerPressed;
    private bool warnedAboutMissingMuzzle;

    private void OnEnable()
    {
        TryAcquireRightHandDevice();
    }

    private void OnDisable()
    {
        wasTriggerPressed = false;
    }

    private void Update()
    {
        bool isTriggerPressed = false;
        bool hasXrInput = false;

        if (rightHandDevice.isValid || TryAcquireRightHandDevice())
        {
            hasXrInput = rightHandDevice.TryGetFeatureValue(
                CommonUsages.triggerButton,
                out isTriggerPressed);
        }

        bool isMousePressed = Input.GetMouseButton(0);
        bool isFirePressed = isMousePressed || (hasXrInput && isTriggerPressed);

        if (holdToFire)
        {
            if (isFirePressed && Time.time >= nextFireTime)
            {
                Fire();
            }
        }
        else if (isFirePressed && !wasTriggerPressed && Time.time >= nextFireTime)
        {
            Fire();
        }

        wasTriggerPressed = isFirePressed;
    }

    private bool TryAcquireRightHandDevice()
    {
        rightHandDevices.Clear();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller,
            rightHandDevices);

        if (rightHandDevices.Count == 0)
        {
            rightHandDevice = default;
            return false;
        }

        rightHandDevice = rightHandDevices[0];
        return rightHandDevice.isValid;
    }

    private void Fire()
    {
        if (muzzle == null)
        {
            if (!warnedAboutMissingMuzzle)
            {
                Debug.LogWarning("XRRaycastGun requires a muzzle Transform reference.", this);
                warnedAboutMissingMuzzle = true;
            }

            return;
        }

        if (bulletPrefab == null)
        {
            Debug.LogWarning("XRRaycastGun requires a bullet prefab reference.", this);
            return;
        }

        warnedAboutMissingMuzzle = false;
        nextFireTime = Time.time + fireInterval;

        Vector3 shootDirection = GetShootDirection();

        if (drawDebugRay)
        {
            Debug.DrawRay(muzzle.position, shootDirection * debugLineLength, Color.red, 1f);
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (fireAudio != null && fireAudio.clip != null)
        {
            fireAudio.PlayOneShot(fireAudio.clip);
        }

        Vector3 spawnPosition = muzzle.position + (shootDirection * spawnForwardOffset);
        GameObject spawnedBullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.LookRotation(shootDirection));

        if (!string.IsNullOrEmpty(bulletTag))
        {
            spawnedBullet.tag = bulletTag;
        }

        BulletController_cr bulletController = spawnedBullet.GetComponent<BulletController_cr>();
        if (bulletController == null)
        {
            Debug.LogWarning("XRRaycastGun bullet prefab needs a BulletController_cr component.", spawnedBullet);
            Destroy(spawnedBullet);
            return;
        }

        bulletController.Shoot(shootDirection);
    }

    private Vector3 GetShootDirection()
    {
        Camera activeCamera = aimCamera != null ? aimCamera : Camera.main;
        if (activeCamera == null)
        {
            return muzzle.forward;
        }

        Ray centerRay = activeCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = centerRay.origin + (centerRay.direction * aimDistance);

        if (Physics.Raycast(centerRay, out RaycastHit hit, aimDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }

        return (targetPoint - muzzle.position).normalized;
    }
}
