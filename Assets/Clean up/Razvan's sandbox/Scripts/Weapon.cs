using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class Weapon : MonoBehaviour
{
    public UnityEvent vibrate;
    public GameObject xrcomp;
    [Header("Rotation axis (X=0, Y=1)")]
    public int rotateAxis = 1;
    [Header("Shooting Settings")]
    [SerializeField] private bool automatic = true;
    public bool Automatic => automatic;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float damage = 10f;

    private float fireCooldown = 0f;

    [Header("VFX & SFX")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;

    [Header("Hit Detection")]
    [SerializeField] private MonoBehaviour hitDetectorComponent; // Assign HitDetector or RaycastHitDetector
    private IHitDetector hitDetector;

    private void Awake()
    {
        if (hitDetectorComponent != null && hitDetectorComponent is IHitDetector detector)
            hitDetector = detector;
        else if (hitDetectorComponent != null)
            Debug.LogWarning("Assigned hit detector does not implement IHitDetector!");
    }

    private void Update()
    {
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
    }

    public void TryShoot()
    {
        if (fireCooldown > 0f)
            return;

        Shoot();
        fireCooldown = fireRate;
    }

    public void FireOnce()
    {
        if (fireCooldown > 0f)
            return;

        Shoot();
        fireCooldown = fireRate;
    }

    public void Shoot()
    {
        vibrate.Invoke();
        //Debug.Log("Weapon fired!");
        if (muzzleFlash != null)
            muzzleFlash.Play();

        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);

        if (hitDetector != null)
            hitDetector.DetectHits(damage);

        if (xrcomp != null)
        {
            if (rotateAxis == 0)
            {
                xrcomp.GetComponent<rotatedouble>().Trigger();
            }
            else if (rotateAxis == 1)
            {
                xrcomp.GetComponent<rotateOnShoot>().Trigger();
            }
            else
            {
                Debug.LogWarning("XR Component not assigned for rotation!");
            }
        }
    }
}
