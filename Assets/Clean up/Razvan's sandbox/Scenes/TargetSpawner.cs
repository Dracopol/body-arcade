using UnityEngine;
using UnityEngine.Splines;

public class TargetSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject allyPrefab;

    [Header("Speed")]
    public float movementSpeed = 5f;

    [Header("Spline")]
    public SplineContainer splineContainer;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (splineContainer != null)
                    {
                        
                    }
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            SpawnAllyOnSpline(splineContainer, movementSpeed);
        }
    }

    public void SpawnEnemyOnSpline(SplineContainer spline, float speed = 5f)
    {
        SpawnOnSpline(enemyPrefab, spline, speed, "Enemy");
    }

    public void SpawnAllyOnSpline(SplineContainer spline, float speed = 5f)
    {
        SpawnOnSpline(allyPrefab, spline, speed, "Ally");
    }

    private GameObject SpawnOnSpline(GameObject prefab, SplineContainer spline, float speed, string prefabLabel)
    {
        if (spline == null || prefab == null)
        {
            Debug.LogWarning($"Spline or {prefabLabel} prefab is missing!");
            return null;
        }

        // Instantiate at start of spline
        GameObject obj = Instantiate(prefab, spline.EvaluatePosition(0f), Quaternion.identity);

        // Initialize its spline follower
        TargetSplineFollower follower = obj.GetComponent<TargetSplineFollower>();
        if (follower != null)
        {
            follower.Initialize(spline, speed);
        }
        else
        {
            Debug.LogWarning($"{prefabLabel} prefab is missing TargetSplineFollower!");
        }

        return obj;
    }
}
