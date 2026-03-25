using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineAnimate))]
public class SplineAnimateSmoothCamera : MonoBehaviour
{
    [Header("Smoothing")]
    [Tooltip("Higher = snappier, Lower = smoother")]
    public float rotationSmoothSpeed = 5f;

    [Header("Lookahead")]
    [Tooltip("How far ahead along the spline (normalized 0-1) to sample for direction")]
    [Range(0.001f, 0.1f)]
    public float lookaheadDistance = 0.02f;

    [Header("Up Vector")]
    public Vector3 worldUp = Vector3.up;

    private SplineAnimate _splineAnimate;
    private SplineContainer _splineContainer;

    void Awake()
    {
        _splineAnimate = GetComponent<SplineAnimate>();
        _splineContainer = _splineAnimate.Container;
    }

    void LateUpdate()
    {
        if (_splineContainer == null) return;

        float currentT = _splineAnimate.NormalizedTime;
        float lookaheadT = Mathf.Clamp01(currentT + lookaheadDistance);

        if (currentT >= 1f - lookaheadDistance)
            lookaheadT = Mathf.Clamp01(currentT - lookaheadDistance);

        Vector3 currentPos = transform.position;
        Vector3 futurePos = _splineContainer.EvaluatePosition(lookaheadT);

        Vector3 direction = (futurePos - currentPos);

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, worldUp);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSmoothSpeed
        );
    }
}