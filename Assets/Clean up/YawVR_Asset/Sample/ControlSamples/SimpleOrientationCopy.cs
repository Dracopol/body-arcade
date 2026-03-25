﻿// Yea Imma remove uncessesary old 2021 packages
using UnityEngine;
using YawVR;

public class MotionChairController : MonoBehaviour
{
    // References
    private YawController yawController;

    [Header("Axis Locking")]
    [Tooltip("If checked, X-axis rotation will be ignored")]
    [SerializeField] private bool lockPitchAxis;
    [Tooltip("If checked, Y-axis rotation will be ignored")]
    [SerializeField] private bool lockYawAxis;
    [Tooltip("If checked, Z-axis rotation will be ignored")]
    [SerializeField] private bool lockRollAxis;

    [Header("Axis Inversion")]
    [Tooltip("Inverts the X-axis rotation")]
    [SerializeField] private bool invertPitchAxis;
    [Tooltip("Inverts the Y-axis rotation")]
    [SerializeField] private bool invertYawAxis;
    [Tooltip("Inverts the Z-axis rotation")]
    [SerializeField] private bool invertRollAxis;

    [Header("Rotation Strength")]
    [Tooltip("Strength multiplier for X-axis rotation (0-1.5)")]
    [SerializeField, Range(0f, 1.5f)] private float PitchAxisStrength = 1f;
    [Tooltip("Strength multiplier for Z-axis rotation (0-1.5)")]
    [SerializeField, Range(0f, 1.5f)] private float RollAxisStrength = 1f;

    private void Start() {
        yawController = YawController.Instance();
    }

    private void Update() {
        Vector3 currentRotation = transform.localEulerAngles;
        Vector3 processedRotation = ProcessRotation(currentRotation);
        if (yawController != null && yawController.TrackerObject != null) {
            yawController.TrackerObject.SetRotation(processedRotation);
        }
    }

    // Update the rotation before applying to the YawVR chair
    private Vector3 ProcessRotation(Vector3 rotation) {
        // Apply angle wrapping
        float x = WrapAngle(rotation.x);
        float y = rotation.y; // The Y-axis does not need to wrap
        float z = WrapAngle(rotation.z);

        // Apply axis locks
        x = lockPitchAxis ? 0f : x;
        y = lockYawAxis ? 0f : y;
        z = lockRollAxis ? 0f : z;

        // Apply strength multipliers, Y axis does not need strength multiplier
        x *= PitchAxisStrength;
        z *= RollAxisStrength;

        // Apply inversion
        x = invertPitchAxis ? -x : x;
        y = invertYawAxis ? -y : y;
        z = invertRollAxis ? -z : z;

        return new Vector3(x, y, z);
    }

    // Wrap the angle to prevent gimbal lock
    private float WrapAngle(float angle) {
        angle %= 360;
        return angle > 180 ? angle - 360 : angle;
    }
}