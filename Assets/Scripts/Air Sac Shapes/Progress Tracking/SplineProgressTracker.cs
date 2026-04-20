using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes.ProgressTrackering
{
    internal class SplineProgressTracker : MonoBehaviour
    {
        #region Events

        internal event Action<float> OnProgressChange;
        internal event Action<float> OnMaxProgressChange;
        internal event Action OnWinStateChange;

        #endregion

        #region Serialized fields

        [Header("References")]
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private Transform _handTransform;
        [SerializeField] private Collider _handCollider;
        [SerializeField] private Collider _donutCollider;

        [Header("Behaviour")]
        [SerializeField] private float _radiusToleranceMultiplier = 1.5f;
        [SerializeField] private float _tubeRadiusMeters = 0.05f;
        [SerializeField] private bool _isForwardOnly = true;
        [SerializeField] private bool _isReversed = false;
        [SerializeField] private float _drainSpeedPerSecond = 5f;
        [SerializeField] private float _fillUpPerSecond = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _winThreshold = 0.98f;

        [Header("Debug visualization")]
        [SerializeField, Range(0f, 1f)] private float _debugProgress;
        [SerializeField] private bool _debugIsHandInside;
        [SerializeField, Range(0f, 1f)] private float _debugMaxProgress;

        [Header("Debug override")]
        [SerializeField] private bool _debugOverrideEnabled;
        [SerializeField, Range(0f, 1f)] private float _debugProgressOverride;

        #endregion

        #region Fields

        private float _progress;
        private float _maxProgress;
        private bool _isWin;

        private SplineProximity _splineProximity;

        #endregion

        #region Properties

        internal float Progress => _progress;
        internal float MaxProgress => _maxProgress;
        internal bool IsWin => _isWin;
        internal bool IsReversed => _isReversed;

        #endregion

        #region Setup

        private void Awake()
        {
            if (_splineContainer != null && _handCollider != null)
            {
                _splineProximity = new SplineProximity(
                    _splineContainer.Spline,
                    _splineContainer.transform,
                    _handCollider);
            }
        }

        #endregion

        #region Update

        private void Update()
        {
            if (_debugOverrideEnabled)
            {
                SetCurrentProgress(_debugProgressOverride);
                return;
            }

            if (_handTransform == null || _splineProximity == null)
                return;

            if (_isWin)
            {
                float filledProgress = Mathf.MoveTowards(_progress, 1f, Time.deltaTime * _fillUpPerSecond);
                SetCurrentProgress(filledProgress);
                SetMaxProgress(filledProgress);
                return;
            }

            float splineProgress = _splineProximity.FindClosestSplineProgress(
                out Vector3 closestSplinePoint,
                out Vector3 closestColliderPoint);

            if (_isReversed)
                splineProgress = 1f - splineProgress;

            float distanceToSplineMeters = (closestSplinePoint - closestColliderPoint).magnitude;
            bool isHandInside = distanceToSplineMeters <= _tubeRadiusMeters * _radiusToleranceMultiplier;

            _debugIsHandInside = isHandInside;

            if (isHandInside)
            {
                float newProgress = _isForwardOnly
                    ? Mathf.Max(_progress, splineProgress)
                    : splineProgress;

                if (_progress >= _winThreshold)
                    newProgress = 1f;

                bool isHandInsideDonut = _donutCollider != null && Physics.ComputePenetration(
                    _handCollider, _handCollider.transform.position, _handCollider.transform.rotation,
                    _donutCollider, _donutCollider.transform.position, _donutCollider.transform.rotation,
                    out _, out _);

                if (newProgress > _maxProgress && isHandInsideDonut)
                {
                    SetMaxProgress(newProgress);
                    SetCurrentProgress(newProgress);
                }

                if (newProgress < _maxProgress && !isHandInsideDonut)
                {
                    SetCurrentProgress(newProgress);
                }
            }
            else
            {
                float drainedProgress = Mathf.Lerp(_progress, 0f, Time.deltaTime * _drainSpeedPerSecond);
                SetCurrentProgress(drainedProgress);
                SetMaxProgress(drainedProgress);
            }

            Debug.DrawLine(
                closestSplinePoint,
                closestColliderPoint,
                isHandInside ? Color.green : Color.red);
        }

        #endregion

        #region Private

        private void SetCurrentProgress(float progress)
        {
            _progress = progress;
            _debugProgress = progress;
            OnProgressChange?.Invoke(_progress);
        }

        private void SetMaxProgress(float maxProgress)
        {
            _maxProgress = maxProgress;
            _debugMaxProgress = maxProgress;
            OnMaxProgressChange?.Invoke(_maxProgress);

            if (_isWin || _maxProgress < 1f)
                return;

            _isWin = true;
            _progress = 0f;
            _maxProgress = 0f;
            Debug.Log("Win condition reached!");
            OnWinStateChange?.Invoke();
        }

        #endregion
    }
}