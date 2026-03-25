using UnityEngine;

public class HandSyncBiopod : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private Vector3 localOffset = Vector3.zero;
    [SerializeField] private Vector3 rotationOffset = Vector3.zero; // New rotation offset

    [Header("Smoothing Settings")]
    [SerializeField] private float positionSmoothTime = 0.1f;
    [SerializeField] private float rotationSmoothTime = 0.1f;

    private Vector3 _velocity = Vector3.zero;
    private Quaternion _rotationRef; // For SmoothDampAngle

    private void Update()
    {
        // Error prevention
        if (targetObject == null) return;

        Vector3 worldOffset = targetObject.transform.TransformDirection(localOffset);
        Vector3 targetPosition = targetObject.transform.position + worldOffset;

        // Rotation handling with offset
        Quaternion targetRotation = targetObject.transform.rotation * Quaternion.Euler(rotationOffset);

        // Smooth Position and Orientation
        transform.SetPositionAndRotation(Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            positionSmoothTime
        ), Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime / rotationSmoothTime
        ));
    }
}