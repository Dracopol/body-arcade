using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes.ProgressTracker
{
    internal class SplineProgressFollower : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private SplineProgressTracker _tracker;
        [SerializeField] private Transform _target;

        [Header("Behaviour")]
        [SerializeField] private bool _alignRotationToSpline = true;

        #endregion

        #region Fields

        private Spline _spline;
        private Transform _splineTransform;

        #endregion

        #region Setup

        private void Awake()
        {
            if (_splineContainer != null)
            {
                _spline = _splineContainer.Spline;
                _splineTransform = _splineContainer.transform;
            }
        }

        private void OnEnable()
        {
            if (_tracker != null)
                _tracker.OnMaxProgressChange += HandleMaxProgressChange;
        }

        private void OnDisable()
        {
            if (_tracker != null)
                _tracker.OnMaxProgressChange -= HandleMaxProgressChange;
        }

        #endregion

        #region Event handlers

        private void HandleMaxProgressChange(float maxProgress)
        {
            if (_spline == null || _target == null)
                return;

            SplineUtility.Evaluate(_spline, maxProgress, out float3 localPosition, out float3 localTangent, out float3 localUp);

            _target.position = _splineTransform.TransformPoint(localPosition);

            if (!_alignRotationToSpline)
                return;

            float3 tangent = _tracker.IsReversed ? -localTangent : localTangent;
            Vector3 worldTangent = _splineTransform.TransformDirection(tangent);
            Vector3 worldUp = _splineTransform.TransformDirection(localUp);

            if (worldTangent != Vector3.zero)
                _target.rotation = Quaternion.LookRotation(worldTangent, worldUp);
        }

        #endregion
    }
}