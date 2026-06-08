using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.ProgressTracking
{
    internal class TubeAnimator : MonoBehaviour
    {
        #region Events

        internal event Action OnFirstFillComplete;
        internal event Action OnWinFillComplete;
        internal event Action OnAppearComplete;
        internal event Action OnComplete;

        #endregion

        #region Serialized fields

        [Header("References")]
        [SerializeField] private SplineProgressTracker _tracker;
        [SerializeField] private MeshRenderer _tube;
        [SerializeField] private MeshRenderer _knob;

        [Header("Behaviour")]
        [SerializeField] private float _fillSpeedPerSecond      = 0.5f;
        [SerializeField] private float _disappearSpeedPerSecond = 3.5f;
        [SerializeField] private float _appearSpeedPerSecond    = 1f;

        #endregion

        #region Fields

        private float _activeFillSpeed;
        private bool  _isWin;
        private bool  _isWinFillComplete;
        private bool  _isDisappearing;
        private bool  _isAppearing;
        private int   _winFillsRemaining;
        private float _destroyDelay;

        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region Properties

        internal SplineProgressTracker Tracker => _tracker;

        #endregion

        #region Setup

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _activeFillSpeed         = _fillSpeedPerSecond;
        }

        private void OnEnable()
        {
            if (_tracker != null)
                _tracker.OnWinStateChange += HandleWinStateChange;
        }

        private void OnDisable()
        {
            if (_tracker != null)
                _tracker.OnWinStateChange -= HandleWinStateChange;
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
            if (_isAppearing)
            {
                float appeared = Mathf.MoveTowards(_tracker.Progress, 1f, Time.deltaTime * _appearSpeedPerSecond);
                _tracker.SetProgress(appeared);
                if (_tracker.Progress >= 1f)
                {
                    _isAppearing = false;
                    _tracker.SetAnimatorDriven(false);
                    _tracker.SetProgress(0f);
                    _tracker.SetMaxProgress(0f);
                    OnAppearComplete?.Invoke();
                }
                return;
            }

            if (!_isWin) return;

            if (_isDisappearing)
            {
                float disappeared = Mathf.MoveTowards(_tracker.Progress, 0f, Time.deltaTime * _disappearSpeedPerSecond);
                _tracker.SetProgress(disappeared);
                if (_tracker.Progress <= 0f)
                {
                    _isDisappearing = false;
                    if (_knob != null) _knob.enabled = false;
                    if (_tube != null) _tube.enabled = false;
                    OnWinFillComplete?.Invoke();
                    DestroyAfterDelayAsync(_destroyDelay, _cancellationTokenSource.Token).Forget();
                }
                return;
            }

            float filledProgress = Mathf.MoveTowards(_tracker.Progress, 1f, Time.deltaTime * _activeFillSpeed);

            if (filledProgress >= 1f)
            {
                if (_winFillsRemaining > 0)
                {
                    _winFillsRemaining--;
                    filledProgress   = 0f;
                    _activeFillSpeed = _disappearSpeedPerSecond;
                    _tracker.SetProgress(filledProgress);
                    _tracker.SetMaxProgress(filledProgress);
                    OnFirstFillComplete?.Invoke();
                    return;
                }

                if (_isWinFillComplete) return;
                _isWinFillComplete = true;
                _isDisappearing    = true;
                _tracker.SetProgress(1f);
                return;
            }

            _tracker.SetProgress(filledProgress);
            _tracker.SetMaxProgress(filledProgress);
        }

        #endregion

        #region Internal

        internal void TriggerAppear()
        {
            _isAppearing = true;
            _tracker.SetAnimatorDriven(true);
            _tracker.SetProgress(0f);
            _tracker.SetMaxProgress(0f);
        }

        internal void RegisterDestroyDelay(float seconds)
        {
            _destroyDelay = Mathf.Max(_destroyDelay, seconds);
        }

        #endregion

        #region Event handlers

        private void HandleWinStateChange()
        {
            _isWin             = true;
            _winFillsRemaining = 1;
            _activeFillSpeed   = _fillSpeedPerSecond;
        }

        #endregion

        #region Private

        private async UniTaskVoid DestroyAfterDelayAsync(float delay, CancellationToken ct)
        {
            if (delay > 0f)
                await UniTask.WaitForSeconds(delay, cancellationToken: ct);

            if (!ct.IsCancellationRequested)
            {
                OnComplete?.Invoke();
                Destroy(gameObject);
            }
        }

        #endregion
    }
}
