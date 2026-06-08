using System;
using System.Collections.Generic;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.ProgressTracking
{
    internal class Shape : MonoBehaviour
    {
        #region Events

        internal event Action<float> OnMinProgressChange;
        internal event Action<float> OnMinMaxChange;
        internal event Action OnWinStateChange;
        internal event Action OnFirstFillComplete;
        internal event Action OnWinFillComplete;
        internal event Action OnAppearComplete;

        #endregion

        #region Serialized fields

        [SerializeField] private List<SplineProgressTracker> _trackers;
        [SerializeField] private List<TubeAnimator> _animators;

        #endregion

        #region Fields

        private float _minProgress;
        private float _minMaxProgress;
        private int _winCount;
        private int _firstFillCount;
        private int _winFillCount;
        private int _appearCount;
        private int _completeCount;

        #endregion

        #region Properties

        internal float MinProgress    => _minProgress;
        internal float MinMaxProgress => _minMaxProgress;
        internal bool IsWin           => _winCount >= _trackers.Count;
        internal IReadOnlyList<SplineProgressTracker> Trackers => _trackers;

        #endregion

        #region Setup

        private void OnEnable()
        {
            foreach (SplineProgressTracker tracker in _trackers)
            {
                tracker.OnProgressChange    += HandleProgressChange;
                tracker.OnMaxProgressChange += HandleMaxProgressChange;
                tracker.OnWinStateChange    += HandleWinStateChange;
                tracker.OnWinReady          += HandleWinReady;
            }

            foreach (TubeAnimator animator in _animators)
            {
                animator.OnFirstFillComplete += HandleFirstFillComplete;
                animator.OnWinFillComplete   += HandleWinFillComplete;
                animator.OnAppearComplete    += HandleAppearComplete;
                animator.OnComplete          += HandleComplete;
            }
        }

        private void OnDisable()
        {
            foreach (SplineProgressTracker tracker in _trackers)
            {
                tracker.OnProgressChange    -= HandleProgressChange;
                tracker.OnMaxProgressChange -= HandleMaxProgressChange;
                tracker.OnWinStateChange    -= HandleWinStateChange;
                tracker.OnWinReady          -= HandleWinReady;
            }

            foreach (TubeAnimator animator in _animators)
            {
                animator.OnFirstFillComplete -= HandleFirstFillComplete;
                animator.OnWinFillComplete   -= HandleWinFillComplete;
                animator.OnAppearComplete    -= HandleAppearComplete;
                animator.OnComplete          -= HandleComplete;
            }
        }

        #endregion

        #region Internal

        internal void RegisterDestroyDelay(float seconds)
        {
            foreach (TubeAnimator animator in _animators)
                animator.RegisterDestroyDelay(seconds);
        }

        internal void TriggerAppear()
        {
            _appearCount = 0;
            if (_animators.Count == 0)
            {
                OnAppearComplete?.Invoke();
                return;
            }
            foreach (TubeAnimator animator in _animators)
                animator.TriggerAppear();
        }

        #endregion

        #region Event handlers

        private void HandleProgressChange(float _) =>
            UpdateMinTracked(ref _minProgress, t => t.Progress, OnMinProgressChange);

        private void HandleMaxProgressChange(float _) =>
            UpdateMinTracked(ref _minMaxProgress, t => t.MaxProgress, OnMinMaxChange);

        private void HandleWinReady()
        {
            foreach (SplineProgressTracker tracker in _trackers)
                if (tracker.MaxProgress < 1f) return;

            foreach (SplineProgressTracker tracker in _trackers)
                tracker.TriggerWin();
        }

        private void HandleWinStateChange()
        {
            if (AllReached(ref _winCount))
                OnWinStateChange?.Invoke();
        }

        private void HandleFirstFillComplete()
        {
            if (AllReached(ref _firstFillCount))
                OnFirstFillComplete?.Invoke();
        }

        private void HandleWinFillComplete()
        {
            if (AllReached(ref _winFillCount))
                OnWinFillComplete?.Invoke();
        }

        private void HandleAppearComplete()
        {
            if (AllReached(ref _appearCount))
                OnAppearComplete?.Invoke();
        }

        private void HandleComplete()
        {
            if (AllReached(ref _completeCount))
                Destroy(gameObject);
        }

        #endregion

        #region Private

        private bool AllReached(ref int count) => ++count >= _trackers.Count;

        private void UpdateMinTracked(ref float stored, Func<SplineProgressTracker, float> selector, Action<float> notify)
        {
            float newMin = ComputeMin(selector);
            if (Mathf.Approximately(newMin, stored)) return;
            stored = newMin;
            notify?.Invoke(stored);
        }

        private float ComputeMin(Func<SplineProgressTracker, float> selector)
        {
            if (_trackers.Count == 0)
                return 0f;

            float min = float.MaxValue;

            for (int i = 0; i < _trackers.Count; i++)
            {
                float value = _trackers[i] != null ? selector(_trackers[i]) : 0f;
                if (value < min)
                    min = value;
            }

            return min;
        }

        #endregion
    }
}
