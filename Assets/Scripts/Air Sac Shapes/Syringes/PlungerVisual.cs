using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes.Syringes
{
    internal class PlungerVisual : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private Transform _target;
        [SerializeField] private Collider _collider;

        [Header("Behaviour")]
        [SerializeField] private bool _alignRotationToSpline = true;

        #endregion

        #region Fields

        private PlungerTracker _tracker;
        private Spline _spline;
        private Transform _splineTransform;

        #endregion

        #region Properties

        internal Collider Collider => _collider;

        #endregion

        #region Internal

        internal void SetTracker(PlungerTracker tracker, SplineContainer splineContainer)
        {
            if (_tracker != null)
                _tracker.OnMaxProgressChange -= HandleMaxProgressChange;

            _tracker = tracker;

            if (splineContainer != null)
            {
                _spline          = splineContainer.Spline;
                _splineTransform = splineContainer.transform;
            }
            else
            {
                _spline          = null;
                _splineTransform = null;
            }

            if (_tracker != null)
            {
                _tracker.OnMaxProgressChange += HandleMaxProgressChange;
                HandleMaxProgressChange(_tracker.MaxProgress);
            }
        }

        #endregion

        #region Event handlers

        private void HandleMaxProgressChange(float maxProgress)
        {
            if (_spline == null || _target == null || _tracker == null || _splineTransform == null)
                return;

            float t = _tracker.IsReversed ? 1f - maxProgress : maxProgress;

            SplineUtility.Evaluate(_spline, t, out float3 localPosition, out float3 localTangent, out float3 localUp);

            _target.position = _splineTransform.TransformPoint(localPosition);

            if (!_alignRotationToSpline)
                return;

            float3 tangent       = _tracker.IsReversed ? -localTangent : localTangent;
            Vector3 worldTangent = _splineTransform.TransformDirection(tangent);
            Vector3 worldUp      = _splineTransform.TransformDirection(localUp);

            if (worldTangent != Vector3.zero)
                _target.rotation = Quaternion.LookRotation(worldTangent, worldUp);
        }

        #endregion
    }
}
