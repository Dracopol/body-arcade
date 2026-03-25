using UnityEngine;
using UnityEngine.Splines;

public class targets : MonoBehaviour
{
    [Header("Spline Spawning")]
    [SerializeField] private GameObject redPrefab;
    [SerializeField] private GameObject yellowPrefab;
    [SerializeField] private GameObject bluePrefab;

    [SerializeField] private SplineContainer targetSpline1;
    [SerializeField] private SplineContainer targetSpline2;

    private void SpawnAndAnimate(GameObject animatedPrefab, int spline)
    {
        SplineContainer targetSpline = spline == 1 ? targetSpline1 : targetSpline2;

        Vector3 spawnPos = targetSpline.EvaluatePosition(0f);
        GameObject instance = Instantiate(animatedPrefab, spawnPos, Quaternion.identity);

        SplineAnimate splineAnimate = instance.GetComponent<SplineAnimate>();
        if (splineAnimate == null)
        {
            Debug.LogWarning("Buttons: Prefab is missing a SplineAnimate component.");
            Destroy(instance);
            return;
        }

        splineAnimate.Container = targetSpline;
        splineAnimate.Loop = SplineAnimate.LoopMode.Once; 
        splineAnimate.Completed += () => Destroy(instance); 
        splineAnimate.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SpawnAndAnimate(redPrefab, 1);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            SpawnAndAnimate(yellowPrefab, 1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnAndAnimate(bluePrefab, 1);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            SpawnAndAnimate(redPrefab, 2);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SpawnAndAnimate(yellowPrefab, 2);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            SpawnAndAnimate(bluePrefab, 2);
        }
    }
}
