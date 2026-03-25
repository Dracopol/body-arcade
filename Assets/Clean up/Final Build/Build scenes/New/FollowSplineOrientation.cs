using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Smooths rotation from SplineAnimate using SLERP to prevent snapping at spline corners.
/// Attach to the same GameObject as SplineAnimate for smooth camera rotation.
/// </summary>
public class FollowSplineOrientation : MonoBehaviour
{
    [SerializeField] private SplineAnimate splineAnimate;
    [SerializeField, Range(0.01f, 1f), Tooltip("How quickly the rotation follows the spline tangent. Higher = smoother.")]
    private float rotationSmoothTime = 0.15f;
    
    private Quaternion targetRotation = Quaternion.identity;
    private Quaternion smoothedRotation = Quaternion.identity;
    private SplinePath<Spline> splinePath;
    
    void Start()
    {
        if (splineAnimate == null)
            splineAnimate = GetComponent<SplineAnimate>();
            
        if (splineAnimate == null || splineAnimate.Container == null)
        {
            Debug.LogError("FollowSplineOrientation requires a SplineAnimate component with a valid SplineContainer!", this);
            enabled = false;
            return;
        }
        
        smoothedRotation = transform.rotation;
        targetRotation = transform.rotation;
        
        // Cache the spline path
        splinePath = new SplinePath<Spline>(splineAnimate.Container.Splines);
    }

    void LateUpdate()
    {
        if (splineAnimate == null || splineAnimate.Container == null)
            return;

        if (!splineAnimate.IsPlaying)
            return;

        // Get current normalized position on spline from SplineAnimate
        float t = splineAnimate.NormalizedTime % 1f;
        
        Debug.Log($"SplineAnimate - IsPlaying: {splineAnimate.IsPlaying}, NormalizedTime: {splineAnimate.NormalizedTime}, t: {t}");
        
        // Calculate target rotation from spline tangent
        UpdateTargetRotation(t);
        
        Debug.Log($"TargetRotation: {targetRotation}, Current smoothed: {smoothedRotation}");
        
        // Smoothly interpolate rotation using SLERP
        smoothedRotation = Quaternion.Slerp(smoothedRotation, targetRotation, Time.deltaTime / rotationSmoothTime);
        transform.rotation = smoothedRotation;
    }
    
    private void UpdateTargetRotation(float t)
    {
        Vector3 forward = splineAnimate.Container.EvaluateTangent(splinePath, t);
        
        // Handle zero-length tangents
        if (Vector3.Magnitude(forward) <= Mathf.Epsilon)
        {
            if (t < 1f)
                forward = splineAnimate.Container.EvaluateTangent(splinePath, Mathf.Min(1f, t + 0.01f));
            else
                forward = splineAnimate.Container.EvaluateTangent(splinePath, t - 0.01f);
        }
        
        forward = Vector3.Normalize(forward);
        Vector3 up = splineAnimate.Container.EvaluateUpVector(splinePath, t);
        
        // Calculate target rotation
        if (Vector3.Magnitude(forward) > Mathf.Epsilon && Vector3.Magnitude(up) > Mathf.Epsilon)
            targetRotation = Quaternion.LookRotation(forward, up);
    }
}
