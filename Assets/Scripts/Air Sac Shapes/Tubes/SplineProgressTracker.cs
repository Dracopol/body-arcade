using System;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes.Tubes
{
    internal class SplineProgressTracker
    {
        #region Events

        internal event Action<float> OnProgressChange;
        internal event Action<float> OnMaxProgressChange;
        internal event Action OnWinStateChange;
        internal event Action OnWinReady;

        #endregion

        #region Fields

        // Difficulty tuning — supplied per spawn via ApplyDifficultySettings (from PreparedShape).
        private float _tubeRadiusMeters    = 0.05f;
        private float _drainSpeedPerSecond = 5f;
        private float _winThreshold        = 0.98f;

        private Transform _handTransform;
        private Collider  _handCollider;
        private Collider  _knobCollider;

        private SplineContainer _splineContainer;
        private bool _isReversed;

        private float _progress;
        private float _maxProgress;
        private bool  _isWin;
        private bool  _isAnimatorDriven;

        private SplineProximity _splineProximity;

        #endregion

        #region Properties

        internal float           Progress        => _progress;
        internal float           MaxProgress     => _maxProgress;
        internal bool            IsWin           => _isWin;
        internal bool            IsReversed      => _isReversed;
        internal SplineContainer SplineContainer => _splineContainer;

        #endregion

        #region Internal

        internal void Tick()
        {
            if (_isWin || _isAnimatorDriven) return;
            if (_handTransform == null || _splineProximity == null || _splineContainer == null) return;

            float splineProgress = _splineProximity.FindClosestSplineProgress(
                out Vector3 closestSplinePoint,
                out Vector3 closestColliderPoint);

            if (_isReversed)
                splineProgress = 1f - splineProgress;

            float distanceToSplineMeters = (closestSplinePoint - closestColliderPoint).magnitude;
            bool isHandInside = distanceToSplineMeters <= _tubeRadiusMeters;

            if (!isHandInside)
            {
                float drained = Mathf.Lerp(_progress, 0f, Time.deltaTime * _drainSpeedPerSecond);
                SetProgress(drained);
                SetMaxProgress(drained);
                return;
            }

            bool isHandInsideKnob = _knobCollider != null && Physics.ComputePenetration(
                _handCollider, _handCollider.transform.position, _handCollider.transform.rotation,
                _knobCollider, _knobCollider.transform.position, _knobCollider.transform.rotation,
                out _, out _);

            if (isHandInsideKnob)
            {
                float newProgress = Mathf.Max(_maxProgress, splineProgress);
                if (newProgress >= _winThreshold)
                    newProgress = 1f;
                SetProgress(newProgress);
                SetMaxProgress(newProgress);
            }
            else if (splineProgress < _maxProgress)
            {
                SetProgress(splineProgress);
            }
            else
            {
                float drained = Mathf.Lerp(_progress, 0f, Time.deltaTime * _drainSpeedPerSecond);
                SetProgress(drained);
                SetMaxProgress(drained);
            }
        }

        internal void AssignSpline(SplineContainer splineContainer, bool isReversed)
        {
            ResetState();
            _splineContainer = splineContainer;
            _isReversed      = isReversed;
            InitSplineProximity();
        }

        internal void ResetState()
        {
            _isWin            = false;
            _isAnimatorDriven = false;
            _splineContainer  = null;
            _splineProximity  = null;
            SetProgress(0f);
            SetMaxProgress(0f);
        }

        internal void SetHand(Transform handTransform, Collider handCollider)
        {
            _handTransform = handTransform;
            _handCollider  = handCollider;
            InitSplineProximity();
        }

        internal void SetKnobCollider(Collider knobCollider)
        {
            _knobCollider = knobCollider;
        }

        internal void TriggerWin()
        {
            _progress    = 0f;
            _maxProgress = 0f;
            _isWin       = true;
            OnWinStateChange?.Invoke();
        }

        internal void SetAnimatorDriven(bool driven)
        {
            _isAnimatorDriven = driven;
        }

        internal void ApplyDifficultySettings(float drainSpeed, float splineRadius, float winThreshold)
        {
            _drainSpeedPerSecond = drainSpeed;
            _tubeRadiusMeters    = splineRadius;
            _winThreshold        = winThreshold;
        }

        internal void SetProgress(float progress)
        {
            _progress = progress;
            OnProgressChange?.Invoke(_progress);
        }

        internal void SetMaxProgress(float maxProgress)
        {
            _maxProgress = maxProgress;
            OnMaxProgressChange?.Invoke(_maxProgress);

            if (_isWin || _maxProgress < 1f) return;
            OnWinReady?.Invoke();
        }

        #endregion

        #region Private

        private void InitSplineProximity()
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
    }
}
