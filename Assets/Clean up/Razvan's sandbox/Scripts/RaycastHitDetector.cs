using UnityEngine;

[RequireComponent(typeof(Weapon))]
[RequireComponent(typeof(WeaponInput))]
public class RaycastHitDetector : MonoBehaviour, IHitDetector
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask hitLayer;

    [Header("Optional Visualizer")]
    [SerializeField] private RaycastHitVisualizer visualizer;

    public void DetectHits(float damage)
    {
        if (firePoint == null) return;

        Vector3 start = firePoint.position;
        Vector3 direction = firePoint.forward;

        if (Physics.Raycast(start, direction, out RaycastHit hit, range, hitLayer))
        {
            Debug.Log($"Raycast hit: {hit.collider.name}");

            // Visual effect (only if not null)
            visualizer?.ShowRay(start, hit.point);

            Target hp = hit.collider.GetComponent<Target>();
            if (hp != null)
                hp.TakeDamage(damage);
        }
        else
        {
            // Missed → still show visual if we want
            Vector3 end = start + direction * range;
            visualizer?.ShowRay(start, end);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.forward * range);
    }
}
