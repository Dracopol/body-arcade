using UnityEngine;
using System.Collections;

public class rotatedouble : MonoBehaviour
{
    [Tooltip("Axis to rotate around (local). For Z axis use (0,0,1).")]
    public Vector3 axis = Vector3.forward;

    [Tooltip("Max angle in degrees.")]
    public float angle = 30f;

    [Tooltip("Total duration (seconds) for Left→Back→Right→Back.")]
    public float duration = 0.4f;

    [Tooltip("Easing curve (evaluated 0..1).")]
    public AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Use local or world rotation when applying the rotation.")]
    public bool useLocalRotation = true;

    bool hasRotated = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Trigger();
        }
    }

    public void Trigger()
    {
        if (hasRotated) return;
        StartCoroutine(RotateLeftRight());
    }

    IEnumerator RotateLeftRight()
    {
        hasRotated = true;

        float quarter = Mathf.Max(0.0001f, duration / 4f);

        Quaternion original = useLocalRotation ? transform.localRotation : transform.rotation;

        Quaternion left = original * Quaternion.Euler(axis.normalized * angle);
        Quaternion right = original * Quaternion.Euler(axis.normalized * -angle);

        yield return RotateBetween(original, left, quarter);
        // yield return RotateBetween(left, original, quarter);
        // yield return RotateBetween(original, right, quarter);
        yield return RotateBetween(left, right, quarter*2f);
        yield return RotateBetween(right, original, quarter);

        hasRotated = false;
    }

    IEnumerator RotateBetween(Quaternion from, Quaternion to, float time)
    {
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / time);
            float e = ease.Evaluate(f);

            if (useLocalRotation)
                transform.localRotation = Quaternion.Slerp(from, to, e);
            else
                transform.rotation = Quaternion.Slerp(from, to, e);

            yield return null;
        }

        if (useLocalRotation)
            transform.localRotation = to;
        else
            transform.rotation = to;
    }
}