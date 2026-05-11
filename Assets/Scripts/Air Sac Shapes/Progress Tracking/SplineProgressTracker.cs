using System;
using Cysharp.Threading.Tasks;
using System.Threading;
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
        internal event Action OnFirstFillComplete;
        internal event Action OnWinReady;
        internal event Action OnComplete;

        #endregion

        #region Serialized fields

        [Header("Extra")]
        [SerializeField] private MeshRenderer _tube;
        [SerializeField] private MeshRenderer _donut1;
        //[SerializeField] private MeshRenderer _donut2;

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
        [SerializeField] private float _disappearPerSecond = 3.5f;
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
        private int _winFillsRemaining;
        private float _destroyDelay;

        private SplineProximity _splineProximity;
        private CancellationTokenSource _cts;

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
            _cts = new CancellationTokenSource();
            InitSplineProximity();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
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

                if (filledProgress >= 1f)
                {
                    if (_winFillsRemaining > 0)
                    {
                        _winFillsRemaining--;
                        filledProgress = 0f;
                        OnFirstFillComplete?.Invoke();
                        Debug.Log("First fill complete!");
                        _fillUpPerSecond = _disappearPerSecond;
                    }
                    else
                    {
                        Debug.Log("Second fill complete — destroying after delay.");
                        _donut1.enabled = false;
                        //_donut2.enabled = false;
                        _tube.enabled = false;
                        DestroyAfterDelayAsync(_destroyDelay, _cts.Token).Forget();
                        return;
                    }
                }

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
                    SetCurrentProgress(newProgress);
                    SetMaxProgress(newProgress);
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

        #region Internal

        internal void SetHand(Transform handTransform, Collider handCollider)
        {
            _handTransform = handTransform;
            _handCollider  = handCollider;
            InitSplineProximity();
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
            Debug.Log("Win condition reached!");
            OnWinStateChange?.Invoke();
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

            // Signal readiness to ProgressTracker — win is triggered externally
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