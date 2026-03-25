using UnityEngine;
using UnityEngine.Splines;

public class TargetSplineFollower : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;      // Units per second
    private SplineContainer spline;
    private float t = 0f;         // 0=start, 1=end
    private float splineLength;

    private bool isMoving = false;

    // Call this to start moving along a spline
    public void Initialize(SplineContainer targetSpline, float moveSpeed = 3f)
    {
        spline = targetSpline;
        speed = moveSpeed;
        t = 0f;
        isMoving = true;

        // Precalculate spline length
        if (spline != null)
            splineLength = spline.CalculateLength();
    }

    void Update()
    {


        if (!isMoving || spline == null) return;

        // Move along spline based on speed
        t += (speed * Time.deltaTime) / splineLength;

        if (t >= 1f)
        {
            // Reached end
            isMoving = false;
            OnReachedEnd();
            return;
        }

        // Update position and rotation
        Vector3 pos = spline.EvaluatePosition(t);
        Vector3 tangent = spline.EvaluateTangent(t);

        transform.position = pos;
        if (tangent != Vector3.zero)
            transform.forward = tangent.normalized;
    }

    protected virtual void OnReachedEnd()
    {
        // Default: destroy enemy
        Destroy(gameObject);
    }
}
