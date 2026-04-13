using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes
{
    /// <summary>
    /// Tracks hand progress along a spline by checking the hand's collider against
    /// the nearest point on the spline. Exposes progress via event and property for
    /// other systems (visuals, score, etc.) to consume independently.
    /// </summary>
    internal class SplineProgressTracker : MonoBehaviour
    {
        #region Events

        internal event Action<float> OnProgressChange;

        #endregion

        #region Serialized fields

        [Header("References")]
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private Transform _handTransform;
        [SerializeField] private Collider _handCollider;

        [Header("Behaviour")]
        [SerializeField] private float _radiusTolerance = 1.5f;
        [SerializeField] private float _tubeRadius = 0.05f;
        [SerializeField] private bool _forwardOnly = true;
        [SerializeField] private bool _reverse = false;
        [SerializeField] private float _drainSpeedPerSecond = 5f;

        [Header("Debug")]
        [SerializeField, Range(0f, 1f)] private float _debugProgress = 0f;
        [SerializeField] private bool _debugHandInside = false;

        #endregion

        #region Fields

        private float _progress = 0f;

        #endregion

        #region Properties

        internal float Progress => _progress;

        #endregion

        #region Update

        private void Update()
        {
            if (_handTransform == null || _splineContainer == null || _handCollider == null)
                return;

            float3 localHandPosition = _splineContainer.transform
                .InverseTransformPoint(_handTransform.position);

            SplineUtility.GetNearestPoint(
                _splineContainer.Spline,
                localHandPosition,
                out float3 nearestLocalPoint,
                out float t,
                resolution: 64);

            if (_reverse)
                t = 1f - t;

            Vector3 nearestWorldPoint = _splineContainer.transform
                .TransformPoint(nearestLocalPoint);

            Vector3 closestPointOnHand = Physics.ClosestPoint(
                nearestWorldPoint,
                _handCollider,
                _handTransform.position,
                _handTransform.rotation);

            float distanceFromSpline = Vector3.Distance(nearestWorldPoint, closestPointOnHand);

            bool handInside = distanceFromSpline <= _tubeRadius * _radiusTolerance;
            _debugHandInside = handInside;

            if (handInside)
            {
                float newProgress = _forwardOnly ? Mathf.Max(_progress, t) : t;
                SetProgress(newProgress);
            }
            else
            {
                SetProgress(Mathf.Lerp(_progress, 0f, Time.deltaTime * _drainSpeedPerSecond));
            }
        }

        #endregion

        #region Private

        private void SetProgress(float t)
        {
            _progress = t;
            _debugProgress = t;
            OnProgressChange?.Invoke(_progress);
        }

        #endregion
    }
}
