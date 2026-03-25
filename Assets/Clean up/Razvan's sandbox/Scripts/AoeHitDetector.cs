using UnityEngine;

[RequireComponent(typeof(Weapon))]
[RequireComponent(typeof(WeaponInput))]
public class AoeHitDetector : MonoBehaviour, IHitDetector
{
    [Header("References")]
    [SerializeField] private GameObject referenceParent;
    [SerializeField] private LayerMask detectionLayer;

    private GameObject[] referenceCubes;

    private void Awake()
    {
        if (referenceParent != null)
        {
            referenceCubes = new GameObject[referenceParent.transform.childCount];
            for (int i = 0; i < referenceParent.transform.childCount; i++)
                referenceCubes[i] = referenceParent.transform.GetChild(i).gameObject;
        }
        else
        {
            referenceCubes = new GameObject[0];
            Debug.LogWarning("HitDetector: Reference parent not assigned!");
        }
    }

    public void DetectHits(float damage)
    {
        if (referenceCubes == null || referenceCubes.Length == 0) return;

        foreach (var cube in referenceCubes)
        {
            if (cube == null) continue;

            Vector3 center = cube.transform.position;
            Quaternion rotation = cube.transform.rotation;
            Vector3 size = cube.transform.localScale;

            Collider[] hits = Physics.OverlapBox(center, size / 2, rotation, detectionLayer);

            foreach (var hit in hits)
            {
                Debug.Log($"Cube '{cube.name}' hit {hit.name}");

                Target hp = hit.GetComponent<Target>();
                if (hp != null)
                    hp.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (referenceParent == null) return;

        Gizmos.color = Color.cyan;
        foreach (Transform cube in referenceParent.transform)
        {
            Gizmos.matrix = Matrix4x4.TRS(cube.position, cube.rotation, cube.localScale);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}
