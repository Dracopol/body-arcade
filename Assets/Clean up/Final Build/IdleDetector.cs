using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects player idle state by monitoring input and tracked transforms (HMD/controllers/player).
/// Attach to a persistent player/root object and assign the transforms to watch.
/// </summary>
public class IdleDetector : MonoBehaviour
{
    [Tooltip("Seconds of inactivity before the player is considered idle.")]
    public float idleTimeout = 30f;

    [Tooltip("Minimum position delta (meters) considered movement for tracked transforms.")]
    public float positionThreshold = 0.01f;

    [Tooltip("Minimum rotation delta (degrees) considered movement for tracked transforms.")]
    public float rotationThresholdDegrees = 1f;

    [Tooltip("Transforms to monitor for motion (e.g. Camera (HMD), left/right controllers, player root)")]
    public Transform[] trackedTransforms;

    [Tooltip("Enable checking keyboard/mouse/controller input (Input.anyKey and mouse movement).")]
    public bool useInputChecks = true;

    public UnityEvent onBecameIdle;
    public UnityEvent onBecameActive;

    public bool IsIdle { get; private set; }

    private float lastActiveTime;
    private Vector3[] lastPositions;
    private Quaternion[] lastRotations;
    private Vector3 lastMousePos;

    void Start()
    {
        lastActiveTime = Time.time;
        if (trackedTransforms != null && trackedTransforms.Length > 0)
        {
            lastPositions = new Vector3[trackedTransforms.Length];
            lastRotations = new Quaternion[trackedTransforms.Length];
            for (int i = 0; i < trackedTransforms.Length; i++)
            {
                if (trackedTransforms[i] != null)
                {
                    lastPositions[i] = trackedTransforms[i].position;
                    lastRotations[i] = trackedTransforms[i].rotation;
                }
            }
        }

        lastMousePos = Input.mousePosition;
        IsIdle = false;
    }

    void Update()
    {
        bool active = false;

        if (useInputChecks)
        {
            if (Input.anyKey)
                active = true;

            Vector3 mp = Input.mousePosition;
            if ((mp - lastMousePos).sqrMagnitude > 0f)
            {
                active = true;
                lastMousePos = mp;
            }
        }

        if (trackedTransforms != null && trackedTransforms.Length > 0)
        {
            for (int i = 0; i < trackedTransforms.Length; i++)
            {
                var t = trackedTransforms[i];
                if (t == null) continue;

                if ((t.position - lastPositions[i]).sqrMagnitude > positionThreshold * positionThreshold)
                {
                    active = true;
                    lastPositions[i] = t.position;
                }

                if (Quaternion.Angle(t.rotation, lastRotations[i]) > rotationThresholdDegrees)
                {
                    active = true;
                    lastRotations[i] = t.rotation;
                }
            }
        }

        if (active)
        {
            lastActiveTime = Time.time;
            if (IsIdle)
            {
                IsIdle = false;
                Debug.Log("[IdleDetector] Player is now ACTIVE");
                onBecameActive?.Invoke();
            }
        }
        else
        {
            if (!IsIdle && Time.time - lastActiveTime >= idleTimeout)
            {
                IsIdle = true;
                Debug.Log("[IdleDetector] Player is now IDLE");
                onBecameIdle?.Invoke();
            }
        }
    }

    /// <summary>
    /// Resets the idle timer and marks the player as active.
    /// </summary>
    public void ResetIdle()
    {
        lastActiveTime = Time.time;
        if (IsIdle)
        {
            IsIdle = false;
            Debug.Log("[IdleDetector] Player idle manually reset");
            onBecameActive?.Invoke();
        }
    }

    /// <summary>
    /// Convenience setter for the idle timeout.
    /// </summary>
    public void SetIdleTimeout(float seconds)
    {
        idleTimeout = Mathf.Max(0f, seconds);
    }
}
