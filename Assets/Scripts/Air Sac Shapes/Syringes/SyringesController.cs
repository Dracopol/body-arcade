using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.Syringes
{
    internal class SyringesController : MonoBehaviour
    {
        #region Events

        internal event Action<float> OnMinProgressChange;
        internal event Action<float> OnMinMaxProgressChange;
        internal event Action OnWinStateChange;
        internal event Action OnFirstFillComplete;
        internal event Action OnWinFillComplete;
        internal event Action OnAppearComplete;
        internal event Action OnComplete;

        #endregion

        #region Serialized fields

        [Header("Tuning (shared by both sides)")]
        [SerializeField] private float _appearSpeed    = 1f;
        [SerializeField] private float _fillSpeed      = 0.5f;
        [SerializeField] private float _disappearSpeed = 3.5f;
        [SerializeField] private Color _winColor       = Color.green;

        [Header("Plungers")]
        [SerializeField] private PlungerVisual _leftPlunger;
        [SerializeField] private PlungerVisual _rightPlunger;

        #endregion

        #region Fields

        // Pure runtime helpers — no serialized state, constructed here like SplineProximity.
        private readonly PlungerTracker _leftTracker    = new();
        private readonly PlungerTracker _rightTracker   = new();
        private readonly TubeAnimator          _leftAnimator   = new();
        private readonly TubeAnimator          _rightAnimator  = new();
        private readonly TubeFillVisualizer    _leftVisualizer  = new();
        private readonly TubeFillVisualizer    _rightVisualizer = new();

        private int _activeSideCount;
        private int _winCount;
        private int _firstFillCount;
        private int _winFillCount;
        private int _appearCount;

        private float _minProgress;
        private float _minMaxProgress;
        private float _destroyDelay;

        private CancellationTokenSource _cts;

        #endregion

        #region Setup

        private void Awake()
        {
            _leftAnimator.Init(_leftTracker,   _fillSpeed, _disappearSpeed, _appearSpeed);
            _rightAnimator.Init(_rightTracker, _fillSpeed, _disappearSpeed, _appearSpeed);
            _leftVisualizer.Init(_winColor);
            _rightVisualizer.Init(_winColor);
            _cts = new CancellationTokenSource();
        }

        private void OnEnable()
        {
            // Internal wiring: animator reacts to tracker win state
            _leftTracker.OnWinStateChange  += _leftAnimator.HandleWinStateChange;
            _rightTracker.OnWinStateChange += _rightAnimator.HandleWinStateChange;

            // Internal wiring: visualizer reacts to tracker + animator
            _leftTracker.OnProgressChange      += _leftVisualizer.HandleProgressChange;
            _leftTracker.OnWinStateChange      += _leftVisualizer.HandleWinStateChange;
            _leftAnimator.OnFirstFillComplete  += _leftVisualizer.HandleFirstFillComplete;

            _rightTracker.OnProgressChange     += _rightVisualizer.HandleProgressChange;
            _rightTracker.OnWinStateChange     += _rightVisualizer.HandleWinStateChange;
            _rightAnimator.OnFirstFillComplete += _rightVisualizer.HandleFirstFillComplete;

            // Outward: tracker events
            _leftTracker.OnProgressChange    += HandleProgressChange;
            _leftTracker.OnMaxProgressChange += HandleMaxProgressChange;
            _leftTracker.OnWinStateChange    += HandleWinStateChange;
            _leftTracker.OnWinReady          += HandleWinReady;

            _rightTracker.OnProgressChange    += HandleProgressChange;
            _rightTracker.OnMaxProgressChange += HandleMaxProgressChange;
            _rightTracker.OnWinStateChange    += HandleWinStateChange;
            _rightTracker.OnWinReady          += HandleWinReady;

            // Outward: animator events
            _leftAnimator.OnFirstFillComplete += HandleFirstFillComplete;
            _leftAnimator.OnWinFillComplete   += HandleWinFillComplete;
            _leftAnimator.OnAppearComplete    += HandleAppearComplete;

            _rightAnimator.OnFirstFillComplete += HandleFirstFillComplete;
            _rightAnimator.OnWinFillComplete   += HandleWinFillComplete;
            _rightAnimator.OnAppearComplete    += HandleAppearComplete;
        }

        private void OnDisable()
        {
            _leftTracker.OnWinStateChange  -= _leftAnimator.HandleWinStateChange;
            _rightTracker.OnWinStateChange -= _rightAnimator.HandleWinStateChange;

            _leftTracker.OnProgressChange     -= _leftVisualizer.HandleProgressChange;
            _leftTracker.OnWinStateChange     -= _leftVisualizer.HandleWinStateChange;
            _leftAnimator.OnFirstFillComplete -= _leftVisualizer.HandleFirstFillComplete;

            _rightTracker.OnProgressChange     -= _rightVisualizer.HandleProgressChange;
            _rightTracker.OnWinStateChange     -= _rightVisualizer.HandleWinStateChange;
            _rightAnimator.OnFirstFillComplete -= _rightVisualizer.HandleFirstFillComplete;

            _leftTracker.OnProgressChange    -= HandleProgressChange;
            _leftTracker.OnMaxProgressChange -= HandleMaxProgressChange;
            _leftTracker.OnWinStateChange    -= HandleWinStateChange;
            _leftTracker.OnWinReady          -= HandleWinReady;

            _rightTracker.OnProgressChange    -= HandleProgressChange;
            _rightTracker.OnMaxProgressChange -= HandleMaxProgressChange;
            _rightTracker.OnWinStateChange    -= HandleWinStateChange;
            _rightTracker.OnWinReady          -= HandleWinReady;

            _leftAnimator.OnFirstFillComplete -= HandleFirstFillComplete;
            _leftAnimator.OnWinFillComplete   -= HandleWinFillComplete;
            _leftAnimator.OnAppearComplete    -= HandleAppearComplete;

            _rightAnimator.OnFirstFillComplete -= HandleFirstFillComplete;
            _rightAnimator.OnWinFillComplete   -= HandleWinFillComplete;
            _rightAnimator.OnAppearComplete    -= HandleAppearComplete;
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _leftVisualizer.Dispose();
            _rightVisualizer.Dispose();
        }

        #endregion

        #region Update

        private void Update()
        {
            _leftTracker.Tick();
            _rightTracker.Tick();
            _leftAnimator.Tick();
            _rightAnimator.Tick();
        }

        #endregion

        #region Internal

        internal void Wire(Shape shape, float drainSpeed, float splineRadius, float winThreshold)
        {
            ResetCounts();

            if (shape.LeftSpline != null)
            {
                _activeSideCount++;
                _leftTracker.ApplyDifficultySettings(drainSpeed, splineRadius, winThreshold);
                _leftTracker.AssignSpline(shape.LeftSpline, shape.LeftIsReversed);
                _leftAnimator.AssignRenderers(shape.LeftMeshRenderer, null);
                _leftVisualizer.AssignMesh(shape.LeftMeshFilter, shape.LeftMeshRenderer, shape.LeftIsReversed);
                if (_leftPlunger != null)
                {
                    _leftPlunger.SetTracker(_leftTracker, _leftTracker.SplineContainer);
                    _leftTracker.SetKnobCollider(_leftPlunger.Collider);
                }
            }

            if (shape.RightSpline != null)
            {
                _activeSideCount++;
                _rightTracker.ApplyDifficultySettings(drainSpeed, splineRadius, winThreshold);
                _rightTracker.AssignSpline(shape.RightSpline, shape.RightIsReversed);
                _rightAnimator.AssignRenderers(shape.RightMeshRenderer, null);
                _rightVisualizer.AssignMesh(shape.RightMeshFilter, shape.RightMeshRenderer, shape.RightIsReversed);
                if (_rightPlunger != null)
                {
                    _rightPlunger.SetTracker(_rightTracker, _rightTracker.SplineContainer);
                    _rightTracker.SetKnobCollider(_rightPlunger.Collider);
                }
            }
        }

        internal void SetLeftHand(Transform handTransform, Collider handCollider)
        {
            _leftTracker.SetHand(handTransform, handCollider);
        }

        internal void SetRightHand(Transform handTransform, Collider handCollider)
        {
            _rightTracker.SetHand(handTransform, handCollider);
        }

        internal void TriggerAppear()
        {
            _appearCount = 0;
            if (_activeSideCount == 0) { OnAppearComplete?.Invoke(); return; }
            if (_leftTracker.SplineContainer  != null) _leftAnimator.TriggerAppear();
            if (_rightTracker.SplineContainer != null) _rightAnimator.TriggerAppear();
        }

        internal void RegisterDestroyDelay(float seconds)
        {
            _destroyDelay = Mathf.Max(_destroyDelay, seconds);
        }

        internal void ResetState()
        {
            CancelPendingDestroy();
            ResetCounts();
            _leftTracker.ResetState();
            _rightTracker.ResetState();
            _leftAnimator.ResetState();
            _rightAnimator.ResetState();
            _leftVisualizer.AssignMesh(null, null, false);
            _rightVisualizer.AssignMesh(null, null, false);
            if (_leftPlunger  != null) _leftPlunger.SetTracker(null, null);
            if (_rightPlunger != null) _rightPlunger.SetTracker(null, null);
        }

        #endregion

        #region Private — aggregation

        private void HandleProgressChange(float _)
        {
            float newMin = ComputeMin(t => t.Progress);
            if (Mathf.Approximately(newMin, _minProgress)) return;
            _minProgress = newMin;
            OnMinProgressChange?.Invoke(newMin);
        }

        private void HandleMaxProgressChange(float _)
        {
            float newMin = ComputeMin(t => t.MaxProgress);
            if (Mathf.Approximately(newMin, _minMaxProgress)) return;
            _minMaxProgress = newMin;
            OnMinMaxProgressChange?.Invoke(newMin);
        }

        private void HandleWinReady()
        {
            if (_leftTracker.SplineContainer  != null && _leftTracker.MaxProgress  < 1f) return;
            if (_rightTracker.SplineContainer != null && _rightTracker.MaxProgress < 1f) return;

            if (_leftTracker.SplineContainer  != null) _leftTracker.TriggerWin();
            if (_rightTracker.SplineContainer != null) _rightTracker.TriggerWin();
        }

        private void HandleWinStateChange()
        {
            if (AllReached(ref _winCount)) OnWinStateChange?.Invoke();
        }

        private void HandleFirstFillComplete()
        {
            if (AllReached(ref _firstFillCount)) OnFirstFillComplete?.Invoke();
        }

        private void HandleWinFillComplete()
        {
            if (!AllReached(ref _winFillCount)) return;
            OnWinFillComplete?.Invoke();
            DestroyAfterDelayAsync(_destroyDelay, _cts.Token).Forget();
        }

        private void HandleAppearComplete()
        {
            if (AllReached(ref _appearCount)) OnAppearComplete?.Invoke();
        }

        private bool AllReached(ref int count) => ++count >= _activeSideCount;

        private float ComputeMin(Func<PlungerTracker, float> selector)
        {
            float min = float.MaxValue;
            bool  any = false;
            if (_leftTracker.SplineContainer  != null) { min = Mathf.Min(min, selector(_leftTracker));  any = true; }
            if (_rightTracker.SplineContainer != null) { min = Mathf.Min(min, selector(_rightTracker)); any = true; }
            return any ? min : 0f;
        }

        #endregion

        #region Private

        private void ResetCounts()
        {
            _activeSideCount = 0;
            _winCount        = 0;
            _firstFillCount  = 0;
            _winFillCount    = 0;
            _appearCount     = 0;
            _minProgress     = 0f;
            _minMaxProgress  = 0f;
            _destroyDelay    = 0f;
        }

        private void CancelPendingDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private async UniTaskVoid DestroyAfterDelayAsync(float delay, CancellationToken ct)
        {
            if (delay > 0f)
                await UniTask.WaitForSeconds(delay, cancellationToken: ct);
            if (!ct.IsCancellationRequested)
                OnComplete?.Invoke();
        }

        #endregion
    }
}
