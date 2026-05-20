using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRRaycastGun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource fireAudio;

    [Header("Raycast")]
    [SerializeField] private float range = 50f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Fire Control")]
    [SerializeField] private float fireInterval = 0.2f;
    [SerializeField] private bool holdToFire;

    [Header("Impact Effect")]
    [SerializeField] private float impactEffectLifetime = 2f;

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
        if (!rightHandDevice.isValid && !TryAcquireRightHandDevice())
        {
            return;
        }

        if (!rightHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool isTriggerPressed))
        {
            return;
        }

        if (holdToFire)
        {
            if (isTriggerPressed && Time.time >= nextFireTime)
            {
                Fire();
            }
        }
        else if (isTriggerPressed && !wasTriggerPressed && Time.time >= nextFireTime)
        {
            Fire();
        }

        wasTriggerPressed = isTriggerPressed;
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

        warnedAboutMissingMuzzle = false;
        nextFireTime = Time.time + fireInterval;

        if (drawDebugRay)
        {
            Debug.DrawRay(muzzle.position, muzzle.forward * range, Color.red, 1f);
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (fireAudio != null && fireAudio.clip != null)
        {
            fireAudio.PlayOneShot(fireAudio.clip);
        }

        if (!Physics.Raycast(
                muzzle.position,
                muzzle.forward,
                out RaycastHit hit,
                range,
                hitMask,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        if (impactEffectPrefab == null)
        {
            return;
        }

        Quaternion impactRotation = hit.normal.sqrMagnitude > 0f
            ? Quaternion.LookRotation(hit.normal)
            : Quaternion.LookRotation(-muzzle.forward);

        GameObject spawnedEffect = Instantiate(impactEffectPrefab, hit.point, impactRotation);

        if (impactEffectLifetime > 0f)
        {
            Destroy(spawnedEffect, impactEffectLifetime);
        }
    }
}
