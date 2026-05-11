using System;
using System.Collections.Generic;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.ProgressTrackering
{
    internal class ProgressTracker : MonoBehaviour
    {
        #region Events

        internal event Action<float> OnMinMaxChanged;
        internal event Action OnWinStateChange;
        internal event Action OnFirstFillComplete;

        #endregion

        #region Serialized fields

        [SerializeField] private List<SplineProgressTracker> _trackers;

        #endregion

        #region Fields

        private float _minMax;
        private int _winCount;
        private int _firstFillCount;

        #endregion

        #region Properties

        internal float MinMax => _minMax;
        internal IReadOnlyList<SplineProgressTracker> Trackers => _trackers;

        #endregion

        #region Setup

        private void OnEnable()
        {
            foreach (SplineProgressTracker tracker in _trackers)
            {
                tracker.OnMaxProgressChange += HandleMaxProgressChange;
                tracker.OnWinStateChange    += HandleWinStateChange;
                tracker.OnFirstFillComplete += HandleFirstFillComplete;
            }
        }

        private void OnDisable()
        {
            foreach (SplineProgressTracker tracker in _trackers)
            {
                tracker.OnMaxProgressChange -= HandleMaxProgressChange;
                tracker.OnWinStateChange    -= HandleWinStateChange;
                tracker.OnFirstFillComplete -= HandleFirstFillComplete;
            }
        }

        #endregion

        #region Event handlers

        private void HandleMaxProgressChange(float _)
        {
            float newMinMax = ComputeMinMax();

            if (Mathf.Approximately(newMinMax, _minMax))
                return;

            _minMax = newMinMax;
            OnMinMaxChanged?.Invoke(_minMax);
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

        private float ComputeMinMax()
        {
            if (_trackers.Count == 0)
                return 0f;

            float min = float.MaxValue;

            for (int i = 0; i < _trackers.Count; i++)
            {
                float max = _trackers[i] != null ? _trackers[i].MaxProgress : 0f;
                if (max < min)
                    min = max;
            }

            return min;
        }

        #endregion
    }
}