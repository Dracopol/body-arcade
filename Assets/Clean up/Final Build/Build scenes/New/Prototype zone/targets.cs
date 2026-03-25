using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.InputSystem;

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
        var kb = Keyboard.current;
        if (kb == null) return;

        // Spline 1
        if (kb.qKey.wasPressedThisFrame) SpawnAndAnimate(redPrefab, 1);
        if (kb.wKey.wasPressedThisFrame) SpawnAndAnimate(yellowPrefab, 1);
        if (kb.eKey.wasPressedThisFrame) SpawnAndAnimate(bluePrefab, 1);

        // Spline 2
        if (kb.aKey.wasPressedThisFrame) SpawnAndAnimate(redPrefab, 2);
        if (kb.sKey.wasPressedThisFrame) SpawnAndAnimate(yellowPrefab, 2);
        if (kb.dKey.wasPressedThisFrame) SpawnAndAnimate(bluePrefab, 2);
    }
}