using System;
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

        private ProgressState _progressState;
        private SplineProximity _splineProximity;

        #endregion

        #region Properties

        internal float Progress => _progressState.Progress;
        internal bool IsReversed => _isReversed;
        internal bool IsWin => _progressState.IsWin;

        #endregion

        #region Setup

        private void Awake()
        {
            _progressState = new ProgressState(_winThreshold);
            _progressState.OnProgressChange += HandleProgressChange;
            _progressState.OnMaxProgressChange += HandleMaxProgressChange;
            _progressState.OnWinStateChange += HandleWinStateChange;

            if (_splineContainer != null && _handCollider != null)
            {
                _splineProximity = new SplineProximity(
                    _splineContainer.Spline,
                    _splineContainer.transform,
                    _handCollider);
            }
        }

        private void OnDestroy()
        {
            if (_progressState == null)
                return;

            _progressState.OnProgressChange -= HandleProgressChange;
            _progressState.OnMaxProgressChange -= HandleMaxProgressChange;
            _progressState.OnWinStateChange -= HandleWinStateChange;
        }

        #endregion

        #region Update

        private void Update()
        {
            if (_debugOverrideEnabled)
            {
                _progressState.SetProgress(_debugProgressOverride, false);
                return;
            }

            if (_handTransform == null || _splineProximity == null)
                return;

            if (_progressState.IsWin)
            {
                float filledProgress = Mathf.MoveTowards(
                    _progressState.Progress,
                    1f,
                    Time.deltaTime * _fillUpPerSecond);

                _progressState.SetProgress(filledProgress, true);
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
                    ? Mathf.Max(_progressState.Progress, splineProgress)
                    : splineProgress;

                _progressState.SetProgress(newProgress, false);
            }
            else
            {
                float drainedProgress = Mathf.Lerp(
                    _progressState.Progress,
                    0f,
                    Time.deltaTime * _drainSpeedPerSecond);

                _progressState.SetProgress(drainedProgress, true);
            }

            Debug.DrawLine(
                closestSplinePoint,
                closestColliderPoint,
                isHandInside ? Color.green : Color.red);
        }

        #endregion

        #region Event handlers

        private void HandleProgressChange(float progress)
        {
            _debugProgress = progress;
            OnProgressChange?.Invoke(progress);
        }

        private void HandleMaxProgressChange(float maxProgress)
        {
            _debugMaxProgress = maxProgress;
            OnMaxProgressChange?.Invoke(maxProgress);
        }

        private void HandleWinStateChange()
        {
            OnWinStateChange?.Invoke();
        }

        #endregion
    }
}