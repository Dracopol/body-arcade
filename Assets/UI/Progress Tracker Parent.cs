using System;
using System.Collections.Generic;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.ProgressTrackering
{
    internal class ProgressTracker : MonoBehaviour
    {
        #region Events

        internal event Action<float> OnMinProgressChanged;
        internal event Action<float> OnMinMaxChanged;
        internal event Action OnWinStateChange;
        internal event Action OnFirstFillComplete;

        #endregion

        #region Serialized fields

        [SerializeField] private List<SplineProgressTracker> _trackers;

        #endregion

        #region Fields

        private float _minProgress;
        private float _minMax;
        private int _winCount;
        private int _firstFillCount;
        private int _winReadyCount;

        #endregion

        #region Properties

        internal float MinProgress => _minProgress;
        internal float MinMax => _minMax;
        internal bool IsWin => _winCount >= _trackers.Count;
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
                tracker.OnFirstFillComplete += HandleFirstFillComplete;
                tracker.OnWinReady          += HandleWinReady;
            }
        }

        private void OnDisable()
        {
            foreach (SplineProgressTracker tracker in _trackers)
            {
                tracker.OnProgressChange    -= HandleProgressChange;
                tracker.OnMaxProgressChange -= HandleMaxProgressChange;
                tracker.OnWinStateChange    -= HandleWinStateChange;
                tracker.OnFirstFillComplete -= HandleFirstFillComplete;
                tracker.OnWinReady          -= HandleWinReady;
            }
        }

        #endregion

        #region Internal

        internal void RegisterDestroyDelay(float seconds)
        {
            foreach (SplineProgressTracker tracker in _trackers)
                tracker.RegisterDestroyDelay(seconds);
        }

        #endregion

        #region Event handlers

        private void HandleProgressChange(float _)
        {
            float newMin = ComputeMin(t => t.Progress);

            if (Mathf.Approximately(newMin, _minProgress))
                return;

            _minProgress = newMin;
            OnMinProgressChanged?.Invoke(_minProgress);
        }

        private void HandleMaxProgressChange(float _)
        {
            float newMinMax = ComputeMin(t => t.MaxProgress);

            if (Mathf.Approximately(newMinMax, _minMax))
                return;

            _minMax = newMinMax;
            OnMinMaxChanged?.Invoke(_minMax);
        }

        private void HandleWinReady()
        {
            _winReadyCount++;

            if (_winReadyCount < _trackers.Count)
                return;

            // All trackers have reached max progress — trigger win on all simultaneously
            foreach (SplineProgressTracker tracker in _trackers)
                tracker.TriggerWin();
        }

        private void HandleWinStateChange()
        {
            _winCount++;

            if (_winCount >= _trackers.Count)
                OnWinStateChange?.Invoke();
        }

        private void HandleFirstFillComplete()
        {
            _firstFillCount++;

            if (_firstFillCount >= _trackers.Count)
                OnFirstFillComplete?.Invoke();
        }

        #endregion

        #region Private

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