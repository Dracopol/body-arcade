using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes.ProgressTracking
{
    internal class SplineProgressTracker : MonoBehaviour
    {
        #region Events

        internal event Action<float> OnProgressChange;
        internal event Action<float> OnMaxProgressChange;
        internal event Action OnWinStateChange;
        internal event Action OnFirstFillComplete;
        internal event Action OnWinFillComplete;
        internal event Action OnAppearComplete;
        internal event Action OnWinReady;
        internal event Action OnComplete;

        #endregion

        #region Serialized fields

        [Header("Extra")]
        [SerializeField] private MeshRenderer _tube;
        [SerializeField] private MeshRenderer _knob;

        [Header("References")]
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private Transform _handTransform;
        [SerializeField] private Collider _handCollider;
        [SerializeField] private Collider _knobCollider;

        [Header("Behaviour")]
        [SerializeField] private float _radiusToleranceMultiplier = 1.5f;
        [SerializeField] private float _tubeRadiusMeters = 0.05f;
        [SerializeField] private bool _isReversed = false;
        [SerializeField] private float _drainSpeedPerSecond = 5f;
        [SerializeField] private float _fillSpeedPerSecond = 0.5f;
        [SerializeField] private float _disappearSpeedPerSecond = 3.5f;
        [SerializeField] private float _appearSpeedPerSecond = 1f;
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
        private float _activeFillSpeed;
        private bool _isWin;
        private bool _isWinFillComplete;
        private bool _isDisappearing;
        private bool _isAppearing;
        private int _winFillsRemaining;
        private float _destroyDelay;

        private SplineProximity _splineProximity;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region Properties

        internal float Progress => _progress;
        internal float MaxProgress => _maxProgress;
        internal bool IsWin => _isWin;
        internal bool IsReversed => _isReversed;
        internal SplineContainer SplineContainer => _splineContainer;

        #endregion

        #region Setup

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _activeFillSpeed = _fillSpeedPerSecond;
            InitSplineProximity();
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
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

            if (_isAppearing)
            {
                float appeared = Mathf.MoveTowards(_progress, 1f, Time.deltaTime * _appearSpeedPerSecond);
                SetCurrentProgress(appeared);
                if (_progress >= 1f)
                {
                    _isAppearing = false;
                    SetCurrentProgress(0f);
                    SetMaxProgress(0f);
                    OnAppearComplete?.Invoke();
                }
                return;
            }

            if (_isWin)
            {
                if (_isDisappearing)
                {
                    float disappeared = Mathf.MoveTowards(_progress, 0f, Time.deltaTime * _disappearSpeedPerSecond);
                    SetCurrentProgress(disappeared);
                    if (_progress <= 0f)
                    {
                        _isDisappearing = false;
                        if (_knob != null) _knob.enabled = false;
                        if (_tube != null) _tube.enabled = false;
                        OnWinFillComplete?.Invoke();
                        DestroyAfterDelayAsync(_destroyDelay, _cancellationTokenSource.Token).Forget();
                    }
                    return;
                }

                float filledProgress = Mathf.MoveTowards(_progress, 1f, Time.deltaTime * _activeFillSpeed);

                if (filledProgress >= 1f)
                {
                    if (_winFillsRemaining > 0)
                    {
                        _winFillsRemaining--;
                        filledProgress = 0f;
                        OnFirstFillComplete?.Invoke();
                        _activeFillSpeed = _disappearSpeedPerSecond;
                    }
                    else
                    {
                        if (_isWinFillComplete) return;
                        _isWinFillComplete = true;
                        _isDisappearing    = true;
                        SetCurrentProgress(1f);
                        return;
                    }
                }

                SetCurrentProgress(filledProgress);
                SetMaxProgress(filledProgress);
                return;
            }

            if (_handTransform == null || _splineProximity == null)
                return;

            float splineProgress = _splineProximity.FindClosestSplineProgress(
                out Vector3 closestSplinePoint,
                out Vector3 closestColliderPoint);

            if (_isReversed)
                splineProgress = 1f - splineProgress;

            float distanceToSplineMeters = (closestSplinePoint - closestColliderPoint).magnitude;
            bool isHandInside = distanceToSplineMeters <= _tubeRadiusMeters * _radiusToleranceMultiplier;

            _debugIsHandInside = isHandInside;

            if (!isHandInside)
            {
                float drained = Mathf.Lerp(_progress, 0f, Time.deltaTime * _drainSpeedPerSecond);
                SetCurrentProgress(drained);
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
                SetCurrentProgress(newProgress);
                SetMaxProgress(newProgress);
            }
            else if (splineProgress < _maxProgress)
            {
                SetCurrentProgress(splineProgress);
            }
            else
            {
                float drained = Mathf.Lerp(_progress, 0f, Time.deltaTime * _drainSpeedPerSecond);
                SetCurrentProgress(drained);
                SetMaxProgress(drained);
            }
        }

        #endregion

        #region Internal

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

        internal void RegisterDestroyDelay(float seconds)
        {
            _destroyDelay = Mathf.Max(_destroyDelay, seconds);
        }

        internal void TriggerWin()
        {
            _progress          = 0f;
            _maxProgress       = 0f;
            _winFillsRemaining = 1;
            _isWin             = true;
            OnWinStateChange?.Invoke();
        }

        internal void ApplyDifficultySettings(float drainSpeed, float splineRadius)
        {
            _drainSpeedPerSecond = drainSpeed;
            _tubeRadiusMeters    = splineRadius;
        }

        internal void TriggerAppear()
        {
            _progress    = 0f;
            _maxProgress = 0f;
            _isAppearing = true;
            SetCurrentProgress(0f);
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

        private void SetCurrentProgress(float progress)
        {
            _progress      = progress;
            _debugProgress = progress;
            OnProgressChange?.Invoke(_progress);
        }

        private void SetMaxProgress(float maxProgress)
        {
            _maxProgress      = maxProgress;
            _debugMaxProgress = maxProgress;
            OnMaxProgressChange?.Invoke(_maxProgress);

            if (_isWin || _maxProgress < 1f)
                return;

            OnWinReady?.Invoke();
        }

        private async UniTaskVoid DestroyAfterDelayAsync(float delay, CancellationToken ct)
        {
            if (delay > 0f)
                await UniTask.WaitForSeconds(delay, cancellationToken: ct);

            if (!ct.IsCancellationRequested)
            {
                // Fire before destroying so ProgressTracker can react
                OnComplete?.Invoke();
                Destroy(gameObject);
            }
        }

        #endregion
    }
}