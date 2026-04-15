using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes.ProgressTracker
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

        private float _progress;
        private float _maxProgress = 0f;
        private bool _isWin = false;

        private Transform _splineTransform;
        private Spline _spline;

        private const int CoarseStepCount = 12;
        private const int RefineStepCount = 6;

        #endregion

        #region Properties

        internal float Progress => _progress;
        internal bool IsReversed => _isReversed;
        internal bool IsWin => _isWin;

        #endregion

        #region Setup

        private void Awake()
        {
            if (_splineContainer != null)
            {
                _splineTransform = _splineContainer.transform;
                _spline = _splineContainer.Spline;
            }
        }

        #endregion

        #region Update

        private void Update()
        {
            if (_debugOverrideEnabled)
            {
                SetProgress(_debugProgressOverride, false);
                return;
            }

            if (_handTransform == null || _handCollider == null || _spline == null)
                return;

            if (_isWin)
            {
                float drainedProgress = Mathf.MoveTowards(_progress, 1f, Time.deltaTime * _fillUpPerSecond);
                SetProgress(drainedProgress, true);
                return;
            }

            float splineProgress = FindClosestSplineProgress(
                out Vector3 closestSplinePoint,
                out Vector3 closestColliderPoint);

            if (_isReversed)
                splineProgress = 1f - splineProgress;

            float distanceToSplineMeters = (closestSplinePoint - closestColliderPoint).magnitude;

            bool isHandInside = distanceToSplineMeters <=
                                _tubeRadiusMeters * _radiusToleranceMultiplier;

            _debugIsHandInside = isHandInside;

            if (isHandInside)
            {
                float newProgress = _isForwardOnly
                    ? Mathf.Max(_progress, splineProgress)
                    : splineProgress;

                SetProgress(newProgress, false);
            }
            else
            {
                float drainedProgress = Mathf.Lerp(
                    _progress,
                    0f,
                    Time.deltaTime * _drainSpeedPerSecond);

                SetProgress(drainedProgress, true);
            }

            Debug.DrawLine(
                closestSplinePoint,
                closestColliderPoint,
                isHandInside ? Color.green : Color.red);
        }

        #endregion

        #region Private

        private float FindClosestSplineProgress(
            out Vector3 closestSplinePoint,
            out Vector3 closestColliderPoint)
        {
            float bestProgress = 0f;
            float smallestDistanceSquared = float.MaxValue;

            float coarseStepSize = 1f / CoarseStepCount;

            // --- Coarse search ---
            for (int stepIndex = 0; stepIndex <= CoarseStepCount; stepIndex++)
            {
                float progress = stepIndex * coarseStepSize;

                float distanceSquared = GetDistanceSquaredToCollider(progress);

                if (distanceSquared < smallestDistanceSquared)
                {
                    smallestDistanceSquared = distanceSquared;
                    bestProgress = progress;
                }
            }

            // --- Refinement search ---
            float searchRange = coarseStepSize;

            for (int iterationIndex = 0; iterationIndex < RefineStepCount; iterationIndex++)
            {
                float leftProgress = Mathf.Clamp01(bestProgress - searchRange);
                float rightProgress = Mathf.Clamp01(bestProgress + searchRange);

                float progressA = Mathf.Lerp(leftProgress, rightProgress, 0.33f);
                float progressB = Mathf.Lerp(leftProgress, rightProgress, 0.66f);

                float distanceA = GetDistanceSquaredToCollider(progressA);
                float distanceB = GetDistanceSquaredToCollider(progressB);

                bestProgress = distanceA < distanceB ? progressA : progressB;

                searchRange *= 0.5f;
            }

            float3 localPoint = SplineUtility.EvaluatePosition(_spline, bestProgress);
            closestSplinePoint = _splineTransform.TransformPoint(localPoint);
            closestColliderPoint = _handCollider.ClosestPoint(closestSplinePoint);

            return bestProgress;
        }

        private float GetDistanceSquaredToCollider(float progress)
        {
            float3 localPoint = SplineUtility.EvaluatePosition(_spline, progress);
            Vector3 worldPoint = _splineTransform.TransformPoint(localPoint);

            Vector3 closestPointOnCollider = _handCollider.ClosestPoint(worldPoint);

            return (worldPoint - closestPointOnCollider).sqrMagnitude;
        }

        private void SetProgress(float progress, bool drainOverride)
        {
            if (drainOverride)
            {
                SetCurrentProgress(progress);
                SetMaxProgress(progress);
                return;
            }

            if (_progress >= _winThreshold)
            {
                progress = 1f;
            }

            SetCurrentProgress(progress);

            if (progress > _maxProgress)
            {
                SetMaxProgress(progress);

                if (!_isWin && progress == 1f)
                {
                    _isWin = true;
                    _progress = 0f;
                    _maxProgress = 0f;
                    Debug.Log("Win condition reached!");
                    OnWinStateChange?.Invoke();
                }
            }
        }

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
        }

        #endregion
    }
}