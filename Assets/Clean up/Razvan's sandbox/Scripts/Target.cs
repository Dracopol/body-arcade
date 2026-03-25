using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
public class Target : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] ParticleSystem deathEffect;
    [Header("Health Settings")]
    [SerializeField] public float maxHealth = 100f;
    public float currentHealth;

    [Header("Force Settings")]
    [SerializeField] private Transform forceOrigin;
    [SerializeField] private float force = 5f;

    [Header("Size Settings")]
    [SerializeField] private Vector3 idealScale = Vector3.one * 0.2f;
    [SerializeField] private float sizeChangeSpeed = 5f;

    private Rigidbody rb;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isDead = false;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        currentHealth = maxHealth;

        try
        {
            rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        catch
        {
            //Debug.LogWarning("No Rigidbody found on this GameObject.");
        }


    }

    private void Update()
    {
        transform.localScale =
            Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * sizeChangeSpeed);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"{gameObject.name} took {damage} damage. Remaining HP: {currentHealth}");

        if (forceOrigin != null)
            ApplyDamageForce();

        UpdateScaleBasedOnHealth();

        if (damageSound != null)
        {
            if (!isDead) audioSource.PlayOneShot(damageSound);
        }

        if (currentHealth <= 0)
        {
            if (isDead) return;
            currentHealth = 0;
            Die();
            isDead = true;
        }

    }

    private void ApplyDamageForce()
    {
        Vector3 dir = (forceOrigin.position - transform.position).normalized;
        Vector3 directionalForce = dir * force;
        rb.AddForce(directionalForce, ForceMode.Impulse);
    }

    private void UpdateScaleBasedOnHealth()
    {
        float healthPercent = currentHealth / maxHealth;

        targetScale = Vector3.Lerp(idealScale, originalScale, healthPercent);
    }

    private void Die()
    {
        if (deathEffect != null)
        {
            audioSource.PlayOneShot(deathSound);
            deathEffect.transform.position = transform.position;
            deathEffect.transform.rotation = transform.rotation;
            deathEffect.Play();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (forceOrigin == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, forceOrigin.position);
    }
}
