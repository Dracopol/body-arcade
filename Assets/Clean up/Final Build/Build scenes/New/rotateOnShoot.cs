using UnityEngine;
using System.Collections;

public class rotateOnShoot : MonoBehaviour
{
    [Tooltip("Axis to rotate around (local).")]
    public Vector3 axis = Vector3.right;

    [Tooltip("Angle in degrees for the forward rotation (then returns to start).")]
    public float angle = 30f;

    [Tooltip("Total duration (seconds) for forward+back motion.")]
    public float duration = 0.4f;

    [Tooltip("Easing curve (evaluated 0..1).")]
    public AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Use local or world rotation when applying the rotation.")]
    public bool useLocalRotation = true;

    bool hasRotated = false;
    Quaternion initialLocalRotation;
    Quaternion initialWorldRotation;

    void Awake()
    {
        initialLocalRotation = transform.localRotation;
        initialWorldRotation = transform.rotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
         {
             Trigger();
         }
    }
    public void Trigger()
    {
        if (hasRotated) return;
        StartCoroutine(RotateOnceCoroutine());
    }

    IEnumerator RotateOnceCoroutine()
    {
        hasRotated = true;
        float half = Mathf.Max(0.0001f, duration / 2f);

        Quaternion from = useLocalRotation ? transform.localRotation : transform.rotation;
        Quaternion to = from * Quaternion.Euler(axis.normalized * angle);

        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / half);
            float e = ease.Evaluate(f);
            if (useLocalRotation) transform.localRotation = Quaternion.Slerp(from, to, e);
            else transform.rotation = Quaternion.Slerp(from, to, e);
            yield return null;
        }

        if (useLocalRotation) transform.localRotation = to; else transform.rotation = to;

        // return
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / half);
            float e = ease.Evaluate(f);
            if (useLocalRotation) transform.localRotation = Quaternion.Slerp(to, from, e);
            else transform.rotation = Quaternion.Slerp(to, from, e);
            yield return null;
        }

        if (useLocalRotation) transform.localRotation = from; else transform.rotation = from;

        hasRotated = false;
    }

}
